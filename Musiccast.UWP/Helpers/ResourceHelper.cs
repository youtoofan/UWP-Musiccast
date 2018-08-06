using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace Musiccast.Helpers
{
    public static class ResourceHelper
    {
        private static ResourceMap _resourceMap = null;
        public static ResourceMap ResourceMap
        {
            get
            {
                if (_resourceMap == null)
                {
                    _resourceMap = ResourceManager.Current.MainResourceMap;
                }
                return _resourceMap;
            }
        }

        public static string GetString(string key)
        {
            return ResourceMap?.GetValue("Resources/" + key, new ResourceContext())?.ValueAsString;
        }

        public static string GetString(string key, ResourceContext context)
        {
            return ResourceMap?.GetValue("Resources/" + key, context)?.ValueAsString;
        }
    }
}
