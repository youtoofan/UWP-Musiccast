﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Musiccast.Helpers;
using Newtonsoft.Json;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Media;

namespace Musiccast.Models
{
    public class Device : ViewModelBase
    {
        private string _power;
        private string _input;
        private string _subTitle;
        private int _volume;
        private int _maxVolume;
        private Brush _backGround;
        private Uri _imageUri;

        public event EventHandler<Device> PowerToggled;
        public event EventHandler<Device> VolumeChanged;

        public string Id { get; set; }
        public string BaseUri { get; set; }
        public string FriendlyName { get; set; }
        public string Zone { get; set; }
        
        public int ImageSize { get; set; }

        public Uri ImageUri
        {
            get { return _imageUri; }
            set
            {
                Set(ref _imageUri, value);
            }
        }

        public string Power
        {
            get => _power; set
            {
                Set(() => Power, ref _power, value);
                RaisePropertyChanged("IsOn");
                RaisePropertyChanged("IsOnString");
            }
        }

        public string SubTitle
        {
            get
            {
                return _subTitle;
            }
            set
            {
                Set(() => SubTitle, ref _subTitle, value);
            }
        }

        public string Input
        {
            get => _input;
            set
            {
                Set(() => Input, ref _input, value);
            }
        }

        public bool IsOn
        {
            get
            {
                return Power != "standby" && Power != "off";
            }
        }

        public string IsOnString
        {
            get
            {
                return ResourceHelper.GetString(IsOn ? "Device_On" : "Device_Off");
            }
        }

        [JsonIgnore]
        public Brush BackGround
        {
            get
            {
                return _backGround;
            }

            set
            {
                Set(ref _backGround, value);
            }
        }

        public ICommand TogglePowerCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (PowerToggled != null)
                        PowerToggled(this, this);
                });
            }
        }

        public int Volume
        {
            get => _volume;

            set
            {
                Set(ref _volume, value);
                if (VolumeChanged != null)
                    VolumeChanged(this, this);
            }
        }

        public int MaxVolume
        {
            get => _maxVolume;
            set
            {
                Set(ref _maxVolume, value);
            }
        }


    }
}
