using GalaSoft.MvvmLight;
using Musiccast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Musiccast.Models
{
    public class Input : ObservableObject
    {
        private string _id;
        private string _icon;
        private string _name;

        public string Id { get => _id; set => Set(ref _id, value); }
        public string Icon { get => _icon; set => Set(ref _icon, value); }
        public string Name { get => _name; set => Set(ref _name, value); }
    }

    public class Preset : ObservableObject
    {
        private string _text;
        private string _band;
        private int _number;
        private Inputs _inputType;
        private int _index;

        public string Text { get => _text; set => Set(ref _text, value); }
        public string Band { get => _band; set => Set(ref _band, value); }
        public int Number { get => _number; set => Set(ref _number, value); }
        public Inputs InputType { get => _inputType; set => Set(ref _inputType, value); }
        public int Index { get => _index; set => Set(ref _index, value); }
    }
}
