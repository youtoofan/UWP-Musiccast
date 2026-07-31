using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Musiccast.App.Services;
using Musiccast.App.Views;

namespace Musiccast.App;

/// <summary>
/// The application window. Hosts a NavigationView shell and a Frame that displays pages.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly NavigationService _navigationService;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        _navigationService = App.Services.GetRequiredService<NavigationService>();
        _navigationService.Frame = ContentFrame;

        ContentFrame.Navigated += ContentFrame_Navigated;
        ContentFrame.Navigate(typeof(ZonesPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            switch (tag)
            {
                case "Zones":
                    ContentFrame.Navigate(typeof(ZonesPage));
                    break;
            }
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
            ContentFrame.GoBack();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        NavView.IsBackEnabled = ContentFrame.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            NavView.SelectedItem = NavView.SettingsItem;
        }
        else if (e.SourcePageType == typeof(ZonesPage))
        {
            foreach (var mi in NavView.MenuItems)
            {
                if (mi is NavigationViewItem item && (item.Tag as string) == "Zones")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
