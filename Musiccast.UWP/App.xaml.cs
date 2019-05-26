using System;

using App4.Services;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Threading;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Musiccast.Service;
using Windows.Storage;
using App4.Helpers;
using Microsoft.Extensions.Logging;

namespace App4
{
    public sealed partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
            this.UnhandledException += App_UnhandledExceptionAsync;
        }

        private async void App_UnhandledExceptionAsync(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = App.ServiceProvider.GetService(typeof(ILogger<App>)) as ILogger<App>;
            logger.LogError(e.Exception, "Unhandled");
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddLogging();
            services.AddTransient<MusicCastService>();

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
    }
}
