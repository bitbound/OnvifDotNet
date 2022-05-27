using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnvifDotNet.Core.Helpers
{
    public class NetHelper
    {
        public static int GetAvailablePort()
        {

            for (var port = IPEndPoint.MaxPort; port > 0; port--)
            {
                try
                {
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                    if (socket.LocalEndPoint is null)
                    { 
                        continue; 
                    }

                    var foundPort = ((IPEndPoint)socket.LocalEndPoint).Port;
                    return foundPort;
                }
                catch { }
            }
            return -1;
        }

    }
}
