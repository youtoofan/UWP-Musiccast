using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using App4.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Musiccast.Helpers;
using Musiccast.Models;
using Musiccast.Service;

namespace App4.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class DeviceDetailPageViewModel : ViewModelBase
    {
        /// <summary>
        /// The navigation service
        /// </summary>
        private readonly NavigationServiceEx navigationService;
        /// <summary>
        /// The service
        /// </summary>
        private MusicCastService service;
        /// <summary>
        /// The device
        /// </summary>
        private Device _device;
        /// <summary>
        /// Gets or sets the input list.
        /// </summary>
        /// <value>
        /// The input list.
        /// </value>
        public ObservableCollection<Input> InputList { get; set; }
        /// <summary>
        /// Gets or sets the favorites list.
        /// </summary>
        /// <value>
        /// The favorites list.
        /// </value>
        public ObservableCollection<Input> FavoritesList { get; set; }
        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public Device Device
        {
            get
            {
                return _device;
            }

            set
            {
                Set(ref _device, value);
            }
        }

        /// <summary>
        /// Gets the navigate home command.
        /// </summary>
        /// <value>
        /// The navigate home command.
        /// </value>
        public ICommand NavigateHomeCommand
        {
            get
            {
                return new RelayCommand(() => navigationService.Navigate(typeof(ZonesViewModel).FullName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDetailPageViewModel"/> class.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        public DeviceDetailPageViewModel(NavigationServiceEx navigationService)
        {
            this.InputList = new ObservableCollection<Input>();
            this.FavoritesList = new ObservableCollection<Input>();

            this.navigationService = navigationService;
            navigationService.Navigated += NavigationService_NavigatedAsync;
        }

        /// <summary>
        /// Handles the NavigatedAsync event of the NavigationService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Windows.UI.Xaml.Navigation.NavigationEventArgs"/> instance containing the event data.</param>
        private async void NavigationService_NavigatedAsync(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.Device = e.Parameter as Device;

            if (Device == null)
                return;

            this.Device.PowerToggled += Device_PowerToggledAsync;
            this.Device.VolumeChanged += Device_VolumeChanged;

            await RefreshDeviceAsync();
        }

        /// <summary>
        /// Devices the volume changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private async void Device_VolumeChanged(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.AdjustDeviceVolume(new Uri(Device.BaseUri), Device.Zone, e.Volume);
        }

        /// <summary>
        /// Devices the power toggled asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private async void Device_PowerToggledAsync(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(new Uri(Device.BaseUri), Device.Zone);
            await RefreshDeviceAsync();
        }

        /// <summary>
        /// Refreshes the device asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshDeviceAsync()
        {
            if (service == null)
                service = new MusicCastService();

            var updatedDevice = await service.RefreshDeviceAsync(Device.Id, new Uri(Device.BaseUri), Device.Zone);
            var features = await service.GetFeatures(new Uri(Device.BaseUri));
            var tunerPresetsDAB = await service.GetTunerPresets(new Uri(Device.BaseUri), "dab");
            var tunerPresetsFM = await service.GetTunerPresets(new Uri(Device.BaseUri), "fm");
            var usbPresets = await service.GetUsbPresets(new Uri(Device.BaseUri));

            await DispatcherHelper.RunAsync(() =>
            {
                this.InputList.Clear();
                this.FavoritesList.Clear();

                Device.Power = updatedDevice.Power;
                Device.Input = updatedDevice.Input.ToString();
                Device.SubTitle = updatedDevice.NowPlayingInformation;
                Device.Volume = updatedDevice.Volume;
                Device.MaxVolume = updatedDevice.MaxVolume;
                Device.ImageUri = UriHelper.ResolvePath(updatedDevice.Location, updatedDevice.ImagePath);
                Device.ImageSize = updatedDevice.ImageSize;

                foreach (var item in features.system.input_list)
                {
                    this.InputList.Add(InputToGlyphAndTitleConverter.ConvertInput(item.id));
                }

                foreach (var item in tunerPresetsDAB.preset_info)
                {
                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Input()
                    {
                        Name = item.text,
                        Icon = item.band
                    });
                }

                foreach (var item in tunerPresetsFM.preset_info)
                {
                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Input()
                    {
                        Name = item.text,
                        Icon = item.band
                    });
                }

                foreach (var item in usbPresets.preset_info)
                {
                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Input()
                    {
                        Name = item.text,
                        Icon = item.input
                    });
                }
            });
        }
    }
}
