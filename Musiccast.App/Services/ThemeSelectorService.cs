using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Musiccast.App.Helpers;

namespace Musiccast.App.Services
{
    public static class ThemeSelectorService
    {
        private const string SettingsKey = "AppBackgroundRequestedTheme";

        public static ElementTheme Theme { get; private set; } = ElementTheme.Default;

        public static async Task InitializeAsync()
        {
            Theme = await LoadThemeFromSettingsAsync();
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;
            SetRequestedTheme();
            await SaveThemeInSettingsAsync(theme);
        }

        public static void SetRequestedTheme()
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = Theme;
            }
        }

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            var themeName = await SettingsStorage.ReadAsync<string>(SettingsKey);
            if (System.Enum.TryParse(themeName, out ElementTheme cacheTheme))
            {
                return cacheTheme;
            }

            return ElementTheme.Default;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await SettingsStorage.SaveAsync(SettingsKey, theme.ToString());
        }
    }
}
