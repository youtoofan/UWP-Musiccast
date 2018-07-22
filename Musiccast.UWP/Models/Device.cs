using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Musiccast.Models
{
    public class Device : ViewModelBase
    {
        private string _power;
        private string _input;
        private string _subTitle;

        public event EventHandler<Device> PowerToggled;

        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public string Zone { get; set; }
        public Uri ImageUri { get; set; }
        public int ImageSize { get; set; }

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
                return IsOn ? "Aan" : "Uit";
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


    }
}
