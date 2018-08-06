using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Musiccast.Helpers
{
    public class UDPListener
    {
        public async Task<int> ListenAsync(long baseAddress, int port)
        {
            await StartListenerAsync(baseAddress, port);

            return 0;
        }

        private async Task StartListenerAsync(long baseAddress, int port)
        {
            bool done = false;
            IPEndPoint groupEP = new IPEndPoint(baseAddress, port);
            UdpClient listener = new UdpClient(groupEP);

            try
            {
                while (!done)
                {
                    Debug.WriteLine("Waiting for broadcast");
                    var result = await listener.ReceiveAsync();
                    var bytes = result.Buffer;
                    Debug.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                listener.Dispose();
            }
        }
    }
}
