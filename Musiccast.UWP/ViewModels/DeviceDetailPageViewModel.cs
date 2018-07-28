using System;
using App4.Services;
using GalaSoft.MvvmLight;
using Musiccast.Models;

namespace App4.ViewModels
{
    public class DeviceDetailPageViewModel : ViewModelBase
    {
        private readonly NavigationServiceEx navigationService;
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
    }
}
