using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class Poison : Passive
	{
		public Poison()
		{
            ObjectType = GameObjectType.Area;
        }

        List<List<int>> _area = new List<List<int>>();

        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
            _coolTime = 100;
            List<int> tmp = new List<int>() { 0, 0 };
            _area.Add(tmp);
            Weapon.AdditionalStat.TryGetValue(EquipType.Poison, out _addData);
        }
        AdditionalWeaponStat _addData = null;

        public override void Update()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            CellPos = Owner.CellPos;

            int level;
            if (Weapon.EquipsA.TryGetValue(EquipType.Poison, out level))
                StatInfo.Level = level;

            PoisonInfo data;
            DataManager.PoisonDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Attack = data.attack;
            _coolTime = (int)(100 * ((200 - _addData.cooltime - Weapon.PlayerStat.Cooltime) / 100f));

            Area area = ObjectManager.Instance.Add<Area>();
            {
                area.Owner = Owner;
                area.Info.Name = Info.Name;
                area.Info.Prefab = 3;
                area.CellPos = CellPos;
                area.StatInfo.Attack = StatInfo.Attack;
                area.AttackCount = (int)(data.attackcount * ((_addData.duraion + Weapon.PlayerStat.Duration) / 100f));
                area.AttackArea = _area;
                area.AdditionalAttack = _addData.attack;
            }

            Room.Push(Room.EnterRoom, area);
            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

