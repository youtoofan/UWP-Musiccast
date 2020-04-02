using Rssdp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Musiccast.Service
{
    public class DLNADiscovery:IDisposable
    {
        private bool isDisposed;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SsdpDeviceLocator deviceLocator;

        public event EventHandler<DLNADescription> DeviceFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLNADiscovery"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public DLNADiscovery(IHttpClientFactory httpClientFactory, string localIp)
        {
            this._httpClientFactory = httpClientFactory;
            this.deviceLocator = new SsdpDeviceLocator(localIp);
        }

        /// <summary>
        /// Scans the network asynchronous.
        /// </summary>
        /// 
        /// <returns></returns>
        public void ScanNetwork()
        {
            if (deviceLocator.IsSearching)
                return;

            deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailableAsync;
            deviceLocator.DeviceUnavailable += DeviceLocator_DeviceUnavailable;
            deviceLocator.StartListeningForNotifications();
            //deviceLocator.NotificationFilter = "urn:schemas-upnp-org:device:MediaRenderer:1";
            deviceLocator.SearchAsync(TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Handles the DeviceAvailable event of the DeviceLocator control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DeviceAvailableEventArgs"/> instance containing the event data.</param>
        private async void DeviceLocator_DeviceAvailableAsync(object sender, DeviceAvailableEventArgs e)
        {
            if (!e.IsNewlyDiscovered)
            {
                Debug.WriteLine("AVAIL DEVICE EVENT --> " + e.DiscoveredDevice.NotificationType + ": " + e.DiscoveredDevice.DescriptionLocation);
            }
            else
            {
                var foundDevice = e.DiscoveredDevice;
                Debug.WriteLine("New DEVICE FOUND: " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());

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
            Debug.WriteLine("UNAVAIL DEVICE EVENT --> " + e.DiscoveredDevice.NotificationType + ": " + e.DiscoveredDevice.DescriptionLocation);
        }

        /// <summary>
        /// Renders the device asynchronous.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        private async Task<DLNADescription> RenderDeviceAsync(string location)
        {
            try
            {
                XmlSerializer reader = new XmlSerializer(typeof(DLNADescription));
                using (var client = _httpClientFactory.CreateClient())
                {
                    var stream = await client.GetStreamAsync(location).ConfigureAwait(false);
                    StreamReader file = new StreamReader(stream);
                    DLNADescription device = (DLNADescription)reader.Deserialize(file);
                    file.Close();

                    device.Location = location;
                    return (device);
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
