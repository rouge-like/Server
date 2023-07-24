using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Contents
{
    public class Room : JobSerializer
    {
        public const int VisionCells = 5;
        public int RoomId  { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();
        public Zone GetZone(Vector2Int pos)
        {
            int x = pos.x / ZoneCells;
            int y = pos.y / ZoneCells;

            if (x < 0 || x >= Zones.GetLength(0) || y < 0 || y >= Zones.GetLength(1))
                return null;

            return Zones[x, y];
        }
        public void Init(int num, int zoneCells)
        {
            Map.LoadMap(num);
            Map.Room = this;

            ZoneCells = zoneCells;
            int countX = (Map.SizeX + ZoneCells - 1) / ZoneCells;
            int countY = (Map.SizeY + ZoneCells - 1) / ZoneCells;
            Zones = new Zone[countX, countY];
            for(int y = 0; y < countY; y++)
            {
                for(int x = 0; x < countX; x++)
                {
                    Zones[x, y] = new Zone(x, y);
                }
            }

        }

        public void Update()
        {
            Flush();
        }

        public void EnterRoom(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            if(type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(player.Id, player);
                player.Room = this;

                Map.MoveObject(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                GetZone(player.CellPos).Players.Add(player);

                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    player.Vision.Update();
                }
            }
            else if(type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(projectile.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);
                projectile.Update();
            }
            else if(type == GameObjectType.Monster)
            {
                    
            }
        }

        public void LeaveRoom(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
            if (type == GameObjectType.Player)
            {

                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.Room = null;
                GetZone(player.CellPos).Players.Remove(player);

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

                GetZone(projectile.CellPos).Projectiles.Remove(projectile);
                projectile.Room = null;
            }  
        }
        public GameObject Find(int objectId)
        {
            GameObjectType objectType = ObjectManager.GetObjectTypeById(objectId);

            if (objectType == GameObjectType.Player)
            {
                Player player = null;
                if (_players.TryGetValue(objectId, out player))
                    return player;
            }

            if (objectType == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.TryGetValue(objectId, out projectile))
                    return projectile;
            }
            return null;
        }
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;          

            S_Move s_MovePacket = new S_Move();
            ObjectInfo info = player.Info;

            if (Map.CanGo(new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY)))
            {
                Map.MoveObject(player, new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY));

                info.PosInfo = movePacket.PosInfo;
                s_MovePacket.ObjectId = player.Info.ObjectId;
                s_MovePacket.PosInfo = movePacket.PosInfo;

                Console.WriteLine($"{player.Info.Name} : Move to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}, {s_MovePacket.PosInfo.Dir}");
            }
            else
            {                   
                s_MovePacket.ObjectId = player.Info.ObjectId;
                s_MovePacket.PosInfo = player.Info.PosInfo;
                s_MovePacket.PosInfo.State = State.Idle;

                Console.WriteLine($"{player.Info.Name} : Stay to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}, {player.Info.PosInfo.Dir}");
            }
            Broadcast(player.CellPos, s_MovePacket);

        }
        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != State.Idle)
                return;

            info.PosInfo.State = State.Skill;;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(player.CellPos, skill);

            Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData))
            {
                switch (skillData.skillType)
                {
                    case SkillType.SkillNone:
                        {
                            Vector2Int skillPos = player.GetFrontCellPos();
                            int targetId = Map.FindId(skillPos);
                            if (targetId != 0)
                                Console.WriteLine("TESTING SKILL 1");
                        }
                        break;
                    case SkillType.SkillProjectile:
                        {
                            Console.WriteLine($"{skill.ObjectId} Player Use Skill {skill.Info.SkillId}");
                            Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                            if (projectile == null)
                                return;

                            projectile.Owner = player;
                            projectile.Data = skillData;
                            projectile.Info.Name = $"Projectile_{projectile.Id}";
                            projectile.PosInfo.State = State.Moving;
                            projectile.SetDir(player.PosInfo.Dir);
                            projectile.PosInfo.PosX = player.PosInfo.PosX;
                            projectile.PosInfo.PosY = player.PosInfo.PosY;
                            projectile.Speed = skillData.projectile.speed;

                            EnterRoom(projectile);
                        }
                        break;
                    case SkillType.SkillArea:
                        {

                        }
                        break;
                }
            }
            
        }

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            foreach(Zone zone in zones)
            {
                foreach(Player p in zone.Players)
                {
                    int dx = p.CellPos.x - pos.x;
                    int dy = p.CellPos.y - pos.y;

                    if (Math.Abs(dx) > VisionCells || Math.Abs(dy) > VisionCells)
                        continue;

                    p.Session.Send(packet);
                }
            }
        }

        public List<Zone> GetAdjacentZones(Vector2Int pos, int cells = VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int[] delta = new int[2] { -cells, +cells };
            foreach(int dy in delta)
            {
                foreach (int dx in delta)
                {
                    int x = pos.x + dx;
                    int y = pos.y + dy;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }
    }
}
