﻿using CEngineSharp_Server.World;
using SharpNetty;
using System;

namespace CEngineSharp_Server.Net.Packets
{
    public class PlayerDataPacket : Packet
    {
        public void WriteData(Player player)
        {
            try
            {
                this.DataBuffer.Flush();
                this.DataBuffer.WriteInteger(player.PlayerIndex);
                this.DataBuffer.WriteString(player.Name);
                this.DataBuffer.WriteInteger(player.Level);
                this.DataBuffer.WriteInteger(player.Position.X);
                this.DataBuffer.WriteInteger(player.Position.Y);
                this.DataBuffer.WriteByte(player.Direction);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void Execute(Netty netty, int socketIndex)
        {
            // Server->Client packet only.
        }

        public override string PacketID
        {
            get { return "PlayerDataPacket"; }
        }
    }
}