using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musiccast.App.Models;
using Musiccast.App.ViewModels;

namespace Musiccast.App.Views
{
    public sealed partial class ZonesPage : Page
    {
        public ZonesViewModel ViewModel { get; }

        public ZonesPage()
        {
            ViewModel = App.Services.GetRequiredService<ZonesViewModel>();
            InitializeComponent();
        }

        private void DevicesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Device device)
                ViewModel.ViewDeviceDetailCommand.Execute(device);
        }
    }
}
