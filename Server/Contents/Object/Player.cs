using Google.Protobuf.Protocol;
using Server.Contents.Object;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public int DaggerCount;
        public Dictionary<GameObjectType, int> Equips { get; set; } = new Dictionary<GameObjectType,int>();
        public Dictionary<int, Trigon> Trigons { get; set; } = new Dictionary<int, Trigon>();
        public Dictionary<int, Passive> Passives { get; set; } = new Dictionary<int, Passive>();
        public int Level { get { return StatInfo.Level; } set { StatInfo.Level = value; } }
        public int EXP { get { return StatInfo.Exp; } set { StatInfo.Exp = value; } }


        int _selectCount;
        int _itemRange;
        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
            DaggerCount = 0;
            _itemRange = 2;
        }

        IJob _job;
        public override void Init()
        {
            PosInfo.State = State.Idle;
            StatInfo.Hp = StatInfo.MaxHp;
            StatInfo.Exp = 0;
            _selectCount = 0;
            _isInvincibility = true;
            Room.PushAfter(3000, AfterInvincibility);
            Update();
        }
        
        void AfterInvincibility()
        {
            if(Room == null)
                return;
            _isInvincibility = false;

            Sword t = ObjectManager.Instance.Add<Sword>();
            t.Owner = this;
            t.Room = Room;
            t.PosInfo = PosInfo;
            t.Info.Name = Id.ToString();
            Trigons.Add(t.Id, t);
            Room.Push(Room.EnterRoom, t);

            Air d = ObjectManager.Instance.Add<Air>();
            d.Owner = this;
            d.Room = Room;
            d.PosInfo = PosInfo;
            d.Info.Name = Id.ToString();
            Passives.Add(d.Id, d);
            d.Init();
            Console.WriteLine("Add Dagger");
        }
        public void UpdatePassive(int skillId)
		{
            if (Room == null)
                return;

            Skill skillData;
            
            if (DataManager.SkillDict.TryGetValue(skillId, out skillData))
            {
                int cooldown = (int)skillData.cooldown * 1000;
                _job = Room.PushAfter(cooldown, UpdatePassive, skillId);

                switch (skillData.skillType)
                {
                    case SkillType.SkillArea:
                        {
                            foreach (List<int> list in skillData.area.posList)
                            {
                                Area area = ObjectManager.Instance.Add<Area>();

                                if (area == null)
                                    continue;

                                area.Owner = this;
                                area.Data = skillData;
                                area.Info.Name = $"Area_{area.Id}";
                                area.PosInfo.PosX = CellPos.x + list[0];
                                area.PosInfo.PosY = CellPos.y + list[1];

                                if (area.PosInfo.PosX < 0 || area.PosInfo.PosX >= Room.Map.SizeX || area.PosInfo.PosY < 0 || area.PosInfo.PosY >= Room.Map.SizeY)
                                    continue;

                                Room.Push(Room.EnterRoom, area);
                            }
                        }
                        break;
                    case SkillType.SkillCircler:
                        {
                            Circler circler = ObjectManager.Instance.Add<Circler>();

                            if (circler == null)
                                return;
                           
                            circler.Owner = this;
                            circler.Data = skillData;
                            circler.Info.Name = $"Circler_{circler.Id}";
                            circler.PosInfo.PosX = CellPos.x;
                            circler.PosInfo.PosY = CellPos.y;


                            if (circler.PosInfo.PosX < 0 || circler.PosInfo.PosX >= Room.Map.SizeX || circler.PosInfo.PosY < 0 || circler.PosInfo.PosY >= Room.Map.SizeY)
                                return;

                            //Drones.Add(circler);
                            Room.Push(Room.EnterRoom, circler);
                        }
                        break;
                }        
            }
        }
        bool _slideCooltime = false;
        int _slidetick;
        public override void Update()
        {
            base.Update();
            if (Room == null)
                return;
            // 아이템 획득
            List<Zone> zones = Room.GetAdjacentZones(CellPos);
            foreach (Zone zone in zones)
            {
                foreach (Item i in zone.Items)
                {
                    Vector2Int a = new Vector2Int(i.PosInfo.PosX, i.PosInfo.PosY);
                    if ((a-CellPos).sqrMangnitude < _itemRange * _itemRange)
                    {
                        EarnItem(i);
                    }
                }
            }
            // 스킬 (슬라이딩 쿨타임 관리)
            if (_slideCooltime)
            {
                if (_slidetick <= Environment.TickCount)
                {
                    _slidetick = Environment.TickCount + 5000;
                    _slideCooltime = false;
                }
            }
            _job = Room.PushAfter(100, Update);
        }
        public bool OnSlide()
        {
            Console.WriteLine("OnSlide");
            if (_slideCooltime)
                return true;
            _slidetick = Environment.TickCount + 5000;
            _slideCooltime = true;
            return false;
        }
        void EarnItem(Item item)
        {
            if (item.Destroyed)
                return;

            S_GetItem packet = new S_GetItem();
            packet.PlayerId = Id;
            packet.ItemId = item.Id;

            switch (item.Info.Prefab)
            {
                case 0:
                    OnDamaged(this, -item.value);
                    break;
                case 1:
                    {
                        EarnEXP(item.value);
                    }
                    break;
            }

            item.Destroyed = true;

            Room.Push(Room.Broadcast, CellPos, packet);
            Room.PushAfter(500, Room.LeaveRoom, item.Id);
        }

        public void EarnEXP(int exp)
        {
            EXP += exp;
            S_ChangeStat packet = new S_ChangeStat();
            packet.PlayerId = Id;

            if (EXP >= StatInfo.TotalExp)
            {
                _selectCount++;
                Level++;
                StatInfo stat = null;
                EXP -= StatInfo.TotalExp;
                DataManager.StatDict.TryGetValue(Level, out stat);
                StatInfo.MergeFrom(stat);

                Console.WriteLine($"Level Up {StatInfo.Hp}");

                S_SelectEquip equip = new S_SelectEquip();
                equip.Equips.Add(EquipType.Sword);
                equip.Equips.Add(EquipType.Fire);
                equip.Equips.Add(EquipType.Dagger);
                equip.Equips.Add(EquipType.Lightning);

                if(Session != null)
                    Room.Push(Session.Send, equip);

                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Attack, Value = StatInfo.Attack });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Level, Value = StatInfo.Level });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.MaxHp, Value = StatInfo.MaxHp });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.TotalExp, Value = StatInfo.TotalExp });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Exp, Value = StatInfo.Exp });

                Room.Push(Room.Broadcast, CellPos, packet);

                return;
            }

            packet.Info.Add(new ChangeStatInfo() { Type = StatType.Exp, Value = EXP });
            Room.Push(Room.Broadcast, CellPos, packet);
        }

        public void SelectEquip(EquipType type)
        {
            if (_selectCount <= 0)
                return;
            _selectCount--;
            switch (type)
            {
                case EquipType.Sword:
                    {
                        Sword t = ObjectManager.Instance.Add<Sword>();
                        t.Owner = this;
                        t.Room = Room;
                        t.PosInfo = PosInfo;
                        t.Info.Name = Id.ToString();
                        Trigons.Add(t.Id, t);

                        Room.Push(Room.EnterRoom, t);
                    }
                    break;
                case EquipType.Dagger:
                    if(DaggerCount == 0)
                    {
                        Dagger d = ObjectManager.Instance.Add<Dagger>();
                        d.Owner = this;
                        d.Room = Room;
                        d.PosInfo = PosInfo;
                        d.Info.Name = Id.ToString();
                        Passives.Add(d.Id, d);
                        d.Init();
                        DaggerCount = 1;
                        Console.WriteLine("Add Dagger");
                    }
                    else
                        DaggerCount++;
                    break;
                case EquipType.Air:
                    break;
                case EquipType.Fire:
                    {
                        Fire f = ObjectManager.Instance.Add<Fire>();
                        f.Owner = this;
                        f.Room = Room;
                        f.PosInfo = PosInfo;
                        f.Info.Name = Id.ToString();
                        Passives.Add(f.Id, f);
                        f.Init();

                    }
                    break;
                case EquipType.Earth:
                    break;
                case EquipType.Lightning:
                    {
                        Lightning l = ObjectManager.Instance.Add<Lightning>();
                        l.Owner = this;
                        l.Room = Room;
                        l.PosInfo = PosInfo;
                        l.Info.Name = Id.ToString();
                        Trigons.Add(l.Id, l);
                        l.Init();
                    }
                    break;
            }
            Console.WriteLine($"Player_{Id} : Select {type.ToString()}");
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            switch (attacker.ObjectType)
            {
                case GameObjectType.Player:

                    break;
                case GameObjectType.Area:

                    break;
            }
        }

        protected override void OnDead(GameObject attacker)
		{
            base.OnDead(attacker);

            if (Room == null)
                return;
            if (State == State.Dead)
                return;

            Info.PosInfo.State = State.Dead;

            S_Die packet = new S_Die();
            packet.ObjectId = Id;
            packet.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, packet);

            foreach (Trigon t in Trigons.Values)
            {
                t.Destroy();
            }

            Trigons.Clear();

            foreach (Passive f in Passives.Values)
            {
                f.Destroy();
            }

            Passives.Clear();

            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            Item item = ObjectManager.Instance.Add<Item>();
            item.PosInfo.PosX = PosInfo.PosX;
            item.PosInfo.PosY = PosInfo.PosY;
            item.Info.Prefab = 0;
            item.value = 20;
            Console.WriteLine($"{Id} DEAD And Drop Item {item.Id}");
            Room.Push(Room.EnterRoom, item);

            Room.PushAfter(2000, Respone);
        }
        public override void Respone()
        {
            base.Respone();
 
            if (Room == null)
                return;
            Room.Push(Room.LeaveRoom, Id);
            Room.Push(Room.EnterRoom, this);
        }
    }
}
