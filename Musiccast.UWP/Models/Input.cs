using Musiccast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Musiccast.Models
{
    public class Input
    {
        public string Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
    }

    public class Preset
    {
        public string Text { get; set; }
        public string Band { get; set; }
        public int Number { get; set; }
        public Inputs InputType { get; set; }
        public int Index { get; set; }
    }
}
