using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Musiccast.App.Services;
using Windows.ApplicationModel;

namespace Musiccast.App.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ElementTheme ElementTheme { get; set; }

        [ObservableProperty]
        public partial string VersionDescription { get; set; }

        public SettingsViewModel()
        {
            ElementTheme = ThemeSelectorService.Theme;
            VersionDescription = GetVersionDescription();
        }

        [RelayCommand]
        private async Task SwitchTheme(ElementTheme theme)
        {
            ElementTheme = theme;
            await ThemeSelectorService.SetThemeAsync(theme);
        }

        private static string GetVersionDescription()
        {
            try
            {
                var version = Package.Current.Id.Version;
                return $"MusicCast Controller - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            catch
            {
                return "MusicCast Controller";
            }
        }
    }
}
