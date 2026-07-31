using CommunityToolkit.Mvvm.ComponentModel;
using Musiccast.Model;

namespace Musiccast.App.Models
{
    public partial class Input : ObservableObject
    {
        [ObservableProperty]
        public partial string? Id { get; set; }

        [ObservableProperty]
        public partial string? Icon { get; set; }

        [ObservableProperty]
        public partial string? Name { get; set; }
    }

    public partial class Preset : ObservableObject
    {
        [ObservableProperty]
        public partial string? Text { get; set; }

        [ObservableProperty]
        public partial string? Band { get; set; }

        [ObservableProperty]
        public partial int Number { get; set; }

        [ObservableProperty]
        public partial Inputs InputType { get; set; }

        [ObservableProperty]
        public partial int Index { get; set; }
    }
}
