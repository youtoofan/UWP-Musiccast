using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Musiccast.App.Helpers;
using Musiccast.App.Models;
using Musiccast.App.Services;
using Musiccast.Model;
using Musiccast.Service;

using Input = Musiccast.App.Models.Input;
using Preset = Musiccast.App.Models.Preset;

namespace Musiccast.App.ViewModels
{
    public partial class DeviceDetailViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private readonly NavigationService _navigationService;
        private readonly MusicCastService _service;

        public ObservableCollection<Input> InputList { get; } = new();
        public ObservableCollection<Preset> FavoritesList { get; } = new();

        [ObservableProperty]
        public partial Device? Device { get; set; }

        public DeviceDetailViewModel(MusicCastService service, NavigationService navigationService)
        {
            _service = service;
            _navigationService = navigationService;
        }

        public async Task LoadAsync(Device device)
        {
            Detach();

            Device = device;
            if (Device == null)
            {
                InputList.Clear();
                FavoritesList.Clear();
                return;
            }

            Device.PowerToggled += Device_PowerToggledAsync;
            Device.VolumeChanged += Device_VolumeChanged;
            App.UDPNotificationReceived += UDPListener_DeviceNotificationReceivedAsync;

            await RefreshDeviceAsync();
        }

        public void Unload() => Detach();

        private void Detach()
        {
            if (Device != null)
            {
                Device.PowerToggled -= Device_PowerToggledAsync;
                Device.VolumeChanged -= Device_VolumeChanged;
            }
            App.UDPNotificationReceived -= UDPListener_DeviceNotificationReceivedAsync;
        }

        [RelayCommand]
        private void NavigateHome() => _navigationService.GoBack();

        [RelayCommand]
        private async Task FavoriteClicked(Preset? item)
        {
            if (item == null || Device == null)
                return;

            if (item.InputType == Inputs.tuner)
                await _service.RecallTunerPresetAsync(new Uri(Device.BaseUri!), Device.Zone!, item.Band!, item.Index);

            if (item.InputType == Inputs.usb)
                await _service.RecallUsbPresetAsync(new Uri(Device.BaseUri!), Device.Zone!, item.Index);
        }

        [RelayCommand]
        private async Task InputClicked(Input? item)
        {
            if (item == null || Device == null)
                return;

            await _service.ChangeDeviceInputAsync(new Uri(Device.BaseUri!), Device.Zone!, item.Id!);
        }

        private async void Device_VolumeChanged(object? sender, Device e)
        {
            if (e == null) return;
            await _service.AdjustDeviceVolume(new Uri(e.BaseUri!), e.Zone!, e.Volume);
        }

        private async void Device_PowerToggledAsync(object? sender, Device e)
        {
            if (e == null) return;
            await _service.TogglePowerAsync(new Uri(e.BaseUri!), e.Zone!);
            await RefreshDeviceAsync();
        }

        private async void UDPListener_DeviceNotificationReceivedAsync(object? sender, string deviceId)
        {
            if (Device?.Id == null || !Device.Id.Equals(deviceId))
                return;

            await RefreshDeviceAsync();
        }

        private async Task RefreshDeviceAsync()
        {
            if (Device == null)
                return;

            try
            {
                var baseUri = new Uri(Device.BaseUri!);
                var refresh = _service.RefreshDeviceAsync(Device.Id, baseUri, Device.Zone!);
                var feat = _service.GetFeatures(baseUri);
                var dab = _service.GetTunerPresets(baseUri, "dab");
                var fm = _service.GetTunerPresets(baseUri, "fm");
                var usb = _service.GetUsbPresets(baseUri);

                await Task.WhenAll(refresh, feat, dab, fm, usb);

                var updated = await refresh;
                var features = await feat;
                var tunerDab = await dab;
                var tunerFm = await fm;
                var usbPresets = await usb;

                if (updated == null)
                    return;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    InputList.Clear();
                    FavoritesList.Clear();

                    Device.Power = updated.Power;
                    Device.Input = updated.Input.ToString();
                    Device.SubTitle = updated.NowPlayingInformation;
                    Device.Volume = updated.Volume;
                    Device.MaxVolume = updated.MaxVolume;
                    Device.ImageUri = UriHelper.ResolvePath(updated.Location, updated.ImagePath);
                    Device.ImageSize = updated.ImageSize;
                    Device.FriendlyName = string.IsNullOrEmpty(updated.FriendlyName) ? Device.FriendlyName : updated.FriendlyName;

                    if (features?.system?.input_list != null)
                        foreach (var item in features.system.input_list)
                            InputList.Add(InputToGlyphAndTitleConverter.ConvertInput(item.id));

                    AddTunerPresets(tunerDab, requireText: false);
                    AddTunerPresets(tunerFm, requireText: true);

                    if (usbPresets?.preset_info != null)
                        for (int i = 0; i < usbPresets.preset_info.Count; i++)
                        {
                            var item = usbPresets.preset_info[i];
                            if (string.IsNullOrEmpty(item.text))
                                continue;

                            FavoritesList.Add(new Preset
                            {
                                Band = item.input,
                                Text = item.text,
                                Index = i + 1,
                                InputType = Inputs.usb
                            });
                        }
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void AddTunerPresets(GetTunerPresetInfo presets, bool requireText)
        {
            if (presets?.preset_info == null)
                return;

            for (int i = 0; i < presets.preset_info.Count; i++)
            {
                var item = presets.preset_info[i];

                if (!requireText && item.number <= 0)
                    continue;

                if (requireText && string.IsNullOrEmpty(item.text))
                    continue;

                if (string.IsNullOrEmpty(item.text))
                    item.text = item.number.ToString();

                FavoritesList.Add(new Preset
                {
                    Band = item.band,
                    Text = item.text,
                    Number = item.number,
                    Index = i + 1,
                    InputType = Inputs.tuner
                });
            }
        }
    }
}
