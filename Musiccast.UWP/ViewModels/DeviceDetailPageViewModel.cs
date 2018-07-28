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

        public DeviceDetailPageViewModel(NavigationServiceEx navigationService)
        {
            this.navigationService = navigationService;
            navigationService.Navigated += NavigationService_Navigated;
        }

        private void NavigationService_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.Device = e.Parameter as Device;
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

        public ICommand TogglePowerCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await TogglePowerAsync();
                    await RefreshDeviceAsync();
                });
            }
        }

        private async Task RefreshDeviceAsync()
        {
            var updatedDevice = await service.RefreshDeviceAsync(Device.Id, Device.Zone);

            await DispatcherHelper.RunAsync(() =>
            {
                Device.Power = updatedDevice.Power;
                Device.Input = updatedDevice.Input.ToString();
                Device.SubTitle = updatedDevice.NowPlayingInformation;
            });
        }

        private async Task TogglePowerAsync()
        {
            if (service == null)
                service = new MusicCastService();

            await service.TogglePowerAsync(Device.Id, Device.Zone);
        }
    }
}
