using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Musiccast.App.Models
{
    public partial class Device : ObservableObject
    {
        public event EventHandler<Device>? PowerToggled;
        public event EventHandler<Device>? VolumeChanged;

        public string? Id { get; set; }
        public string? BaseUri { get; set; }
        public string? Zone { get; set; }
        public int ImageSize { get; set; }

        [ObservableProperty]
        public partial Uri? ImageUri { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOn))]
        [NotifyPropertyChangedFor(nameof(IsOnString))]
        public partial string? Power { get; set; }

        [ObservableProperty]
        public partial string? FriendlyName { get; set; }

        [ObservableProperty]
        public partial string? SubTitle { get; set; }

        [ObservableProperty]
        public partial string? Input { get; set; }

        [ObservableProperty]
        public partial bool IsAlive { get; set; }

        private int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                    VolumeChanged?.Invoke(this, this);
            }
        }

        [ObservableProperty]
        public partial int MaxVolume { get; set; }

        public bool IsOn => Power != "standby" && Power != "off";

        public string IsOnString => IsOn ? "On" : "Off";

        [RelayCommand]
        private void TogglePower()
        {
            PowerToggled?.Invoke(this, this);
        }
    }
}
