using Musiccast.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Musiccast.Helpers
{
    public class UDPListener
    {
        public event EventHandler<string> DeviceNotificationRecieved;
        const string _ssdpMulticastIp = "239.255.255.250";
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

            ThreadPool.QueueUserWorkItem((state) =>
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
                        byte[] bytes = listener.Receive(ref groupEP);
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
                }
            });
        }

        public void StopListener()
        {
            listening = false;

            if (listener != null)
            {
                listener.Close();
            }
        }
    }
}
