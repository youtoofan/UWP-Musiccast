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
        public ObservableCollection<Preset> FavoritesList { get; set; }
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

        public ICommand FavoriteClickedCommand
        {
            get
            {
                return new RelayCommand<Preset>((item) => ChangeDevicePreset(item));
            }
        }

        

        public ICommand InputClickedCommand
        {
            get
            {
                return new RelayCommand<Input>((item) => ChangeDeviceInput(item));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDetailPageViewModel"/> class.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        public DeviceDetailPageViewModel(NavigationServiceEx navigationService)
        {
            this.InputList = new ObservableCollection<Input>();
            this.FavoritesList = new ObservableCollection<Preset>();

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
            {
                this.InputList.Clear();
                this.FavoritesList.Clear();
                return;
            }

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
            if (e == null)
                return;

            if (service == null)
                service = new MusicCastService();

            await service.AdjustDeviceVolume(new Uri(e.BaseUri), e.Zone, e.Volume).ConfigureAwait(false);
        }

        /// <summary>
        /// Devices the power toggled asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private async void Device_PowerToggledAsync(object sender, Device e)
        {
            if (e == null)
                return;

            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(new Uri(e.BaseUri), e.Zone).ConfigureAwait(false);
            await RefreshDeviceAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the device preset.
        /// </summary>
        /// <param name="item">The item.</param>
        private async void ChangeDevicePreset(Preset item)
        {
            if (service == null)
                service = new MusicCastService();

            if (item.InputType == Musiccast.Model.Inputs.tuner)
                await service.RecallTunerPresetAsync(new Uri(Device.BaseUri), Device.Zone, item.Band, item.Index).ConfigureAwait(false);

            if (item.InputType == Musiccast.Model.Inputs.usb)
                await service.RecallUsbPresetAsync(new Uri(Device.BaseUri), Device.Zone, item.Index).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the device input.
        /// </summary>
        /// <param name="item">The item.</param>
        private async void ChangeDeviceInput(Input item)
        {
            if (service == null)
                service = new MusicCastService();

            await service.ChangeDeviceInputAsync(new Uri(Device.BaseUri), Device.Zone, item.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Refreshes the device asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshDeviceAsync()
        {
            if (service == null)
                service = new MusicCastService();

            var refresh = service.RefreshDeviceAsync(Device.Id, new Uri(Device.BaseUri), Device.Zone);
            var feat = service.GetFeatures(new Uri(Device.BaseUri));
            var dab = service.GetTunerPresets(new Uri(Device.BaseUri), "dab");
            var fm = service.GetTunerPresets(new Uri(Device.BaseUri), "fm");
            var usb = service.GetUsbPresets(new Uri(Device.BaseUri));

            await Task.WhenAll(new Task[]{
                refresh,
                feat,
                dab,
                fm,
                usb
            }).ConfigureAwait(false);

            var updatedDevice = await refresh;
            var features = await feat;
            var tunerPresetsDAB = await dab;
            var tunerPresetsFM = await fm;
            var usbPresets = await usb;

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

                for (int i = 0; i < tunerPresetsDAB.preset_info.Count; i++)
                {
                    Musiccast.Model.TunerPresetInfo item = tunerPresetsDAB.preset_info[i];

                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Preset()
                    {
                        Band = item.band,
                        Text = item.text,
                        Number = item.number,
                        Index = i + 1,
                        InputType = Musiccast.Model.Inputs.tuner
                    });
                }

                for (int i = 0; i < tunerPresetsFM.preset_info.Count; i++)
                {
                    Musiccast.Model.TunerPresetInfo item = tunerPresetsFM.preset_info[i];

                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Preset()
                    {
                        Band = item.band,
                        Text = item.text,
                        Number = item.number,
                        Index = i + 1,
                        InputType = Musiccast.Model.Inputs.tuner
                    });
                }

                for (int i = 0; i < usbPresets.preset_info.Count; i++)
                {
                    Musiccast.Model.NetUsbPresetInfo item = usbPresets.preset_info[i];

                    if (string.IsNullOrEmpty(item.text))
                        continue;

                    this.FavoritesList.Add(new Preset()
                    {
                        Band = item.input,
                        Text = item.text,
                        Index = i + 1,
                        InputType = Musiccast.Model.Inputs.usb
                    });
                }
            });
        }
    }
}
