﻿using System;

using App4.Services;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Threading;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Musiccast.Service;

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

            var services = new ServiceCollection().AddHttpClient();
            services.AddTransient<MusicCastService>();
            ServiceProvider = services.BuildServiceProvider();

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
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
