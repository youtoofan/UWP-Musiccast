using System;

using App4.Services;
using GalaSoft.MvvmLight.Threading;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Musiccast.Service;
using Windows.Storage;
using Microsoft.Extensions.Logging;
using Musiccast.Helpers;

namespace App4
{
    public sealed partial class App : Application
    {
        private UDPListener UDPListener;
        public static event EventHandler<string> UDPNotificationReceived;

        public static ServiceProvider ServiceProvider { get; private set; }

        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();
            StartUDPListener();

            Current.Resuming += Current_Resuming;
            Current.Suspending += Current_Suspending;
            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
            this.UnhandledException += App_UnhandledException;
        }

        

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = App.ServiceProvider.GetService(typeof(ILogger<App>)) as ILogger<App>;
            logger.LogError(e.Exception, "Unhandled");
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddLogging();
            services.AddSingleton<MusicCastService>();

            var folder = ApplicationData.Current.LocalFolder;
            var fullPath = $"{folder.Path}\\Logs\\App.log";

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.GetService<ILoggerFactory>().AddFile(fullPath, LogLevel.Error, null, false, retainedFileCountLimit: 2);

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }

            DispatcherHelper.Initialize();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(ViewModels.ZonesViewModel));
        }

        private void Current_Resuming(object sender, object e)
        {
            StartUDPListener();
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StopUDPpListener();
        }

        private void StartUDPListener()
        {
            if (UDPListener == null)
            {
                UDPListener = new UDPListener();
                UDPListener.DeviceNotificationRecieved += HandleUDP;
                UDPListener.StartListener(Musiccast.Model.Constants.UDP_ListenPort);
            }
        }

        private void HandleUDP(object sender, string e)
        {
            if (UDPNotificationReceived != null)
                UDPNotificationReceived.Invoke(this, e);
        }

        private void StopUDPpListener()
        {
            if (UDPListener != null)
            {
                UDPListener.DeviceNotificationRecieved -= HandleUDP;
                UDPListener.StopListener();
            }
        }
    }
}
