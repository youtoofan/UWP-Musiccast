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
