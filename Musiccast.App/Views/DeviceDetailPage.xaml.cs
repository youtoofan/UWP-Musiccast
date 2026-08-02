using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Musiccast.App.Models;
using Musiccast.App.ViewModels;

namespace Musiccast.App.Views
{
    public sealed partial class DeviceDetailPage : Page
    {
        public DeviceDetailViewModel ViewModel { get; }

        public DeviceDetailPage()
        {
            ViewModel = App.Services.GetRequiredService<DeviceDetailViewModel>();
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Device device)
            {
                await ViewModel.LoadAsync(device);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.Unload();
        }

        private void InputsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Input input)
            {
                ViewModel.InputClickedCommand.Execute(input);
            }
        }

        private void FavoritesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Preset preset)
            {
                ViewModel.FavoriteClickedCommand.Execute(preset);
            }
        }
    }
}
