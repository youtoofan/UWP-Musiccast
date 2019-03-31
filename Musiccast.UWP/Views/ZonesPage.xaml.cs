using System;

using App4.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace App4.Views
{
    public sealed partial class ZonesPage : Page
    {
        private ZonesViewModel ViewModel
        {
            get { return DataContext as ZonesViewModel; }
        }

        public ZonesPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitAsync();
            base.OnNavigatedTo(e);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await ViewModel.DestroyAsync();
            base.OnNavigatedFrom(e);
        }
    }
}
