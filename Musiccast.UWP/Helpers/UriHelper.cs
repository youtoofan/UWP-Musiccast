using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musiccast.Helpers
{
    public class UriHelper
    {
        public static Uri ResolvePath(string deviceLocation, string imagePath)
        {
            if (string.IsNullOrEmpty(deviceLocation) || string.IsNullOrEmpty(imagePath))
                return null;

            var baseUri = new Uri(deviceLocation);
            var temp = baseUri.GetComponents(UriComponents.Scheme | UriComponents.StrongAuthority, UriFormat.Unescaped);
            return new Uri(new Uri(temp), imagePath);
        }
    }
}
