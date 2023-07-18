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

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        object _lock = new object();

        public Map Map { get; private set; } = new Map();
        public void Init(int num)
        {
            Map.LoadMap(num);
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach(Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterRoom(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            lock (_lock)
            {
                if(type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(player.Id, player);
                    player.Room = this;

                    Map.AddPlayer(new Vector2Int(0, 0), player.Id);

                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;
                        player.Session.Send(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player p in _players.Values)
                        {
                            if (player != p)
                                spawnPacket.Objects.Add(p.Info);
                        }
                        player.Session.Send(spawnPacket);
                    }
                }
                else if(type == GameObjectType.Projectile)
                {
                    Projectile projectile = gameObject as Projectile;
                    _projectiles.Add(projectile.Id, projectile);
                    projectile.Room = this;
                }
                else if(type == GameObjectType.Monster)
                {
                    
                }

                {
                    S_Spawn spawnPakcet = new S_Spawn();
                    spawnPakcet.Objects.Add(gameObject.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPakcet);
                    }
                }

            }
        }

        public void LeaveRoom(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {

                    Player player = null;
                    if (_players.Remove(objectId, out player) == false)
                        return;

                    player.Room = null;
                    Map.RemovePlayer(objectId);

                    {
                        S_LeaveGame leavePacket = new S_LeaveGame();
                        player.Session.Send(leavePacket);
                    }
                }
                else if(type == GameObjectType.Projectile)
                {
                    Projectile projectile = null;
                    if (_projectiles.Remove(objectId, out projectile) == false)
                        return;

                    projectile.Room = null;
                }

                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(objectId);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                            p.Session.Send(despawnPacket);
                    }
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
                if (Map.CanGo(new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY)))
                {
                    Map.MovePlayer(new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY), player.Id);

                    player.Info.PosInfo = movePacket.PosInfo;
                    s_MovePacket.ObjectId = player.Info.ObjectId;
                    s_MovePacket.PosInfo = movePacket.PosInfo;

                    Console.WriteLine($"{player.Info.Name} : Move to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}");
                }
                else
                {                   
                    s_MovePacket.ObjectId = player.Info.ObjectId;
                    s_MovePacket.PosInfo = player.Info.PosInfo;
                    s_MovePacket.PosInfo.State = State.Idle;

                    Console.WriteLine($"{player.Info.Name} : Stay to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}");
                }
                Broadcast(s_MovePacket);
            }
        }
        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                ObjectInfo info = player.Info;
                if (info.PosInfo.State != State.Idle)
                    return;

                info.PosInfo.State = State.Skill;;
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = info.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                if(skillPacket.Info.SkillId == 1)
                {
                    Vector2Int skillPos = player.GetFrontCellPos();
                    int targetId = Map.FindId(skillPos);
                    if(targetId != 0)
                        Console.WriteLine("TESTING SKILL 1");
                }

                else if(skillPacket.Info.SkillId == 2)
                {
                    Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                    if(projectile == null)
                        return;

                    projectile.Owner = player;
                    projectile.PosInfo.State = State.Moving;
                    projectile.PosInfo.Dir = player.PosInfo.Dir;
                    projectile.PosInfo.PosX = player.PosInfo.PosX;
                    projectile.PosInfo.PosY = player.PosInfo.PosY;
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            foreach(Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
