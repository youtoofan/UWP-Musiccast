using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musiccast.App.ViewModels;

namespace Musiccast.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public bool IsLightChecked => ViewModel.ElementTheme == ElementTheme.Light;
        public bool IsDarkChecked => ViewModel.ElementTheme == ElementTheme.Dark;
        public bool IsDefaultChecked => ViewModel.ElementTheme == ElementTheme.Default;

        public SettingsPage()
        {
            ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
            InitializeComponent();
        }
    }
}
