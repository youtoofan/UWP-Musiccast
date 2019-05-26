using Rssdp;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Musiccast.Service
{
    public class DLNADiscovery:IDisposable
    {
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
            deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailableAsync;
            deviceLocator.StartListeningForNotifications();
            deviceLocator.NotificationFilter = "urn:schemas-upnp-org:device:MediaRenderer:1";
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
                Debug.WriteLine("DEVICE EVENT --> " + e.DiscoveredDevice.NotificationType + ": " + e.DiscoveredDevice.DescriptionLocation);
            }
            else
            {
                var foundDevice = e.DiscoveredDevice;
                Debug.WriteLine("Found " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());

                // Can retrieve the full device description easily though.
                var fullDevice = await foundDevice.GetDeviceInfo().ConfigureAwait(false);
                Debug.WriteLine(fullDevice.FriendlyName);
                Debug.WriteLine("");

                var result = await RenderDeviceAsync(foundDevice.DescriptionLocation.ToString()).ConfigureAwait(false);
                if (result != null && DeviceFound != null)
                    DeviceFound(this, result);
            }
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
                var serializer = new XmlSerializer(typeof(DLNADescription));

                var client = _httpClientFactory.CreateClient();
                {
                    using (var stream = await client.GetStreamAsync(location).ConfigureAwait(false))
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            var device = (DLNADescription)serializer.Deserialize(reader);
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

        public void Dispose()
        {
            if (deviceLocator != null && !deviceLocator.IsDisposed)
                deviceLocator.Dispose();
        }
    }
}
