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
            Console.WriteLine("NULL PLAYER at Move");
            return;
        }

        Room room = myPlayer.Room;
        if (room == null)
        {
            Console.WriteLine("NULL ROOM");
            return;
        }

        room.Push(room.HandleMove, myPlayer, c_MovePacket);
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill c_SkillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player myPlayer = clientSession.MyPlayer;
        if (myPlayer == null)
        {
            Console.WriteLine("NULL PLAYER at Skill");
            return;
        }

        Room room = myPlayer.Room;
        if (room == null)
        {
            Console.WriteLine("NULL ROOM");
            return;
        }

        room.Push(room.HandleSkill, myPlayer, c_SkillPacket);
    }

    public static void C_PongHandler(PacketSession session, IMessage packet) 
    {
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandlePong();
    }

    public static void C_SelectEquipHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_SelectEquip c_equip = (C_SelectEquip)packet;

        Player myPlayer = clientSession.MyPlayer;
        Room room = myPlayer.Room;

        room.Push(myPlayer.SelectEquip, c_equip.Equip, false);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_Login login = (C_Login)packet;
        Console.WriteLine("Loging....");
        Player player = ObjectManager.Instance.Add<Player>();
        {
            player.Info.Name = $"{login.PlayerName}";
            player.Info.Prefab = login.PlayerCode;
            player.Info.PosInfo = new PosInfo();
            player.Session = clientSession;
        }

        clientSession.MyPlayer = player;

        RoomManager.Instance.Push(() =>
        {
            Room room = RoomManager.Instance.Find(1);
            room.Push(player.MakeUidByToken, login.Token);
            room.Push(room.EnterRoom, player);
        });
    }
    public static void C_RespawnOrExitHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_RespawnOrExit respawnOrExit = (C_RespawnOrExit)packet;

        Player player = clientSession.MyPlayer;
        Room room = player.Room;

        if (respawnOrExit.Exit)
        {
            clientSession.Disconnect();
        }
        else
        {
            room.Push(player.Respone);
        }
    }
}
