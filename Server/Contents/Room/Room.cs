using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Contents.Object;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Server.Contents
{
    public class Room : JobSerializer
    {
        public const int VisionCells = 10;
        public int RoomId  { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Trigon> _trigons = new Dictionary<int, Trigon>();
        Dictionary<int, Fire> _fires = new Dictionary<int, Fire>();
        Dictionary<int, Item> _items = new Dictionary<int, Item>();

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

            Player DummyPlayer = ObjectManager.Instance.Add<Player>();
            {
                DummyPlayer.Info.Name = $"DummyPlayer_{DummyPlayer.Info.ObjectId}";
                DummyPlayer.Info.PosInfo = new PosInfo();

                DummyPlayer.Session = null;
            }

            EnterRoom(DummyPlayer);
            PushAfter(100, SpawnMonster);
 
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

                StatInfo stat = null;
                DataManager.StatDict.TryGetValue(1, out stat);
                player.StatInfo.MergeFrom(stat);

                player.Init();
              

                Random rand = new Random();
                while (true)
                {
                    Vector2Int random = new Vector2Int(rand.Next(10), rand.Next(5));
                    if (Map.CanGo(random))
                    {
                        player.CellPos = random;
                        break;
                    }
                }
                zone = GetZone(player.CellPos);
                zone.Players.Add(player);
                Map.MoveObject(player, player.CellPos);

                if (player.Session != null)
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);
                    //player.Session.Send(packet);
                }
                player.Vision.Clear();
                player.Vision.Update();
                Console.WriteLine($"{player.Info.Name} Spawn in {player.CellPos.x}, {player.CellPos.y}");
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(projectile.Id, projectile);
                projectile.Room = this;

                zone.Projectiles.Add(projectile);

                projectile.Update();
            }/**
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
            }**/
            else if (type == GameObjectType.Trigon)
            {
                if(gameObject.Info.Prefab == 0)
                {
                    Sword sword = gameObject as Sword;

                    if (sword.Owner.PosInfo.State == State.Dead)
                        return;

                    _trigons.Add(sword.Id, sword);
                    sword.Room = this;

                    zone.Trigons.Add(sword);

                    sword.Init();
                }
                else if(gameObject.Info.Prefab == 1)
                {
                    Lightning lightning = gameObject as Lightning;

                    if (lightning.Owner.PosInfo.State == State.Dead)
                        return;

                    _trigons.Add(lightning.Id, lightning);
                    lightning.Room = this;

                    zone.Trigons.Add(lightning);
                }

            }
            else if (type == GameObjectType.Area)
            {
                Fire fire = gameObject as Fire;

                if (fire.Owner.PosInfo.State == State.Dead)
                    return;

                _fires.Add(fire.Id, fire);
                fire.Room = this;

                zone.Fires.Add(fire);
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(monster.Id, monster);
                monster.Room = this;

                zone = GetZone(monster.CellPos);
                zone.Monsters.Add(monster);
                Map.MoveObject(monster, monster.CellPos);

               //Console.WriteLine($"Monster_{monster.Id} Spawn : {monster.CellPos.x} , {monster.CellPos.y}");

                monster.Init();
            }
            else if (type == GameObjectType.Item)
            {
                Item item = gameObject as Item;
                _items.Add(item.Id, item);
                item.Room = this;
                item.Init();

                zone.Items.Add(item);
            }
            foreach (Zone z in GetAdjacentZones(gameObject.CellPos))
            {
                foreach (Player p in z.FindAll())
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
                    if(player.Session != null)
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
                Console.WriteLine($"Leave Room Projectile_{projectile.Id}");
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;

                cellPos = monster.CellPos;
                Map.LeaveObject(monster);
                monster.Room = null;
            }/**
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
            }**/
            else if (type == GameObjectType.Trigon)
            {
                Trigon trigon = null;
                if (_trigons.Remove(objectId, out trigon) == false)
                    return;

                cellPos = trigon.CellPos;

                if (trigon.Info.Prefab == 0)
                { 
                    GetZone(trigon.CellPos).Remove(trigon);
                    trigon.Room = null;
                    trigon.Owner = null;
                }
                else if(trigon.Info.Prefab == 1)
                {
                    cellPos = trigon.CellPos;
                    GetZone(trigon.CellPos).Remove(trigon);
                }
            }
            else if (type == GameObjectType.Area)
            {
                Fire fire = null;
                if (_fires.Remove(objectId, out fire) == false)
                    return;

                cellPos = fire.CellPos;
                GetZone(fire.CellPos).Remove(fire);
            }
            else if (type == GameObjectType.Item)
            {
                Item item = null;
                if (_items.Remove(objectId, out item) == false)
                    return;

                cellPos = item.CellPos;
                item.Destroy();
                GetZone(item.CellPos).Remove(item);
                item.Room = null;
            }
            else
            {
                return;
            }
            foreach (Zone z in GetAdjacentZones(cellPos))
            {
                foreach (Player p in z.FindAll())
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
            else if (objectType == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.TryGetValue(objectId, out projectile))
                    return projectile;
            }
            else if (objectType == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.TryGetValue(objectId, out monster))
                    return monster;
            }
            return null;
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;          
            
            S_Move s_MovePacket = new S_Move();
            ObjectInfo info = player.Info;

            if (info.PosInfo.State == State.Dead)
                return;

            if (Map.CanGo(new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY)))
            {
                Map.MoveObject(player, new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY));

                info.PosInfo = movePacket.PosInfo;
                s_MovePacket.ObjectId = player.Info.ObjectId;
                s_MovePacket.PosInfo = movePacket.PosInfo;
                foreach (Trigon t in player.Trigons.Values)
                {
                    t.CheckAttack();
                }
                if(!GetZone(player.CellPos).Players.Contains(player))
                    Console.WriteLine($"{player.CellPos.x}, {player.CellPos.y} : ERROR");
                Console.WriteLine($"{player.Info.Name} : Move to {s_MovePacket.PosInfo.PosX},{s_MovePacket.PosInfo.PosY}, {s_MovePacket.PosInfo.Dir}");
            }
            else
            {                   
                s_MovePacket.ObjectId = player.Info.ObjectId;
                s_MovePacket.PosInfo = player.Info.PosInfo;
                s_MovePacket.PosInfo.State = movePacket.PosInfo.State;
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
            if (info.PosInfo.State != State.Idle && info.PosInfo.State != State.Moving)
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
                            if (player.OnSlide())
                                return;

                            player.State = State.Slide;
                            Vector2Int dirVector = player.DirToVector();

                            HandleSlide(player, dirVector, 5);
                        }
                        break;
                    case SkillType.SkillProjectile:
                        {
                            Console.WriteLine($"{skill.ObjectId} Player Use Skill {skill.Info.SkillId}");

                            for(int i = 0; i < player.DaggerCount; i++)
                            {
                                Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                                projectile.Owner = player;
                                projectile.Data = skillData;
                                projectile.Info.Name = $"Projectile_{projectile.Id}";
                                projectile.PosInfo.State = State.Moving;
                                projectile.SetDir(player.PosInfo.Dir);
                                projectile.PosInfo.PosX = player.PosInfo.PosX;
                                projectile.PosInfo.PosY = player.PosInfo.PosY;
                                projectile.Speed = skillData.projectile.speed;

                                Vector2Int desPos = projectile.GetFrontCellPos();
                                int id = Map.FindId(desPos);
                                if (Map.CanGo(desPos))
                                    EnterRoom(projectile);
                                else if (id != 1 && id != 0) 
                                {
                                    GameObject target = Find(id);
                                    target.OnDamaged(projectile.Owner, projectile.Data.damage);
                                }
                                else
                                    Console.WriteLine("Cannot Enter Projectile : Wrong Position");
                            }

                        }
                        break;
                    case SkillType.SkillArea:
                        {
                            
                        }
                        break;
                }
            }
            
        }
        public void HandleSlide(GameObject go, Vector2Int dir, int range = 3)
        {
            S_Move s_MovePacket = new S_Move();

            Vector2Int endPoint;

            if (dir.x == 0 || dir.y == 0)
                endPoint = new Vector2Int((dir.x * range) + go.CellPos.x, (dir.y * range) + go.CellPos.y);
            else
                endPoint = new Vector2Int((dir.x * (int)(range / 1.4f)) + go.CellPos.x, (dir.y * (int)(range / 1.4f)) + go.CellPos.y);
            Map.SlideObject(go, endPoint, dir);

            s_MovePacket.ObjectId = go.Info.ObjectId;
            s_MovePacket.PosInfo = go.PosInfo;

            Broadcast(go.CellPos, s_MovePacket);
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
                    if(p.Session != null)
                        p.Session.Send(packet);
                }
            }
        }

        public List<Zone> GetAdjacentZones(Vector2Int pos, int cells = VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int[] delta = new int[3] {0, -cells, +cells };
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
        void SpawnMonster()
        {
            if(_monsters.Count < 10000)
            {
                for (int i = 0; i < 10; i++)
                {
                    Monster monster1 = ObjectManager.Instance.Add<Monster>();
                    {
                        monster1.Info.Name = $"Slime0_{monster1.Id}";
                    }
                    Random rand = new Random();
                    while (true)
                    {
                        monster1.PosInfo.PosX = rand.Next(100);
                        monster1.PosInfo.PosY = rand.Next(200);
                        if (Map.CanGo(monster1.CellPos))
                            break;
                    }
                    StatInfo stat = null;
                    DataManager.MonsterDict.TryGetValue(1, out stat);
                    monster1.StatInfo.MergeFrom(stat);
                    EnterRoom(monster1);
                }
                for (int i = 0; i < 20; i++)
                {
                    Monster monster2 = ObjectManager.Instance.Add<Monster>();
                    {
                        monster2.Info.Name = $"Slime1_{monster2.Id}";
                    }
                    Random rand = new Random();
                    while (true)
                    {
                        monster2.PosInfo.PosX = rand.Next(100,Map.SizeX);
                        monster2.PosInfo.PosY = rand.Next(200,Map.SizeY);
                        if (Map.CanGo(monster2.CellPos))
                            break;
                    }
                    StatInfo stat = null;
                    DataManager.MonsterDict.TryGetValue(2, out stat);
                    monster2.StatInfo.MergeFrom(stat);
                    EnterRoom(monster2);
                }
                for (int i = 0; i < 20; i++)
                {
                    Monster monster3 = ObjectManager.Instance.Add<Monster>();
                    {
                        monster3.Info.Name = $"Slime3_{monster3.Id}";
                    }
                    Random rand = new Random();
                    while (true)
                    {
                        monster3.PosInfo.PosX = rand.Next(100);
                        monster3.PosInfo.PosY = rand.Next(200, Map.SizeY);
                        if (Map.CanGo(monster3.CellPos))
                            break;
                    }
                    StatInfo stat = null;
                    DataManager.MonsterDict.TryGetValue(3, out stat);
                    monster3.StatInfo.MergeFrom(stat);
                    EnterRoom(monster3);
                }
            }
            else
                Console.WriteLine("Monster Full");


            PushAfter(1000, SpawnMonster);
        }
    }
}
