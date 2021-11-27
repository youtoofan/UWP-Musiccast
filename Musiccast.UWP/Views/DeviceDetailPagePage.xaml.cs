
using App4.ViewModels;

using Windows.UI.Xaml.Controls;

namespace App4.Views
{
    public sealed partial class DeviceDetailPagePage : Page
    {
        private DeviceDetailPageViewModel ViewModel
        {
            get { return DataContext as DeviceDetailPageViewModel; }
        }

        public DeviceDetailPagePage()
        {
            InitializeComponent();
        }
    }
}
