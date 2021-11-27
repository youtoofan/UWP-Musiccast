
using App4.ViewModels;
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
    }
}
