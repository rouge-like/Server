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
        Dictionary<int, Area> _areas = new Dictionary<int, Area>();
        Dictionary<int, Circler> _circlers = new Dictionary<int, Circler>();

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
            Zone zone = GetZone(gameObject.CellPos);
            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(player.Id, player);
                player.Room = this;
                //player.UpdatePassive(1);
                //player.UpdatePassive(3);

                Map.MoveObject(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                zone.Players.Add(player);

                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    player.Vision.Clear();
                    player.Vision.Update();
                }
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(projectile.Id, projectile);
                projectile.Room = this;

                zone.Projectiles.Add(projectile);

                projectile.Update();
            }
            else if (type == GameObjectType.Area)
            {
                Area area = gameObject as Area;
                _areas.Add(area.Id, area);
                area.Room = this;

                zone.Areas.Add(area);

                area.Init();

            }
            else if (type == GameObjectType.Circler)
            {
                Circler circler = gameObject as Circler;
                _circlers.Add(circler.Id, circler);
                circler.Room = this;

                zone.Circlers.Add(circler);

                circler.Update();
            }
            else if (type == GameObjectType.Monster)
            {

            }
            {
                foreach (Player p in zone.FindAll())
                {
                    p.Vision.UpdateImmediately();
                }
            }
        }

        public void LeaveRoom(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
            Vector2Int cellPos;
            if (type == GameObjectType.Player)
            {

                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                cellPos = player.CellPos;
                Map.LeaveObject(player);
                player.Room = null;

                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;

                cellPos = projectile.CellPos;
                Map.LeaveObject(projectile);
                projectile.Room = null;
            }
            else if (type == GameObjectType.Area)
            {
                Area area = null;
                if (_areas.Remove(objectId, out area) == false)
                    return;

                cellPos = area.CellPos;
                GetZone(area.CellPos).Remove(area);
                area.Room = null;
                
            }
            else if (type == GameObjectType.Circler)
            {
                Circler circler = null;
                if (_circlers.Remove(objectId, out circler) == false)
                    return;

                cellPos = circler.CellPos;
                GetZone(circler.CellPos).Remove(circler);
                circler.Room = null;
            }
            else
            {
                return;
            }

            {                
                foreach (Player p in GetZone(cellPos).FindAll())
                {
                    p.Vision.UpdateImmediately();
                }
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
                foreach (Circler circler in player.Drones)
                    circler.UpdateByPlayer();

                Console.WriteLine($"{player.Info.Name} : Move to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}, {s_MovePacket.PosInfo.Dir}");
            }
            else
            {                   
                s_MovePacket.ObjectId = player.Info.ObjectId;
                s_MovePacket.PosInfo = player.Info.PosInfo;
                s_MovePacket.PosInfo.State = State.Idle;
                s_MovePacket.PosInfo.Dir = movePacket.PosInfo.Dir;

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

                            Vector2Int desPos = projectile.GetFrontCellPos();
                            if (Map.CanGo(desPos))
                                EnterRoom(projectile);
                            else
								Console.WriteLine("Cannot Enter Projectile : Wrong Position");
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
