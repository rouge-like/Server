using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Room : JobSerializer
    {
        public int RoomId  { get; set; }

        List<Player> _players = new List<Player>();
        object _lock = new object();

        Map _map = new Map();
        public void Init()
        {
            _map.LoadMap(0);
        }

        public void EnterRoom(Player newPlayer)
        {
            if (newPlayer == null)
                return;
            _players.Add(newPlayer);
            newPlayer.Room = this;

            _map.AddPlayer(new Vector2Int(0,0),newPlayer.Info.PlayerId);

            {
                S_EnterGame enterPacket = new S_EnterGame();
                enterPacket.Player = newPlayer.Info;
                newPlayer.Session.Send(enterPacket);

                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players)
                {
                    if (newPlayer != p)
                        spawnPacket.Players.Add(p.Info);
                }
                newPlayer.Session.Send(spawnPacket);
            }

            {
                S_Spawn spawnPakcet = new S_Spawn();
                spawnPakcet.Players.Add(newPlayer.Info);
                foreach(Player p in _players)
                {
                    if (newPlayer != p)
                        p.Session.Send(spawnPakcet);
                }
            }
        }

        public void LeaveRoom(int playerId)
        {
            Player player = _players.Find(p => p.Info.PlayerId == playerId);
            if (player == null)
                return;

            _players.Remove(player);
            player.Room = null;

            _map.RemovePlayer(playerId);

            {
                S_LeaveGame leavePacket = new S_LeaveGame();
                player.Session.Send(leavePacket);
            }

            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                foreach(Player p in _players)
                {
                    if(player != p)
                        p.Session.Send(despawnPacket);
                }
            }
            
        }
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            S_Move s_MovePacket = new S_Move();

            lock (_lock)
            {               
                if (_map.Cango(new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY), player.Info.PlayerId))
                {
                    player.Info.PosInfo = movePacket.PosInfo;
                    s_MovePacket.PlayerId = player.Info.PlayerId;
                    s_MovePacket.PosInfo = movePacket.PosInfo;

                    Console.WriteLine($"{player.Info.Name} : Move to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}");
                }
                else
                {                   
                    s_MovePacket.PlayerId = player.Info.PlayerId;
                    s_MovePacket.PosInfo = player.Info.PosInfo;
                    s_MovePacket.PosInfo.State = State.Idle;

                    Console.WriteLine($"{player.Info.Name} : Stay to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}");
                }
                Broadcast(s_MovePacket);
            }
        }

        public void Broadcast(IMessage packet)
        {
            foreach(Player p in _players)
            {
                p.Session.Send(packet);
            }
        }
    }
}
