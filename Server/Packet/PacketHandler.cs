using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Contents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_TestHandler(PacketSession session, IMessage packet)
	{
        C_Test testPacket = packet as C_Test;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"Value : {testPacket.Value} // From : {testPacket.Player.Name} ID : {testPacket.Player.PlayerId}");
	}
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move c_MovePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        Player myPlayer = clientSession.MyPlayer;
        if (myPlayer == null)
        {
            Console.WriteLine("NULL PLAYER");
            return;
        }

        Room room = myPlayer.Room;
        if (room == null)
        {
            Console.WriteLine("NULL ROOM");
            return;
        }

        room.HandleMove(myPlayer, c_MovePacket);
        
    }
}
