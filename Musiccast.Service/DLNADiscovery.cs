using Rssdp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Musiccast.Service
{
    public class DLNADiscovery
    {
        public event EventHandler<DLNADescription> DeviceFound;

        public async Task ScanNetworkAsync(CancellationToken token)
        {
            // This code goes in a method somewhere.
            using (var deviceLocator = new SsdpDeviceLocator())
            {
                await Task.Factory.StartNew(async () =>
                {
                    var foundDevices = await deviceLocator.SearchAsync(); // Can pass search arguments here (device type, uuid). No arguments means all devices.

                    foreach (var foundDevice in foundDevices)
                    {
                        // Device data returned only contains basic device details and location ]
                        // of full device description.
                        Debug.WriteLine("Found " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());

                        // Can retrieve the full device description easily though.
                        var fullDevice = await foundDevice.GetDeviceInfo();
                        Debug.WriteLine(fullDevice.FriendlyName);
                        Debug.WriteLine("");

                        var result = await RenderDeviceAsync(foundDevice.DescriptionLocation.ToString());
                        if (result != null && DeviceFound != null)
                            DeviceFound(this, result);
                    }
                });

                deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
                deviceLocator.StartListeningForNotifications();
            }
        }

        private void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
        {
            if (!e.IsNewlyDiscovered)
                Debug.WriteLine("DEVICE EVENT --> " + e.DiscoveredDevice.NotificationType + ": " + e.DiscoveredDevice.DescriptionLocation);
        }

        private async Task<DLNADescription> RenderDeviceAsync(string location)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(DLNADescription));

                using (HttpClient client = new HttpClient())
                {
                    using (var stream = await client.GetStreamAsync(location))
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
