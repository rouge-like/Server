using System;
using ServerCore;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using System.Diagnostics;

class PacketHandler
{
    static int count;
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        ServerSession serverSession = session as ServerSession;

        serverSession.p.PosInfo = enterGamePacket.Player.PosInfo;
        serverSession.p.Id = enterGamePacket.Player.ObjectId;
        serverSession.p.Start();
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;

    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        if(movePacket.ObjectId == serverSession.p.Id)
        {
            //Console.WriteLine($"Move {serverSession.p.PosInfo.PosX} ,{serverSession.p.PosInfo.PosY} to {movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}");
            serverSession.p.PosInfo = movePacket.PosInfo;
        }


        //Debug.Log($"{movePacket.PosInfo.State} {movePacket.ObjectId} : {oc.PosInfo} , {movePacket.PosInfo}");
    }
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

        //Debug.Log($"Skill : {skillPacket.Info.SkillId} , {skillPacket.ObjectId}");
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;
    }
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPakcet = new C_Pong();
    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die die = (S_Die)packet;
    }
    public static void S_MoveFloatHandler(PacketSession session, IMessage packet)
    {
        S_MoveFloat s_move = (S_MoveFloat)packet;
    }

    public static void S_HitTrigonHandler(PacketSession session, IMessage packet)
    {
        S_HitTrigon hitTrigon = (S_HitTrigon)packet;
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat stat = (S_ChangeStat)packet;
    }

    public static void S_SelectEquipHandler(PacketSession session, IMessage packet)
    {
        S_SelectEquip equip = (S_SelectEquip)packet;
        ServerSession serverSession = session as ServerSession;

        List<EquipType> list = new List<EquipType>();
        foreach (EquipType e in equip.Equips)
        {
            list.Add(e);
        }
        Random rand = new Random();
        EquipType value = list[rand.Next(list.Count)];

        C_SelectEquip selectPacket = new C_SelectEquip() { Equip = value };
        serverSession.p.Send(selectPacket);
    }
    public static void S_GetItemHandler(PacketSession session, IMessage packet)
    {
        S_GetItem getItem = (S_GetItem)packet;
    }
    public static void S_EquipInfoHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_RankingHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        ServerSession serverSession = session as ServerSession;
        C_Login login = new C_Login();
        Random rand = new Random();
        login.PlayerCode = rand.Next(1048575);
        login.PlayerName = $"Bot{count++}";

        serverSession.p.Send(login);
    }
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_PingInfoHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_DiePlayerHandler(PacketSession session, IMessage packet)
    {
        ServerSession serverSession = session as ServerSession;

        serverSession.p.Dead = true;
    }
    public static void S_TotalDamageHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_ShotProjectileHandler(PacketSession session, IMessage packet)
    {
        
    }
}
