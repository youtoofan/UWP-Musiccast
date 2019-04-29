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
    public class DLNADiscovery
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public event EventHandler<DLNADescription> DeviceFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLNADiscovery"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public DLNADiscovery(IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Scans the network asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task ScanNetworkAsync(CancellationToken token)
        {
            // This code goes in a method somewhere.
            using (var deviceLocator = new SsdpDeviceLocator())
            {
                //deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
                //deviceLocator.StartListeningForNotifications();

                await Task.Factory.StartNew(async () =>
                {
                    var foundDevices = await deviceLocator.SearchAsync().ConfigureAwait(false); // Can pass search arguments here (device type, uuid). No arguments means all devices.

                    foreach (var foundDevice in foundDevices)
                    {
                        // Device data returned only contains basic device details and location ]
                        // of full device description.
                        Debug.WriteLine("Found " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());

                        // Can retrieve the full device description easily though.
                        var fullDevice = await foundDevice.GetDeviceInfo().ConfigureAwait(false);
                        Debug.WriteLine(fullDevice.FriendlyName);
                        Debug.WriteLine("");

                        var result = await RenderDeviceAsync(foundDevice.DescriptionLocation.ToString()).ConfigureAwait(false);
                        if (result != null && DeviceFound != null)
                            DeviceFound(this, result);
                    }
                });
            }
        }

        /// <summary>
        /// Handles the DeviceAvailable event of the DeviceLocator control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DeviceAvailableEventArgs"/> instance containing the event data.</param>
        private void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
        {
            if (!e.IsNewlyDiscovered)
                Debug.WriteLine("DEVICE EVENT --> " + e.DiscoveredDevice.NotificationType + ": " + e.DiscoveredDevice.DescriptionLocation);
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
    }
}
