using System;
using System.Threading.Tasks;
using System.Windows.Input;
using App4.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Musiccast.Models;
using Musiccast.Service;

namespace App4.ViewModels
{
    public class DeviceDetailPageViewModel : ViewModelBase
    {
        private readonly NavigationServiceEx navigationService;
        private MusicCastService service;
        private Device _device;
        private double _volume;

        public DeviceDetailPageViewModel(NavigationServiceEx navigationService)
        {
            this.navigationService = navigationService;
            navigationService.Navigated += NavigationService_NavigatedAsync;
        }

        private async void NavigationService_NavigatedAsync(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.Device = e.Parameter as Device;

            if (Device == null)
                return;

            this.Device.PowerToggled += Device_PowerToggledAsync;
            this.Device.VolumeChanged += Device_VolumeChanged;

            await RefreshDeviceAsync();
        }

        

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

        public ICommand NavigateHomeCommand
        {
            get
            {
                return new RelayCommand(() => navigationService.Navigate(typeof(ZonesViewModel).FullName));
            }
        }

        private async void Device_VolumeChanged(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.AdjustDeviceVolume(Device.Id, Device.Zone, e.Volume);
            //await RefreshDeviceAsync();
        }

        private async void Device_PowerToggledAsync(object sender, Device e)
        {
            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(Device.Id, Device.Zone);
            await RefreshDeviceAsync();
        }

        private async Task RefreshDeviceAsync()
        {
            if (service == null)
                service = new MusicCastService();

            var updatedDevice = await service.RefreshDeviceAsync(Device.Id, Device.Zone);

            await DispatcherHelper.RunAsync(() =>
            {
                Device.Power = updatedDevice.Power;
                Device.Input = updatedDevice.Input.ToString();
                Device.SubTitle = updatedDevice.NowPlayingInformation;
                Device.Volume = updatedDevice.Volume;
                Device.MaxVolume = updatedDevice.MaxVolume;
            });
        }
    }
}
