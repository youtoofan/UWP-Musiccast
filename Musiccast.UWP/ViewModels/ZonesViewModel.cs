using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.System.Threading;
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
        /// The UDP listener
        /// </summary>
        public UDPListener UDPListener;

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

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
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
        }

        /// <summary>
        /// UDPs the listener device notification recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="deviceId">The device identifier.</param>
        private async void UDPListener_DeviceNotificationRecievedAsync(object sender, string deviceId)
        {
            var existingDevice = Devices.FirstOrDefault(w => w.Id.Equals(deviceId));
            if (existingDevice == null)
                return;

            var updatedDevice = await service.RefreshDeviceAsync(existingDevice.Id, new Uri(existingDevice.BaseUri), existingDevice.Zone);

            await DispatcherHelper.RunAsync(() =>
            {
                try
                {
                    existingDevice.Power = updatedDevice.Power;
                    existingDevice.Input = updatedDevice.Input.ToString();
                    existingDevice.SubTitle = updatedDevice.NowPlayingInformation;
                    existingDevice.BackGround = new SolidColorBrush(Colors.OrangeRed);
                    existingDevice.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                    existingDevice.ImageSize = existingDevice.ImageSize;

                    if (string.IsNullOrEmpty(existingDevice.FriendlyName))
                        existingDevice.FriendlyName = ResourceHelper.GetString("DeviceName_Unknown");

                    existingDevice.PowerToggled -= async (s, args) => { await Item_PowerToggledAsync(s, args); };
                    existingDevice.PowerToggled += async (s, args) => { await Item_PowerToggledAsync(s, args); };

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "Exception");
                }
                finally
                {
                    IsLoading = false;
                }
            });
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
                    try
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
                    
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex, "Exception");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
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
                }
            }

            await RefreshDevicesAsync(null);

            await SaveDevicesInStorageAsync(Devices.ToList());

            await ThreadPool.RunAsync((state) =>
            {
                if (UDPListener == null)
                {
                    UDPListener = new UDPListener();
                    UDPListener.DeviceNotificationRecieved += UDPListener_DeviceNotificationRecievedAsync;
                    UDPListener.StartListener(41100);
                }
            });
        }

        /// <summary>
        /// Unregisters this instance from the Messenger class.
        /// <para>To cleanup additional resources, override this method, clean
        /// up and then call base.Cleanup().</para>
        /// </summary>
        public override void Cleanup()
        {
            if (UDPListener != null)
                UDPListener.Dispose();

            base.Cleanup();
        }

        /// <summary>
        /// Addresses to ip address.
        /// </summary>
        /// <param name="addr">The addr.</param>
        /// <returns></returns>
        private IPAddress AddressToIPAddress(string addr)
        {
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            var address = addr.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ? IPAddress.Parse(addr.Substring(7, 13)) : IPAddress.Parse(addr);
            return address;
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
            await DispatcherHelper.RunAsync(async () =>
            {
                IsLoading = true;
                service = new MusicCastService();
                service.DeviceFound += Service_DeviceFoundAsync;
                await service.LoadRoomsAsync();
            });
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
                IsLoading = false;

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

