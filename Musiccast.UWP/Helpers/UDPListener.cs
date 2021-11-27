using Musiccast.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Musiccast.Helpers
{
    public class UDPListener
    {
        public event EventHandler<string> DeviceNotificationRecieved;
        const string _ssdpMulticastIp = "239.255.255.250";
        const string _msearchMediaRenderer = "M-SEARCH * HTTP/1.1\r\nMX: 1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nST: urn:schemas-upnp-org:device:";
        const string _msearchOther = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nContent-Length: 0\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:";
        const int _endpointPort = 1900;
        private bool listening = false;
        private UdpClient listener;

        /// <summary>
        /// Starts the listener.
        /// </summary>
        /// <param name="port">The port.</param>
        public void StartListener(int port)
        {
            if (listening)
                return;

            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                var multicastIp = IPAddress.Parse(_ssdpMulticastIp);
                IPEndPoint groupEP = new IPEndPoint(multicastIp, _endpointPort);
                listener = new UdpClient(new IPEndPoint(IPAddress.Any, port));
                listener.EnableBroadcast = true;
                listening = true;

                try
                {
                    do
                    {
                        Debug.WriteLine("Waiting for broadcast");
                        var udpResult = await listener.ReceiveAsync();
                        byte[] bytes = udpResult.Buffer;
                        var temp = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                        Debug.WriteLine($"Received broadcast from {groupEP} :");
                        Debug.WriteLine(temp);

                        var result = JsonConvert.DeserializeObject<MusicCastNotification>(temp);
                        if (result != null && !string.IsNullOrEmpty(result.device_id))
                        {
                            string id = result.device_id;
                            DeviceNotificationRecieved(this, id);
                        }
                    } while (listening);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    listener.Close();
                    listener.Dispose();

                    Debug.WriteLine("Listener Disposed");
                }
            });
        }

        public void StopListener()
        {
            listening = false;
        }
    }
}
