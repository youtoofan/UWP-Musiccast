using System;
using Microsoft.UI.Xaml.Controls;

namespace Musiccast.App.Services
{
    /// <summary>
    /// Minimal Frame-based navigation service replacing the old UWP NavigationServiceEx.
    /// </summary>
    public class NavigationService
    {
        public Frame? Frame { get; set; }

        public event EventHandler? Navigated;

        public bool CanGoBack => Frame?.CanGoBack ?? false;

        public bool Navigate(Type pageType, object? parameter = null)
        {
            if (Frame == null)
            {
                return false;
            }

            var result = Frame.Navigate(pageType, parameter);
            if (result)
            {
                Navigated?.Invoke(this, EventArgs.Empty);
            }

            return result;
        }

        public void GoBack()
        {
            if (Frame?.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
    }
}
