using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Musiccast.App.Helpers;
using Musiccast.App.Models;
using Musiccast.App.Services;
using Musiccast.App.Views;
using Musiccast.Model;
using Musiccast.Service;

namespace Musiccast.App.ViewModels
{
    public partial class ZonesViewModel : ObservableObject, IDisposable
    {
        private const string DevicesKey = "DEVICES2";

        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private readonly NavigationService _navigationService;
        private readonly MusicCastService _service;
        private bool _disposed;

        public ObservableCollection<Device> Devices { get; } = new();

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        public ZonesViewModel(MusicCastService service, NavigationService navigationService)
        {
            _service = service;
            _navigationService = navigationService;
            _service.DeviceFound += Service_DeviceFoundAsync;
            App.UDPNotificationReceived += UDPListener_DeviceNotificationReceivedAsync;

            _ = InitAsync();
        }

        [RelayCommand]
        private async Task AddDevice() => await FindNewDevicesAsync();

        [RelayCommand]
        private async Task ClearDevices()
        {
            Enqueue(async () =>
            {
                IsLoading = true;
                Devices.Clear();
                await SaveDevicesAsync();
                IsLoading = false;
            });
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void ViewDeviceDetail(Device? device)
        {
            if (device != null)
            {
                _navigationService.Navigate(typeof(DeviceDetailPage), device);
            }
        }

        [RelayCommand]
        private void CancelSearch() => IsLoading = false;

        public async Task InitAsync()
        {
            var stored = await LoadDevicesFromStorageAsync();

            Enqueue(async () =>
            {
                Devices.Clear();
                foreach (var item in stored)
                {
                    Devices.Add(item);
                }

                await RefreshDevicesAsync();
                await SaveDevicesAsync();
            });
        }

        private async Task FindNewDevicesAsync()
        {
            try
            {
                IsLoading = true;
                await _service.LoadRoomsAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDevicesAsync()
        {
            if (!Devices.Any())
            {
                return;
            }

            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                var device = Devices.Count > i ? Devices[i] : null;
                if (device == null || string.IsNullOrEmpty(device.BaseUri))
                {
                    continue;
                }

                MusicCastDevice? updated = null;
                try
                {
                    updated = await _service.RefreshDeviceAsync(device.Id, new Uri(device.BaseUri!), device.Zone!);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                Enqueue(() =>
                {
                    if (updated == null)
                    {
                        device.IsAlive = false;
                    }
                    else
                    {
                        device.Power = updated.Power;
                        device.Input = updated.Input.ToString();
                        device.SubTitle = updated.NowPlayingInformation;
                        device.IsAlive = true;
                        device.ImageUri = UriHelper.ResolvePath(updated.Location, updated.ImagePath);
                        device.FriendlyName = string.IsNullOrEmpty(updated.FriendlyName) ? device.FriendlyName : updated.FriendlyName;
                        HookPowerToggle(device);
                    }
                });
            }
        }

        private void HookPowerToggle(Device device)
        {
            device.PowerToggled -= Device_PowerToggledAsync;
            device.PowerToggled += Device_PowerToggledAsync;
        }

        private async void Device_PowerToggledAsync(object? sender, Device e)
        {
            try
            {
                await _service.TogglePowerAsync(new Uri(e.BaseUri!), e.Zone!);
                await RefreshDevicesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void Service_DeviceFoundAsync(object? sender, MusicCastDevice device)
        {
            Enqueue(() =>
            {
                IsLoading = false;

                if (Devices.Where(w => w != null).Select(s => s.FriendlyName).Contains(device.FriendlyName))
                {
                    return;
                }

                var newDevice = new Device
                {
                    Id = device.Id,
                    BaseUri = device.BaseUri?.ToString(),
                    Zone = device.Zone,
                    FriendlyName = device.FriendlyName,
                    ImageUri = UriHelper.ResolvePath(device.Location, device.ImagePath),
                    ImageSize = device.ImageSize,
                    Power = device.Power,
                    Input = device.Input.ToString(),
                    SubTitle = device.NowPlayingInformation,
                    IsAlive = true
                };
                HookPowerToggle(newDevice);
                Devices.Add(newDevice);
            });

            await SaveDevicesAsync();
        }

        private async void UDPListener_DeviceNotificationReceivedAsync(object? sender, string deviceId)
        {
            var existing = Devices.Where(w => w.Id != null && w.Id.Equals(deviceId)).ToList();
            if (existing.Count == 0)
            {
                return;
            }

            foreach (var device in existing)
            {
                MusicCastDevice? updated;
                try
                {
                    updated = await _service.RefreshDeviceAsync(device.Id, new Uri(device.BaseUri!), device.Zone!);
                }
                catch
                {
                    continue;
                }

                if (updated == null)
                {
                    continue;
                }

                Enqueue(() =>
                {
                    device.Power = updated.Power;
                    device.Input = updated.Input.ToString();
                    device.SubTitle = updated.NowPlayingInformation;
                    device.IsAlive = true;
                    device.ImageUri = UriHelper.ResolvePath(updated.Location, updated.ImagePath);
                    device.FriendlyName = string.IsNullOrEmpty(updated.FriendlyName) ? device.FriendlyName : updated.FriendlyName;
                });
            }
        }

        private async Task<List<Device>> LoadDevicesFromStorageAsync()
        {
            var devices = await SettingsStorage.ReadAsync<List<Device>>(DevicesKey);
            if (devices != null)
            {
                foreach (var item in devices)
                {
                    item.IsAlive = false;
                }
            }

            return devices ?? new List<Device>();
        }

        private async Task SaveDevicesAsync() => await SettingsStorage.SaveAsync(DevicesKey, Devices.ToList());

        private void Enqueue(Action action) => _dispatcherQueue.TryEnqueue(() => action());

        private void Enqueue(Func<Task> action) => _dispatcherQueue.TryEnqueue(async () => await action());

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _service.DeviceFound -= Service_DeviceFoundAsync;
            App.UDPNotificationReceived -= UDPListener_DeviceNotificationReceivedAsync;
            _disposed = true;
        }
    }
}
