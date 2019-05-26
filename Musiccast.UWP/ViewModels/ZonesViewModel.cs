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
using GalaSoft.MvvmLight.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
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
        private readonly ILogger<ZonesViewModel> logger;

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
                return new RelayCommand(async () => { await FindNewDevices().ConfigureAwait(false); });
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

        public ICommand CancelSearchCommand
        {
            get
            {
                return new RelayCommand(async () => { await CancelFindNewDevices().ConfigureAwait(false); });
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
            Devices = new ObservableCollection<Device>();
            this.navigationService = navigationService;
            this.logger = App.ServiceProvider.GetService(typeof(ILogger<ZonesViewModel>)) as ILogger<ZonesViewModel>;
            this.service = App.ServiceProvider.GetService(typeof(MusicCastService)) as MusicCastService;
        }

        /// <summary>
        /// UDPs the listener device notification recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="deviceId">The device identifier.</param>
        private async void UDPListener_DeviceNotificationRecievedAsync(object sender, string deviceId)
        {
            logger.LogInformation("Listening");

            var existingDevice = Devices.FirstOrDefault(w => w.Id.Equals(deviceId));
            if (existingDevice == null)
                return;

            var updatedDevice = await service.RefreshDeviceAsync(existingDevice.Id, new Uri(existingDevice.BaseUri), existingDevice.Zone).ConfigureAwait(false);

            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                try
                {
                    existingDevice.Power = updatedDevice.Power;
                    existingDevice.Input = updatedDevice.Input.ToString();
                    existingDevice.SubTitle = updatedDevice.NowPlayingInformation;
                    existingDevice.BackGround = new AcrylicBrush() { TintColor = Colors.OrangeRed, TintOpacity = 0.8 };
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

                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
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
                                e.BackGround = new AcrylicBrush() { TintColor = Colors.OrangeRed, TintOpacity = 0.8 };
                                e.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                                e.ImageSize = e.ImageSize;

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

        /// <summary>
        /// Initializes the asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            logger.LogInformation("Init async");

            var temp = await LoadDevicesFromStorageAsync().ConfigureAwait(false);

            System.Threading.ThreadPool.QueueUserWorkItem(StartUDPListener);

            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
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

        /// <summary>
        /// Starts the UDP listener.
        /// </summary>
        /// <param name="state">The state.</param>
        private void StartUDPListener(object state)
        {
            if (UDPListener == null)
            {
                UDPListener = new UDPListener();
                UDPListener.DeviceNotificationRecieved += UDPListener_DeviceNotificationRecievedAsync;
                UDPListener.StartListener(41100);
            }
        }

        /// <summary>
        /// Destroys the asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task DestroyAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Devices.Clear();
            });

            await Windows.System.Threading.ThreadPool.RunAsync((state) =>
            {
                if (UDPListener != null)
                {
                    UDPListener.DeviceNotificationRecieved -= UDPListener_DeviceNotificationRecievedAsync;
                    UDPListener.Dispose();
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
            await service.TogglePowerAsync(new Uri(e.BaseUri), e.Zone).ConfigureAwait(false);
            await RefreshDevicesAsync(null).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds the new devices.
        /// </summary>
        /// <returns></returns>
        public async Task FindNewDevices()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                IsLoading = true;
                service.DeviceFound += Service_DeviceFoundAsync;
                service.LoadRooms();
            });
        }

        /// <summary>
        /// Cancels the find new devices.
        /// </summary>
        /// <returns></returns>
        private async Task CancelFindNewDevices()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                IsLoading = false;
            });
        }

        /// <summary>
        /// Services the device found asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="device">The device.</param>
        private async void Service_DeviceFoundAsync(object sender, MusicCastDevice device)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
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
            await SaveDevicesInStorageAsync(Devices.ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the devices from storage asynchronous.
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Device>> LoadDevicesFromStorageAsync()
        {
            try
            {
                var devices = await ApplicationData.Current.LocalSettings.ReadAsync<List<Device>>(DevicesKey).ConfigureAwait(false);
                return devices ?? new List<Device>();
            }
            catch (Exception)
            {
                return new List<Device>(0);
            }
        }

        /// <summary>
        /// Saves the devices in storage asynchronous.
        /// </summary>
        /// <param name="devices">The devices.</param>
        /// <returns></returns>
        private static async Task SaveDevicesInStorageAsync(List<Device> devices)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(DevicesKey, devices).ConfigureAwait(false);
        }
    }
}

