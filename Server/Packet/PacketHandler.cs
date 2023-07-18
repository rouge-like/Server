﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Contents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
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

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill c_SkillPacket = packet as C_Skill;
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

        room.HandleSkill(myPlayer, c_SkillPacket);
    }
}
