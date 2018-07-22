using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using App4.Helpers;
using App4.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Musiccast.Model;
using Musiccast.Models;
using Musiccast.Service;
using Windows.Storage;

namespace App4.ViewModels
{
    public class ZonesViewModel : ViewModelBase
    {
        private const string DevicesKey = "DEVICES";
        private readonly NavigationServiceEx navigationService;
        private MusicCastService service;
        public ObservableCollection<Device> Devices { get; set; }
        public Timer refreshTimer;

        public Device SelectedDevice { get; set; }

        public ICommand AddDeviceCommand
        {
            get
            {
                return new RelayCommand(async () => { await FindNewDevices(); });
            }
        }

        public ICommand ViewDeviceDetailCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    navigationService.Navigate(typeof(DeviceDetailPageViewModel).FullName, this.SelectedDevice);
                });
            }
        }

        public ZonesViewModel(NavigationServiceEx navigationService)
        {
            this.navigationService = navigationService;
            Devices = new ObservableCollection<Device>();

            if (IsInDesignMode)
            {
                Devices.Add(new Device() { FriendlyName = "Test" });
            }
            else
            {
                refreshTimer = new Timer(RefreshDevicesAsync, null, 10000, 10000);
            }
        }

        private async void RefreshDevicesAsync(object state)
        {
            if (Devices == null || Devices.Count <= 0)
                return;

            foreach (var e in Devices.AsParallel())
            {
                var updatedDevice = await service.RefreshDeviceAsync(e.Id, e.Zone);

                await DispatcherHelper.RunAsync(() =>
                {
                    e.Power = updatedDevice.Power;
                    e.Input = updatedDevice.Input.ToString();
                    e.SubTitle = updatedDevice.NowPlayingInformation;
                });
            }
        }

        public async Task InitAsync()
        {
            Devices.Clear();
            var temp = await LoadDevicesFromStorageAsync();
            foreach (var item in temp.AsParallel())
            {
                if (temp != null)
                {
                    if (service == null)
                        service = new MusicCastService();

                    var updatedDevice = await service.RefreshDeviceAsync(item.Id, item.Zone);
                    item.Power = updatedDevice.Power;
                    item.Input = updatedDevice.Input.ToString();
                    item.SubTitle = updatedDevice.NowPlayingInformation;

                    if (string.IsNullOrEmpty(item.FriendlyName))
                        item.FriendlyName = "UNKNOWN";

                    item.PowerToggled += async (s, e) => { await Item_PowerToggledAsync(s, e); };
                    Devices.Add(item);
                }
            }
        }

        private async Task Item_PowerToggledAsync(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(e.Id, e.Zone);
            RefreshDevicesAsync(null);
        }

        public async Task FindNewDevices()
        {
            service = new MusicCastService();
            service.DeviceFound += Service_DeviceFoundAsync;
            await service.LoadRoomsAsync();
        }

        private async void Service_DeviceFoundAsync(object sender, MusicCastDevice device)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (Devices.Where(w => w != null).Select(s => s.Id + s.Zone).Contains(device.Id + device.Zone))
                    return;

                Devices.Add(new Device()
                {
                    Id = device.Id,
                    Zone = device.Zone,
                    FriendlyName = device.FriendlyName,
                        //ImageUri = new Uri(new Uri(new Uri(device.Location).GetLeftPart(UriPartial.Authority)), device.ImagePath),
                        ImageSize = device.ImageSize,
                    Power = device.Power,
                    Input = device.Input.ToString(),
                    SubTitle = device.NowPlayingInformation
                });
            });
            await SaveDevicesInStorageAsync(Devices.ToList());
        }

        private static async Task<List<Device>> LoadDevicesFromStorageAsync()
        {
            var devices = await ApplicationData.Current.LocalSettings.ReadAsync<List<Device>>(DevicesKey);
            return devices ?? new List<Device>();
        }

        private static async Task SaveDevicesInStorageAsync(List<Device> devices)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(DevicesKey, devices);
        }
    }
}

