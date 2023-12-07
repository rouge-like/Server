using Google.Protobuf.Protocol;
using Server.Contents.Object;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Server.Contents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public Dictionary<EquipType, int> EquipsA { get; set; } = new Dictionary<EquipType, int>();
        public Dictionary<EquipType, int> EquipsS { get; set; } = new Dictionary<EquipType, int>();
        public Dictionary<int, Trigon> Trigons { get; set; } = new Dictionary<int, Trigon>();
        public Dictionary<int, Passive> Passives { get; set; } = new Dictionary<int, Passive>();
        public int Level { get { return StatInfo.Level; } set { StatInfo.Level = value; } }
        public int EXP { get { return StatInfo.Exp; } set { StatInfo.Exp = value; } }
        public PlayerStatInfo PlayerStat = new PlayerStatInfo();

        int _selectCount;
        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

        IJob _job;
        public override void Init()
        {
            PosInfo.State = State.Idle;
            StatInfo.Hp = StatInfo.MaxHp;
            StatInfo.Exp = 0;
            PlayerStat.ItemRange = 2;
            PlayerStat.SlideCooltime = 50;
            PlayerStat.Number = 0;
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
            _selectCount += 1;
            SelectEquip(EquipType.Sword);
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
                    if ((a-CellPos).sqrMangnitude < PlayerStat.ItemRange * PlayerStat.ItemRange)
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
                    _slidetick = Environment.TickCount + (PlayerStat.SlideCooltime * 100);
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
            _slidetick = Environment.TickCount + (PlayerStat.SlideCooltime * 100);
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
        void SetEquips(int num)
        {
            S_SelectEquip equip = new S_SelectEquip();
            List<EquipType> list = new List<EquipType>();
            if(EquipsA.Count == 5)
            {
                foreach (EquipType e in EquipsA.Keys)
                {
                    if (EquipsA[e] < 8)
                        list.Add(e);
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                    list.Add((EquipType)i);
            }
            if(EquipsS.Count == 5)
            {
                foreach (EquipType e in EquipsS.Keys)
                {
                    if (EquipsS[e] < 8)
                        list.Add(e);
                }
            }
            else
            {
                for (int i = 10; i < 20; i++)
                    list.Add((EquipType)i);
            }
            if (list.Count < num)
                num = list.Count;
            for (int i = 0; i < num; i++)
            {
                Random rand = new Random();
                int value = rand.Next(list.Count - i);
                EquipType type = list[value];
                equip.Equips.Add(type);
                list.Remove(type);
            }

            if (Session != null)
                Room.Push(Session.Send, equip);
        }

        public void EarnEXP(int exp)
        {
            EXP += (int)(exp + (exp * (PlayerStat.AdditionalExp / 100f)));
            S_ChangeStat packet = new S_ChangeStat();
            packet.PlayerId = Id;

            if (EXP >= StatInfo.TotalExp)
            {
                _selectCount++;
                Level++;
                StatInfo.Hp = StatInfo.MaxHp;
                StatInfo stat = null;
                EXP -= StatInfo.TotalExp;
                DataManager.StatDict.TryGetValue(Level, out stat);
                StatInfo.MergeFrom(stat);

                Random rand = new Random();
                int value = rand.Next(100);
                int num;
                if (value < 30 - (PlayerStat.Luck * 10))
                    num = 2;
                else if (value < 80)
                    num = 3;
                else if (value < 95 -(PlayerStat.Luck * 5))
                    num = 4;
                else
                    num = 5;
                SetEquips(num);

                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Attack, Value = StatInfo.Attack });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Level, Value = StatInfo.Level });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.MaxHp, Value = StatInfo.MaxHp });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.TotalExp, Value = StatInfo.TotalExp });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Exp, Value = StatInfo.Exp });

                Room.Push(Room.Broadcast, CellPos, packet);
                Room.Push(Room.RankingSet);
                return;
            }

            packet.Info.Add(new ChangeStatInfo() { Type = StatType.Exp, Value = EXP });
            Room.Push(Room.Broadcast, CellPos, packet);
        }

        void ChangeStat(StatType type, int value)
        {
            S_ChangeStat packet = new S_ChangeStat();
            packet.PlayerId = Id;
            packet.Info.Add(new ChangeStatInfo() { Type = type, Value = value });
            Room.Push(Room.Broadcast, CellPos, packet);
        }

        public void SelectEquip(EquipType type)
        {
            if (_selectCount <= 0)
                return;

            _selectCount--;

            if (EquipsA.ContainsKey(type))
                EquipsA[type] += 1;
            else
            {
                switch (type)
                {
                    case EquipType.Dagger:
                        {
                            Dagger d = ObjectManager.Instance.Add<Dagger>();
                            d.Owner = this;
                            d.Room = Room;
                            d.PosInfo = PosInfo;
                            d.Info.Name = Id.ToString();
                            Passives.Add(d.Id, d);
                            d.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Sword:
                        {
                            Sword t = ObjectManager.Instance.Add<Sword>();
                            t.Owner = this;
                            t.Room = Room;
                            t.PosInfo = PosInfo;
                            t.Info.Name = Id.ToString();
                            Trigons.Add(t.Id, t);
                            EquipsA.Add(type, 1);

                            Room.Push(Room.EnterRoom, t);
                        }
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
                            EquipsA.Add(type, 1);
                        }
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
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Ice:
                        {
                            Ice i = ObjectManager.Instance.Add<Ice>();
                            i.Owner = this;
                            i.Room = Room;
                            i.PosInfo = PosInfo;
                            i.Info.Name = Id.ToString();
                            Passives.Add(i.Id, i);
                            i.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Earth:
                        {
                            Earth e = ObjectManager.Instance.Add<Earth>();
                            e.Owner = this;
                            e.Room = Room;
                            e.PosInfo = PosInfo;
                            e.Info.Name = Id.ToString();
                            Passives.Add(e.Id, e);
                            e.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Air:
                        {
                            Air a = ObjectManager.Instance.Add<Air>();
                            a.Owner = this;
                            a.Room = Room;
                            a.PosInfo = PosInfo;
                            a.Info.Name = Id.ToString();
                            Passives.Add(a.Id, a);
                            a.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Light:
                        {
                            Light i = ObjectManager.Instance.Add<Light>();
                            i.Owner = this;
                            i.Room = Room;
                            i.PosInfo = PosInfo;
                            i.Info.Name = Id.ToString();
                            Passives.Add(i.Id, i);
                            i.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Dark:
                        {
                            Dark d = ObjectManager.Instance.Add<Dark>();
                            d.Owner = this;
                            d.Room = Room;
                            d.PosInfo = PosInfo;
                            d.Info.Name = Id.ToString();
                            Trigons.Add(d.Id, d);
                            d.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Poison:
                        {
                            Poison p = ObjectManager.Instance.Add<Poison>();
                            p.Owner = this;
                            p.Room = Room;
                            p.PosInfo = PosInfo;
                            p.Info.Name = Id.ToString();
                            Passives.Add(p.Id, p);
                            p.Init();
                            EquipsA.Add(type, 1);
                        }
                        break;
                    case EquipType.Mushroom:
                        {
                            StatInfo.Attack += 10;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }

                        break;
                    case EquipType.Shield:
                        {
                            PlayerStat.Defense += 10;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }

                        break;
                    case EquipType.Heart:
                        {
                            StatInfo.MaxHp += 10;
                            StatInfo.Hp = StatInfo.MaxHp;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                            ChangeStat(StatType.MaxHp, StatInfo.MaxHp);
                        }


                        break;
                    case EquipType.Necklace:
                        {
                            PlayerStat.AdditionalExp += 10;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Shoes:
                        {
                            PlayerStat.SlideCooltime -= 5;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                            ChangeStat(StatType.Cooltime, PlayerStat.SlideCooltime);
                        }
                        break;
                    case EquipType.Magnet:
                        {
                            PlayerStat.ItemRange += 1;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Clover:
                        {
                            PlayerStat.Luck += 1;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Book:
                        {
                            PlayerStat.Cooltime += 10;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Glove:
                        {
                            PlayerStat.AttackSpeed += 10;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Ring:
                        {
                            PlayerStat.Number += 1;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                }
            }

            S_EquipInfo packet = new S_EquipInfo();
            packet.Equip = type;
            if (EquipsA.ContainsKey(type))
                packet.Level = EquipsA[type];
            else
                packet.Level = EquipsS[type];
            if(Session != null)
                Room.Push(Session.Send, packet);

            Console.WriteLine($"Player_{Id} : Select {type.ToString()}");
            CheckTrigonNumber();
        }
        void CheckTrigonNumber(int tick = 0, int degree = 0)
        {
            List<Sword> swords = new List<Sword>();
            List<Lightning> lightnings = new List<Lightning>();
            List<Dark> darks = new List<Dark>();

            foreach(Trigon t in Trigons.Values)
            {
                switch (t.Info.Prefab)
                {
                    case 0:
                        swords.Add((Sword)t);
                        break;
                    case 1:
                        lightnings.Add((Lightning)t);
                        break;
                    case 2:
                        darks.Add((Dark)t);
                        break;
                }
            }

            if(swords.Count != 0)
            {
                SwordInfo data = null;
                DataManager.SwordDict.TryGetValue(EquipsA[EquipType.Sword], out data);
                int d = data.number + PlayerStat.Number - swords.Count;

                Console.WriteLine($"SwordCount {data.number + PlayerStat.Number} , {swords.Count}");
                if (d > 0)
                {
                    for(int i = 0; i < d; i++)
                    {
                        Sword t = ObjectManager.Instance.Add<Sword>();
                        t.Owner = this;
                        t.Room = Room;
                        t.PosInfo = PosInfo;
                        t.Info.Name = Id.ToString();
                        Trigons.Add(t.Id, t);

                        Room.Push(Room.EnterRoom, t);
                    }
                }
            }
            if (lightnings.Count != 0)
            {
                LightningInfo data = null;
                DataManager.LightningDict.TryGetValue(EquipsA[EquipType.Lightning], out data);
                int n = data.number + PlayerStat.Number;

                Console.WriteLine($"LightningCount {data.number + PlayerStat.Number} , {lightnings.Count}");
                if (n != lightnings.Count)
                {
                    foreach (Lightning l in lightnings)
                        l.Destroy();
                    for (int i = 0; i < n; i++)
                    {
                        Lightning l = ObjectManager.Instance.Add<Lightning>();
                        l.Owner = this;
                        l.Room = Room;
                        l.PosInfo = PosInfo;
                        l.Info.Name = Id.ToString();
                        Trigons.Add(l.Id, l);
                        l.Init();
                        l.Degree = (360 / n) * (i); 
                        l.Tick = 0;
                    }
                }
            }
            if (darks.Count != 0)
            {
                DarkInfo data = null;
                DataManager.DarkDict.TryGetValue(EquipsA[EquipType.Dark], out data);
                int d = data.number + PlayerStat.Number - darks.Count;

                Console.WriteLine($"DarkCount {data.number + PlayerStat.Number} , {darks.Count}");
                if (d > 0)
                {
                    foreach (Dark dark in darks)
                        dark.Tick = 0;
                    for (int i = 0; i < d; i++)
                    {
                        Dark dark = ObjectManager.Instance.Add<Dark>();
                        dark.Owner = this;
                        dark.Room = Room;
                        dark.PosInfo = PosInfo;
                        dark.Info.Name = Id.ToString();
                        Trigons.Add(dark.Id, dark);
                        dark.Init();
                        dark.Degree = darks[darks.Count - 1].Degree + (30*(i+1));
                        dark.Tick = 0;
                    }
                }
            }
        }
        public override void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;
            if (_isInvincibility || State == State.Dead)
                return;
            S_ChangeHp packet = new S_ChangeHp();
            int EffectId = GetEffectNum(attacker);
            if (EffectId == -1)
                return;

            StatInfo.Hp -= (int)(damage - (damage * (PlayerStat.Defense / 100f)));
            StatInfo.Hp = Math.Max(StatInfo.Hp, 0);

            //Console.WriteLine($"{Info.Name} On Damaged, HP : {StatInfo.Hp} by {attacker.Info.Name}");
            packet.EffectId = EffectId;
            packet.ObjectId = Id;
            packet.Hp = StatInfo.Hp;
            Room.Push(Room.Broadcast, CellPos, packet);

            if (StatInfo.Hp <= 0)
            {
                OnDead(attacker);
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
            EquipsA.Clear();

            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            for (int i = 0; i < StatInfo.Level; i++)
            {
                Random rand = new Random();
                int v = rand.Next(10);

                Item item = ObjectManager.Instance.Add<Item>();
                item.PosInfo.PosX = PosInfo.PosX;
                item.PosInfo.PosY = PosInfo.PosY;
                if (v > 7)
                    item.Info.Prefab = 0;
                else
                    item.Info.Prefab = 1;
                item.value = 10;
                Room.Push(Room.EnterRoom, item);
            }

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
