using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Musiccast.App.Services;
using Musiccast.App.ViewModels;
using Musiccast.Helpers;
using Musiccast.Service;

namespace Musiccast.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private UDPListener? _udpListener;

    public static event EventHandler<string>? UDPNotificationReceived;

    public static IServiceProvider Services { get; private set; } = default!;

    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
        StartUDPListener();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddHttpClient();
        services.AddSingleton<MusicCastService>();
        services.AddSingleton<NavigationService>();
        services.AddTransient<ZonesViewModel>();
        services.AddTransient<DeviceDetailViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        await ThemeSelectorService.InitializeAsync();

        MainWindow = new MainWindow();
        ThemeSelectorService.SetRequestedTheme();
        MainWindow.Activate();
    }

    private void StartUDPListener()
    {
        if (_udpListener != null)
        {
            return;
        }

        _udpListener = new UDPListener();
        _udpListener.DeviceNotificationRecieved += HandleUDP;
        _udpListener.StartListener(Musiccast.Model.Constants.UDP_ListenPort);
    }

    private void HandleUDP(object? sender, string e) => UDPNotificationReceived?.Invoke(this, e);
}
