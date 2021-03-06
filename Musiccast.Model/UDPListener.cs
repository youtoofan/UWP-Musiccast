﻿using Musiccast.Model;
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
    public class UDPListener : IDisposable
    {
        public event EventHandler<string> DeviceNotificationRecieved;

        bool _disposed = false;
        private Socket _udpSocket;
        const string _ssdpMulticastIp = "239.255.255.250";
        const int _endpointPort = 1900;
        private IPEndPoint _multicastEndPoint;

        /// <summary>
        /// Starts the listener.
        /// </summary>
        /// <param name="port">The port.</param>
        public void StartListener(int port)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

                    var multicastIp = IPAddress.Parse(_ssdpMulticastIp);
                    _multicastEndPoint = new IPEndPoint(multicastIp, _endpointPort);
                    _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    var option = new MulticastOption(multicastIp);
                    _udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);
                    _udpSocket.Bind(localEndPoint);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                byte[] receiveBuffer = new byte[64000];

                while (DeviceNotificationRecieved != null)
                {
                    if (!_disposed && _udpSocket.Available > 0)
                    {
                        var receivedBytes = _udpSocket.Receive(receiveBuffer, SocketFlags.None);

                        if (receivedBytes > 0)
                        {
                            var temp = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
                            Debug.WriteLine(temp);
                            var result = JsonConvert.DeserializeObject<MusicCastNotification>(temp);
                            if (result != null && !string.IsNullOrEmpty(result.device_id))
                            {
                                string id = result.device_id;
                                DeviceNotificationRecieved(this, id);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                //_udpSocket.Close();
                _udpSocket.Shutdown(SocketShutdown.Both);
                _udpSocket.Dispose();
            }

            _disposed = true;
        }
    }
}
