using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using App4.Helpers;
using App4.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Musiccast.Helpers;
using Musiccast.Model;
using Musiccast.Models;
using Musiccast.Service;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace App4.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class ZonesViewModel : ViewModelBase
    {
        /// <summary>
        /// The devices key
        /// </summary>
        private const string DevicesKey = "DEVICES";
        /// <summary>
        /// The navigation service
        /// </summary>
        private readonly NavigationServiceEx navigationService;
        /// <summary>
        /// The service
        /// </summary>
        private MusicCastService service;
        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public ObservableCollection<Device> Devices { get; set; }
        /// <summary>
        /// The refresh timer
        /// </summary>
        public Timer refreshTimer;

        /// <summary>
        /// Gets the add device command.
        /// </summary>
        /// <value>
        /// The add device command.
        /// </value>
        public ICommand AddDeviceCommand
        {
            get
            {
                return new RelayCommand(async () => { await FindNewDevices(); });
            }
        }

        /// <summary>
        /// Gets the view device detail command.
        /// </summary>
        /// <value>
        /// The view device detail command.
        /// </value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonesViewModel"/> class.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
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
                refreshTimer = new Timer(async (e) => { await RefreshDevicesAsync(e); }, null, 10000, 10000);
            }
        }

        /// <summary>
        /// Refreshes the devices asynchronous.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private async Task RefreshDevicesAsync(object state)
        {
            if (Devices == null || Devices.Count <= 0)
                return;

            if (service == null)
                service = new MusicCastService();

            foreach (var e in Devices.AsParallel())
            {
                if (e == null || e.BaseUri == null)
                    continue;

                var updatedDevice = await service.RefreshDeviceAsync(e.Id, new Uri(e.BaseUri), e.Zone);

                await DispatcherHelper.RunAsync(() =>
                {
                    e.Power = updatedDevice.Power;
                    e.Input = updatedDevice.Input.ToString();
                    e.SubTitle = updatedDevice.NowPlayingInformation;
                    e.BackGround = new SolidColorBrush(Colors.OrangeRed);
                    e.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                    e.ImageSize = e.ImageSize;

                    if (string.IsNullOrEmpty(e.FriendlyName))
                        e.FriendlyName = ResourceHelper.GetString("DeviceName_Unknown");

                    e.PowerToggled -= async (s, args) => { await Item_PowerToggledAsync(s, args); };
                    e.PowerToggled += async (s, args) => { await Item_PowerToggledAsync(s, args); };
                });
            }
        }

        /// <summary>
        /// Initializes the asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            Devices.Clear();

            var temp = await LoadDevicesFromStorageAsync();
            foreach (var item in temp)
            {
                if (temp != null)
                {
                    Devices.Add(item);
                   // await new UDPListener().ListenAsync(AddressToInt(item.BaseUri), 41100);
                }
            }

            await RefreshDevicesAsync(null);

            await SaveDevicesInStorageAsync(Devices.ToList());
        }

        private long AddressToInt(string addr)
        {
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            var address = IPAddress.Parse(addr.Substring(7, 13));
            var temp = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);
            return (uint)IPAddress.NetworkToHostOrder(temp);
        }

        /// <summary>
        /// Items the power toggled asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        private async Task Item_PowerToggledAsync(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(new Uri(e.BaseUri), e.Zone);
            await RefreshDevicesAsync(null);
        }

        /// <summary>
        /// Finds the new devices.
        /// </summary>
        /// <returns></returns>
        public async Task FindNewDevices()
        {
            service = new MusicCastService();
            service.DeviceFound += Service_DeviceFoundAsync;
            await service.LoadRoomsAsync();
        }

        /// <summary>
        /// Services the device found asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="device">The device.</param>
        private async void Service_DeviceFoundAsync(object sender, MusicCastDevice device)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (Devices.Where(w => w != null).Select(s => s.Id + s.Zone).Contains(device.Id + device.Zone))
                    return;

                Devices.Add(new Device()
                {
                    Id = device.Id,
                    BaseUri = device.BaseUri,
                    Zone = device.Zone,
                    FriendlyName = device.FriendlyName,
                    ImageUri = UriHelper.ResolvePath(device.Location, device.ImagePath),
                    ImageSize = device.ImageSize,
                    Power = device.Power,
                    Input = device.Input.ToString(),
                    SubTitle = device.NowPlayingInformation,
                    BackGround = new SolidColorBrush(Colors.LightGray)
                });
            });
            await SaveDevicesInStorageAsync(Devices.ToList());
        }

        /// <summary>
        /// Loads the devices from storage asynchronous.
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Device>> LoadDevicesFromStorageAsync()
        {
            var devices = await ApplicationData.Current.LocalSettings.ReadAsync<List<Device>>(DevicesKey);
            return devices ?? new List<Device>();
        }

        /// <summary>
        /// Saves the devices in storage asynchronous.
        /// </summary>
        /// <param name="devices">The devices.</param>
        /// <returns></returns>
        private static async Task SaveDevicesInStorageAsync(List<Device> devices)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(DevicesKey, devices);
        }
    }
}

