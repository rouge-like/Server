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

        Console.WriteLine($"{clientSession.MyPlayer.Info.Name} : {c_MovePacket.PosInfo.PosX}, {c_MovePacket.PosInfo.PosY}");

        if (clientSession.MyPlayer == null)
        {
            Console.WriteLine("NULL PLAYER");
            return;
        }
        if (clientSession.MyPlayer.Room == null)
        {
            Console.WriteLine("NULL ROOM");
            return;
        }

        // 검증

        clientSession.MyPlayer.Info.PosInfo = c_MovePacket.PosInfo; 

        S_Move s_MovePacket = new S_Move();
        s_MovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        s_MovePacket.PosInfo = clientSession.MyPlayer.Info.PosInfo;

        clientSession.MyPlayer.Room.Broadcast(s_MovePacket);

    }
}
