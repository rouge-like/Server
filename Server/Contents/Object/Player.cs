using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Server.Contents.Object;
using Server.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

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

        string _uid;
        int _selectCount;
        int _tick;
        
        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }
        #region DB 
        public Dictionary<EquipType, AdditionalWeaponStat> AdditionalStat = new Dictionary<EquipType, AdditionalWeaponStat>();

        async void GetAdditionalStat()
        {
            if (AdditionalStat.Count > 0)
                return;
            WebRequest request = WebRequest.Create($"https://luckysurvior-5e2d9-default-rtdb.firebaseio.com/users/{_uid}/weapons.json");
            request.Method = "Get";

             using (var response = await request.GetResponseAsync())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                Console.WriteLine($"{Info.Name}'s Data {result}");
                Weapons weapons = JsonConvert.DeserializeObject<Weapons>(result);
                CalAdditionalStats(weapons);
            }
        }

        void CalAdditionalStats(Weapons weapons)
        {
            if (weapons == null)
            {
                AdditionalStat.Add(EquipType.Sword, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Arrow, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Fire, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Lightning, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Ice, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Earth, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Air, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Light, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Dark, new AdditionalWeaponStat());
                AdditionalStat.Add(EquipType.Poison, new AdditionalWeaponStat());
                return;
            }

            CalAdditionalStat(EquipType.Sword, weapons.sword);
            CalAdditionalStat(EquipType.Arrow, weapons.arrow);
            CalAdditionalStat(EquipType.Fire, weapons.fire);
            CalAdditionalStat(EquipType.Lightning, weapons.lightning);
            CalAdditionalStat(EquipType.Ice, weapons.ice);
            CalAdditionalStat(EquipType.Earth, weapons.earth);
            CalAdditionalStat(EquipType.Air, weapons.air);
            CalAdditionalStat(EquipType.Light, weapons.light);
            CalAdditionalStat(EquipType.Dark, weapons.dark);
            CalAdditionalStat(EquipType.Poison, weapons.poison);
        }

        void CalAdditionalStat(EquipType type,WeaponStat weapon)
        {
            AdditionalWeaponStat stat = new AdditionalWeaponStat();

            if (weapon.gems != null)
            {
                foreach (int gem in weapon.gems)
                {
                    int up = gem / 10;
                    int down = gem % 10;
                    CalAdditionalStat(stat, up);
                    CalAdditionalStat(stat, down, false);
                }
            }

            AdditionalStat.Add(type, stat);
        }
        void CalAdditionalStat(AdditionalWeaponStat weapon, int stat, bool up = true)
        {
            int value = 10;
            if (!up)
                value = -10;

            switch (stat)
            {
                case 1:
                    {
                        weapon.attack += value;
                    }
                    break;
                case 2:
                    {
                        weapon.speed += value;
                    }
                    break;
                case 3:
                    {
                        weapon.range += value;
                    }
                    break;
                case 4:
                    {
                        weapon.cooltime += value;
                    }
                    break;
                case 5:
                    {
                        weapon.duraion += value;
                    }
                    break;
            }
        }

        public async void MakeUidByToken(string idToken)
        {
            FirebaseToken token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            _uid = token.Uid;
            Console.WriteLine($"{_uid}");
        }

        #endregion
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
            _tick = Environment.TickCount;
            Room.PushAfter(3000, AfterInvincibility);
            Update();

            GetAdditionalStat();
        }
        void AfterInvincibility()
        {
            if(Room == null)
                return;
            _isInvincibility = false;
            SelectEquip(EquipType.Sword, true);
            //SelectEquip(EquipType.Lightning, true);
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
        public void SetEquips()
        {
            Random rand = new Random();
            int value = rand.Next(100);
            int num;
            if (value < 30 - (PlayerStat.Luck * 10))
                num = 2;
            else if (value < 80)
                num = 3;
            else if (value < 95 - (PlayerStat.Luck * 5))
                num = 4;
            else
                num = 5;

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
            {
                if (list.Count == 0)
                    return;
                for(int i =0;i < list.Count; i++)
                {
                    EquipType type = list[i];
                    equip.Equips.Add(type);
                    list.Remove(type);
                }
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    int v = rand.Next(list.Count);
                    EquipType type = list[v];
                    equip.Equips.Add(type);
                    list.Remove(type);
                }
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
                StatInfo.MaxHp += PlayerStat.MaxHp;
                StatInfo.Attack += PlayerStat.Attack;

                if (_selectCount == 1)
                    SetEquips();
                

                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Attack, Value = StatInfo.Attack });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Level, Value = StatInfo.Level });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.MaxHp, Value = StatInfo.MaxHp });
                packet.Info.Add(new ChangeStatInfo() { Type = StatType.Hp, Value = StatInfo.Hp });
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

        public void SelectEquip(EquipType type, bool force = false)
        {
            if (!force)
            {
                if (_selectCount <= 0)
                    return;

                _selectCount--;
            }

            if (EquipsA.ContainsKey(type))
                EquipsA[type] += 1;
            else
            {
                switch (type)
                {
                    case EquipType.Arrow:
                        {
                            Arrow d = ObjectManager.Instance.Add<Arrow>();
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
                            t.Init();
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
                            PlayerStat.Attack += 2;
                            StatInfo.Attack += 2;
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
                            PlayerStat.MaxHp += 10;
                            StatInfo.MaxHp += 10;
                            StatInfo.Hp = StatInfo.MaxHp;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                            S_ChangeStat heartPacket = new S_ChangeStat();
                            heartPacket.PlayerId = Id;
                            heartPacket.Info.Add(new ChangeStatInfo() { Type = StatType.MaxHp, Value = StatInfo.MaxHp });
                            heartPacket.Info.Add(new ChangeStatInfo() { Type = StatType.Hp, Value = StatInfo.Hp });
                            Room.Push(Room.Broadcast, CellPos, heartPacket);
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
                            PlayerStat.Cooltime += 5;
                            if (EquipsS.ContainsKey(type))
                                EquipsS[type] += 1;
                            else
                                EquipsS.Add(type, 1);
                        }
                        break;
                    case EquipType.Glove:
                        {
                            PlayerStat.WeaponSpeed += 10;
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
            if (_selectCount > 0)
                SetEquips();
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
                        t.Init();

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

            //Console.WriteLine($"{Info.Name} On Damaged, HP : {StatInfo.Hp} by {attacker.Info.Name}");
            packet.EffectId = EffectId;
            packet.ObjectId = Id;
            packet.Hp = StatInfo.Hp;

            StatInfo.Hp = Math.Max(StatInfo.Hp, 0);
            StatInfo.Hp = Math.Min(StatInfo.Hp, StatInfo.MaxHp);

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
            Room.Push(Room.Broadcast,CellPos, packet);

            S_DiePlayer playerPacket = new S_DiePlayer();
            playerPacket.Time = Environment.TickCount - _tick;
            playerPacket.EarnPoint = 0;
            Room.Push(Session.Send, playerPacket);

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

            //Room.PushAfter(2000, Respone);
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
