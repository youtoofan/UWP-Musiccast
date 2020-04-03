using System;
using System.Net.Http;
using App4.Services;
using App4.Views;

using CommonServiceLocator;

using GalaSoft.MvvmLight.Ioc;
using Musiccast.Service;

namespace App4.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register(() => new NavigationServiceEx());
            Register<ZonesViewModel, ZonesPage>();
            Register<SettingsViewModel, SettingsPage>();
            Register<DeviceDetailPageViewModel, DeviceDetailPagePage>();
        }

        public NavigationServiceEx NavigationService => ServiceLocator.Current.GetInstance<NavigationServiceEx>();

        public DeviceDetailPageViewModel DeviceDetailPageViewModel => ServiceLocator.Current.GetInstance<DeviceDetailPageViewModel>();

        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public ZonesViewModel ZonesViewModel => ServiceLocator.Current.GetInstance<ZonesViewModel>();

        

        public void Register<VM, V>()
            where VM : class
        {
            SimpleIoc.Default.Register<VM>();

            NavigationService.Configure(typeof(VM).FullName, typeof(V));
        }
    }
}
