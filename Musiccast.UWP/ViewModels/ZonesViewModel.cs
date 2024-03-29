﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using App4.Helpers;
using App4.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp;
using Musiccast.Helpers;
using Musiccast.Model;
using Musiccast.Models;
using Musiccast.Service;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;

namespace App4.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class ZonesViewModel : ViewModelBase, IDisposable
    {
        private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private const string DevicesKey = "DEVICES2";
        private readonly NavigationServiceEx navigationService;
        private readonly ILogger<ZonesViewModel> logger;
        private MusicCastService service;
        public ObservableCollection<Device> Devices { get; set; }
        public ICommand AddDeviceCommand
        {
            get
            {
                return new RelayCommand(async () => { await FindNewDevices().ConfigureAwait(false); });
            }
        }

        public ICommand ClearDeviceCommand
        {
            get
            {
                return new RelayCommand(async () => { await ClearDevices().ConfigureAwait(false); });
            }
        }



        public ICommand ViewDeviceDetailCommand
        {
            get
            {
                return new RelayCommand<Device>((device) =>
                {
                    navigationService.Navigate(typeof(DeviceDetailPageViewModel).FullName, device);
                });
            }
        }

        public ICommand CancelSearchCommand
        {
            get
            {
                return new RelayCommand(async () => { await CancelFindNewDevices().ConfigureAwait(false); });
            }
        }

        private bool _isLoading;
        private bool disposedValue;
        private bool registeredNetworkStatusNotif;
        private bool hasNetwork;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public bool HasNetwork { get => hasNetwork; set => Set(ref hasNetwork, value); }

        public ZonesViewModel(NavigationServiceEx navigationService)
        {
            Devices = new ObservableCollection<Device>();

            this.navigationService = navigationService;
            this.logger = App.ServiceProvider.GetService(typeof(ILogger<ZonesViewModel>)) as ILogger<ZonesViewModel>;
            this.service = App.ServiceProvider.GetService(typeof(MusicCastService)) as MusicCastService;
            this.service.DeviceFound += Service_DeviceFoundAsync;

            _ = InitAsync();
        }

        private void NetworkStatusChange()
        {
            // register for network status change notifications
            try
            {
                var networkStatusCallback = new NetworkStatusChangedEventHandler(OnNetworkStatusChange);
                if (!registeredNetworkStatusNotif)
                {
                    NetworkInformation.NetworkStatusChanged += networkStatusCallback;
                    registeredNetworkStatusNotif = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Unexpected exception occured: " + ex.ToString());
            }
        }

        async void OnNetworkStatusChange(object sender)
        {
            try
            {
                // get the ConnectionProfile that is currently used to connect to the Internet                
                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

                if (InternetConnectionProfile == null)
                {
                    await dispatcherQueue.EnqueueAsync(() => { HasNetwork = false; });
                }
                else
                {
                    await dispatcherQueue.EnqueueAsync(() => { HasNetwork = InternetConnectionProfile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.LocalAccess; });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Unexpected exception occured: " + ex.ToString());
            }
        }

        private async void UDPListener_DeviceNotificationRecievedAsync(object sender, string deviceId)
        {
            logger.LogInformation("Listening");

            var existingDevices = Devices.Where(w => w.Id.Equals(deviceId));
            if (existingDevices == null || existingDevices.Count() <= 0)
                return;

            foreach (var existingDevice in existingDevices)
            {
                var updatedDevice = await service.RefreshDeviceAsync(existingDevice.Id, new Uri(existingDevice.BaseUri), existingDevice.Zone).ConfigureAwait(false);
                if (updatedDevice == null)
                    continue;

                await dispatcherQueue.EnqueueAsync(() =>
                {
                    try
                    {
                        existingDevice.Power = updatedDevice.Power;
                        existingDevice.Input = updatedDevice.Input.ToString();
                        existingDevice.SubTitle = updatedDevice.NowPlayingInformation;
                        existingDevice.IsAlive = true;
                        existingDevice.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                        existingDevice.ImageSize = existingDevice.ImageSize;
                        existingDevice.FriendlyName = string.IsNullOrEmpty(updatedDevice.FriendlyName) ? existingDevice.FriendlyName : updatedDevice.FriendlyName;

                        if (string.IsNullOrEmpty(existingDevice.FriendlyName))
                            existingDevice.FriendlyName = ResourceHelper.GetString("DeviceName_Unknown");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                });
            }
        }

        private async Task RefreshDevicesAsync(object state)
        {
            try
            {
                if (!Devices.Any())
                    return;

                logger.LogInformation("Refreshing");

                for (int i = Devices.Count - 1; i >= 0; i--)
                {
                    Device e = Devices.Count >= i ? Devices[i] : null;
                    if (e == null || e.BaseUri == null)
                        continue;

                    var updatedDevice = await service.RefreshDeviceAsync(e.Id, new Uri(e.BaseUri), e.Zone).ConfigureAwait(false);
                    if (updatedDevice == null)
                        continue;

                    await dispatcherQueue.EnqueueAsync(() =>
                    {
                        try
                        {
                            if (updatedDevice == null)
                            {
                                if (Devices.Any() && Devices.Count > i)
                                    Devices.RemoveAt(i);
                            }
                            else
                            {
                                e.Power = updatedDevice.Power;
                                e.Input = updatedDevice.Input.ToString();
                                e.SubTitle = updatedDevice.NowPlayingInformation;
                                e.IsAlive = true;
                                e.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                                e.ImageSize = e.ImageSize;
                                e.FriendlyName = string.IsNullOrEmpty(updatedDevice.FriendlyName) ? e.FriendlyName : updatedDevice.FriendlyName;

                                if (string.IsNullOrEmpty(e.FriendlyName))
                                    e.FriendlyName = ResourceHelper.GetString("DeviceName_Unknown");

                                e.PowerToggled -= async (s, args) => { await Item_PowerToggledAsync(s, args); };
                                e.PowerToggled += async (s, args) => { await Item_PowerToggledAsync(s, args); };
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Exception while refreshing");
                        }
                        finally
                        {
                            IsLoading = false;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Errors.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, e.ToString());
            }
        }

        public async Task InitAsync()
        {
            logger.LogInformation("Init async");

            var temp = await LoadDevicesFromStorageAsync().ConfigureAwait(false);

            App.UDPNotificationReceived += UDPListener_DeviceNotificationRecievedAsync;

            await dispatcherQueue.EnqueueAsync(async () =>
            {
                Devices.Clear();

                foreach (var item in temp)
                {
                    if (temp != null)
                    {
                        Devices.Add(item);
                    }
                }

                await RefreshDevicesAsync(null).ConfigureAwait(false);
                await SaveDevicesInStorageAsync(Devices.ToList()).ConfigureAwait(false);
            });
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private IPAddress AddressToIPAddress(string addr)
        {
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            var address = addr.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ? IPAddress.Parse(addr.Substring(7, 13)) : IPAddress.Parse(addr);
            return address;
        }

        private async Task Item_PowerToggledAsync(object sender, Device e)
        {
            await service.TogglePowerAsync(new Uri(e.BaseUri), e.Zone).ConfigureAwait(false);
            await RefreshDevicesAsync(null).ConfigureAwait(false);
        }

        private async Task FindNewDevices()
        {
            await dispatcherQueue.EnqueueAsync(async () =>
            {
                try
                {
                    IsLoading = true;
                    await service.LoadRoomsAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Finding devices failed.");
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private async Task ClearDevices()
        {
            await dispatcherQueue.EnqueueAsync(async () =>
            {
                try
                {
                    IsLoading = true;
                    Devices.Clear();
                    await SaveDevicesInStorageAsync(Devices.ToList());
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Finding devices failed.");
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private async Task CancelFindNewDevices()
        {
            await dispatcherQueue.EnqueueAsync(() =>
            {
                IsLoading = false;
            });
        }

        private async void Service_DeviceFoundAsync(object sender, MusicCastDevice device)
        {
            await dispatcherQueue.EnqueueAsync(() =>
            {
                IsLoading = false;

                if (Devices == null || Devices.Where(w => w != null).Select(s => s.FriendlyName).Contains(device.FriendlyName))
                    return;

                Devices.Add(new Device()
                {
                    Id = device.Id,
                    BaseUri = device.BaseUri.ToString(),
                    Zone = device.Zone,
                    FriendlyName = device.FriendlyName,
                    ImageUri = UriHelper.ResolvePath(device.Location, device.ImagePath),
                    ImageSize = device.ImageSize,
                    Power = device.Power,
                    Input = device.Input.ToString(),
                    SubTitle = device.NowPlayingInformation,
                    IsAlive = true
                });
            });

            await SaveDevicesInStorageAsync(Devices.ToList()).ConfigureAwait(false);
        }

        private async Task<List<Device>> LoadDevicesFromStorageAsync()
        {
            try
            {
                var devices = await ApplicationData.Current.LocalSettings.ReadAsync<List<Device>>(DevicesKey).ConfigureAwait(false);

                if (devices != null)
                    foreach (var item in devices)
                    {
                        item.IsAlive = false;
                    }

                return devices ?? new List<Device>();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Loading devices from storage failed");
                return new List<Device>(0);
            }
        }

        private async Task SaveDevicesInStorageAsync(List<Device> devices)
        {
            try
            {
                await ApplicationData.Current.LocalSettings.SaveAsync(DevicesKey, devices).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Save devices to storage failed");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (service != null)
                        service.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

