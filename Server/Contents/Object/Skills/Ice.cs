using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class Ice : Passive
	{
		public Ice()
		{
            ObjectType = GameObjectType.Area;
            StatInfo.Attack = 1;
        }

        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
            _coolTime = 5000;
        }

        public override void Update()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            CellPos = Owner.CellPos;

            int level;
            if (Owner.EquipsA.TryGetValue(EquipType.Ice, out level))
                StatInfo.Level = level;

            IceInfo data;
            DataManager.IceDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Attack = data.attack;
            _coolTime = data.cooltime;

            for (int i = 0; i < data.number + Owner.PlayerStat.Number; i++)
            {
                Random rand = new Random();
                Vector2Int random;
                while (true)
                {
                    while (true)
                    {
                        random = new Vector2Int(rand.Next(-4, 5), rand.Next(-4, 5));
                        if (Room.Map.CanGo(CellPos + random))
                        {
                            break;
                        }
                    }
                    break;
                }
                float value = 0.1f;
                if (data.area.Count > 4)
                    value = 0.2f;
                Area area = ObjectManager.Instance.Add<Area>();
                {
                    area.Owner = Owner;
                    area.Info.Name = Info.Name;
                    area.Info.Prefab = 1;
                    area.Info.Degree = value;
                    area.CellPos = CellPos + random;
                    area.StatInfo.Attack = StatInfo.Attack;
                    area.AttackCount = data.attackcount;
                    area.AttackArea = data.area;
                }

                Room.Push(Room.EnterRoom, area);
            }

            _job = Room.PushAfter(_coolTime, Update);
        }
        public override void Attack()
        {
            if (Room == null)
                return;
        }
    }
}

