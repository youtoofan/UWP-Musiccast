using System;
using App4.Services;
using GalaSoft.MvvmLight;

namespace App4.ViewModels
{
    public class DeviceDetailPageViewModel : ViewModelBase
    {
        private readonly NavigationServiceEx navigationService;

        public DeviceDetailPageViewModel(NavigationServiceEx navigationService)
        {
            this.navigationService = navigationService;
        }
    }
}
