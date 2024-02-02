using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Google.Protobuf.Protocol;
using Server.Contents.Object;
using Server.Data;

namespace Server.Contents
{
	public class TargetMonster : Monster, IWeaponAble
	{
		public Dictionary<int, Trigon> Trigons { get; set; } = new Dictionary<int, Trigon>();
		public Dictionary<int, Passive> Passives { get; set; } = new Dictionary<int, Passive>();
		public Dictionary<EquipType, AdditionalWeaponStat> AdditionalStat { get; set; } = new Dictionary<EquipType, AdditionalWeaponStat>();
		public Dictionary<EquipType, int> EquipsA { get; set; } = new Dictionary<EquipType, int>();
		public Dictionary<EquipType, int> EquipsS { get; set; } = new Dictionary<EquipType, int>();
        public Dictionary<EquipType, int> TotalDamages { get; set; } = new Dictionary<EquipType, int>();
        public PlayerStatInfo PlayerStat { get; set; } = new PlayerStatInfo();
        public Player Target { get { return _target; } set { _target = value; } }
        EquipType _weaponCode;
        bool _upgraded;

        public TargetMonster()
		{
			ObjectType = GameObjectType.Monster;
		}

        protected override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);

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

        }
        public override void Respone()
        {
            base.Respone();
            if(_target != null)
            {
                Room.PushAfter(900, SetTartget, _target, _upgraded);
                Room.PushAfter(10000, Room.EnterRoom, this);
            }  
        }
        public override void Init()
		{
            if(_target.State == State.Dead || _target.StatInfo.Level < 10)
            {
                Room.Push(Room.LeaveRoom, Id);
                return;
            }

            State = State.Moving;

            AdditionalStat = new Dictionary<EquipType, AdditionalWeaponStat>
            {
                { EquipType.Sword, new AdditionalWeaponStat() },
                { EquipType.Lightning, new AdditionalWeaponStat() },
                { EquipType.Dark, new AdditionalWeaponStat() },
                { EquipType.Light, new AdditionalWeaponStat() },
                { EquipType.Fire, new AdditionalWeaponStat() },
                { EquipType.Poison, new AdditionalWeaponStat() },
                { EquipType.Earth, new AdditionalWeaponStat() },
                { EquipType.Air, new AdditionalWeaponStat() },
                { EquipType.Arrow, new AdditionalWeaponStat() },
                { EquipType.Ice, new AdditionalWeaponStat() },
            };
            PlayerStat = _target.PlayerStat;

            Random random = new Random();
            int weaponLevel = Math.Min(_target.Level / 10, 8);
            
            if (_upgraded)
            {
                StatInfo.MaxHp += 40 * weaponLevel;
                IsMetal = false;
                Info.Prefab = 6;
                Info.Name = "Angel Slime";
                AddEquip(EquipType.Light);
                EquipsA[EquipType.Light] = weaponLevel;             
            }
            else
            {
                List<EquipType> list = new List<EquipType>
                {
                    EquipType.Fire,
                    EquipType.Lightning,
                    EquipType.Dark,
                    EquipType.Sword
                };
                if (list.Count != 0)
                {
                    _weaponCode = list[random.Next(list.Count)];

                    AddEquip(_weaponCode);
                    EquipsA[_weaponCode] = weaponLevel;

                    if (_weaponCode == EquipType.Sword)
                    {
                        StatInfo.MaxHp += 40 * weaponLevel;
                        IsMetal = true;
                        Info.Prefab = 4;
                        Info.Name = "Viking Slime";
                    }
                    else
                    {
                        StatInfo.MaxHp += 40 * weaponLevel;
                        IsMetal = false;
                        Info.Prefab = 5;
                        Info.Name = "Magician Slime";
                    }
                }
            }
            StatInfo.Hp = StatInfo.MaxHp;
            CheckTrigonNumber();
            Room.PushAfter(500, Update);
        }
        public override IWeaponAble GetWeapon()
        {
            return this;
        }

        public void SetTartget(Player player, bool upgraded = false)
        {
            if (player == null || player.Room == null)
                return;

            _target = player;
            _upgraded = upgraded;
            
            Vector2Int dest = new Vector2Int();
            while (true)
            {
                Random random = new Random();
                Vector2Int dist = new Vector2Int(random.Next(-20, 20), random.Next(-20, 20));

                dest = _target.CellPos + dist;

                float distance = dist.magnitude;

                if (_target.Room.Map.CanGo(dest) && distance > 10.0f)
                    break;
            }
            CellPos = dest;
        }

        protected override void UpdateIdle()
        {
            
        }
        protected override void UpdateMoving()
		{
            if (_moveTick > (Environment.TickCount & Int32.MaxValue))
                return;
            int moveTick = (int)(1000 / Speed);
            _moveTick = Environment.TickCount & Int32.MaxValue + moveTick;
            
            if (_target == null || _target.Room != Room || _target.State == State.Dead)
            {
                OnDamaged(_target, StatInfo.Hp);
                _target = null;

                return;
            }
            Vector2Int dir = _target.CellPos - CellPos;
            float dist = dir.magnitude;
            if (dist > 20)
                Room.Push(Room.Teleport, this, _target);
            if (dist <= 1.5)
            {
                State = State.Skill;
                return;
            }

            Dir destDir = GetDirFromVec(dir);
            Vector2Int destPos = GetFrontCellPos(destDir);

            for (int i = 0; i < 8; i++)
            {
                if (Room.Map.CanGo(destPos))
                {
                    Dir = destDir;
                    Room.Map.MoveObject(this, destPos);
                    break;
                }
                else
                {
                    destDir = GetRightDir(destDir);
                    destPos = GetFrontCellPos(destDir);
                }

            }

            BroadcastMove();
        }
        protected override void UpdateSkill()
        {
            if (_target == null || _target.Room != Room || _target.State == State.Dead)
            {
                OnDamaged(_target, StatInfo.Hp);
                _target = null;

                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            float dist = dir.magnitude;
            Dir = GetDirFromVec(dir);

            if (dist > 1.5f)
            {
                State = State.Moving;
                BroadcastMove();
                return;
            }
            _target.OnDamaged(this, StatInfo.Attack);
            BroadcastMove();
        }
        public void AddEquip(EquipType type)
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
            }        
        }
        public void SetDamages(EquipType equip, int damage)
        {

        }

        void CheckTrigonNumber(int tick = 0, int degree = 0)
        {
            List<Sword> swords = new List<Sword>();
            List<Lightning> lightnings = new List<Lightning>();
            List<Dark> darks = new List<Dark>();

            foreach (Trigon t in Trigons.Values)
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

            if (swords.Count != 0)
            {
                SwordInfo data = null;
                DataManager.SwordDict.TryGetValue(EquipsA[EquipType.Sword], out data);
                int d = data.number + PlayerStat.Number - swords.Count;

                if (d > 0)
                {
                    for (int i = 0; i < d; i++)
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
                int d = data.number + PlayerStat.Number;

                if (d != darks.Count)
                {
                    foreach (Dark dark in darks)
                        dark.Destroy();
                    for (int i = 0; i < d; i++)
                    {
                        Dark dark = ObjectManager.Instance.Add<Dark>();
                        dark.Owner = this;
                        dark.Room = Room;
                        dark.PosInfo = PosInfo;
                        dark.Info.Name = Id.ToString();
                        Trigons.Add(dark.Id, dark);
                        dark.Init();
                        dark.Degree = (360 / d) * (i);
                        dark.Tick = 0;
                    }
                }
            }
        }
    }
}

