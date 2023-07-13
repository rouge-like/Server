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

        public void EnterRoom(Player newPlayer)
        {
            if (newPlayer == null)
                return;
            _players.Add(newPlayer);
            newPlayer.Room = this;

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

        public void Broadcast(IMessage packet)
        {
            foreach(Player p in _players)
            {
                p.Session.Send(packet);
            }
        }
    }
}
