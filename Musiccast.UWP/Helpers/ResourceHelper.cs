using Windows.ApplicationModel.Resources.Core;

namespace Musiccast.Helpers
{
    public static class ResourceHelper
    {
        private static ResourceContext _context = null;
        private static ResourceContext ResourceContext
        {
            get
            {
                if (_context == null)
                    _context = ResourceManager.Current.DefaultContext;

                return _context;
            }
        }
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
            return ResourceHelper.GetString(key, ResourceContext);
        }

        public static string GetString(string key, ResourceContext context)
        {
            return ResourceMap?.GetValue("Resources/" + key, context)?.ValueAsString;
        }
    }
}
