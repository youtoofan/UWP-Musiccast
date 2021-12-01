using Rssdp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Musiccast.Service
{
    public class DLNADiscovery:IDisposable
    {
        private bool isDisposed;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string localIp;
        private SsdpDeviceLocator deviceLocator;
        private bool ignoreCache = true;

        public event EventHandler<DLNADescription> DeviceFound;

        public DLNADiscovery(IHttpClientFactory httpClientFactory, string localIp)
        {
            this._httpClientFactory = httpClientFactory;
            this.localIp = localIp;

            InitListener();
        }

        private void InitListener()
        {
            try
            {
                deviceLocator = new SsdpDeviceLocator(new Rssdp.Infrastructure.SsdpCommunicationsServer(new Rssdp.SocketFactory(localIp)));
                deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailableAsync;
                deviceLocator.DeviceUnavailable += DeviceLocator_DeviceUnavailable;
                deviceLocator.StartListeningForNotifications();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task ScanNetworkAsync()
        {
            if (deviceLocator == null)
                InitListener();

            if (deviceLocator == null || deviceLocator.IsSearching)
                return;
            
            _ = await deviceLocator.SearchAsync("urn:schemas-upnp-org:device:MediaRenderer:1", TimeSpan.FromSeconds(5));
        }

        public async Task ScanNetworkForDeviceAsync(string id)
        {
            if (deviceLocator == null)
                InitListener();

            if (deviceLocator == null || deviceLocator.IsSearching)
                return;

            await deviceLocator.SearchAsync($"uuid:{id}", TimeSpan.FromMilliseconds(100));
        }

        private async void DeviceLocator_DeviceAvailableAsync(object sender, DeviceAvailableEventArgs e)
        {
            if (!e.IsNewlyDiscovered && !ignoreCache)
            {
                Debug.WriteLine($"AVAIL DEVICE EVENT --> {e.DiscoveredDevice.NotificationType}: {e.DiscoveredDevice.DescriptionLocation}");
            }
            else
            {
                var foundDevice = e.DiscoveredDevice;
                Debug.WriteLine($"New DEVICE FOUND: {foundDevice.Usn} at {foundDevice.DescriptionLocation.ToString()}");

                // Can retrieve the full device description easily though.
                var fullDevice = await foundDevice.GetDeviceInfo().ConfigureAwait(false);
                Debug.WriteLine(fullDevice.FriendlyName);

                var result = await RenderDeviceAsync(foundDevice.DescriptionLocation.ToString()).ConfigureAwait(false);
                if (result != null && DeviceFound != null)
                    DeviceFound(this, result);
            }
        }

        private void DeviceLocator_DeviceUnavailable(object sender, DeviceUnavailableEventArgs e)
        {
            Debug.WriteLine($"UNAVAIL DEVICE EVENT --> {e.DiscoveredDevice.NotificationType}: {e.DiscoveredDevice.DescriptionLocation}");
        }

        private async Task<DLNADescription> RenderDeviceAsync(string location)
        {
            try
            {
                var reader = new XmlSerializer(typeof(DLNADescription));
                using (var client = _httpClientFactory.CreateClient())
                {
                    using (var stream = await client.GetStreamAsync(location).ConfigureAwait(false))
                    {
                        using (StreamReader file = new StreamReader(stream))
                        {
                            var device = (DLNADescription)reader.Deserialize(file);
                            device.Location = location;
                            return (device);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                if (deviceLocator != null && !deviceLocator.IsDisposed)
                    deviceLocator.Dispose();
            }

            isDisposed = true;
        }

        
    }
}
