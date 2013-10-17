﻿using CEngineSharp_Client.Net.Packets;
using SharpNetty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEngineSharp_Client.Net
{
    public static class Networking
    {
        private static NettyClient nettyClient;

        public static void Connect()
        {
            nettyClient = new NettyClient(true);

            if (nettyClient.Connected)
                nettyClient.Disconnect();

            nettyClient.Handle_ConnectionLost = Handle_ConnectionLost;

            nettyClient.Connect("127.0.0.1", 25566, 1);
        }

        public static void Disconnect()
        {
            nettyClient.Disconnect();
        }

        private static void Handle_ConnectionLost()
        {
            nettyClient = null;
        }

        public static void SendPacket(Packet packet)
        {
            nettyClient.SendPacket(packet);
        }
    }
}