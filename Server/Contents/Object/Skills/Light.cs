using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class Light : Passive
	{
		public Light()
		{
            ObjectType = GameObjectType.Area;
        }
        AdditionalWeaponStat _addData;
        List<List<int>> _area = new List<List<int>>();

        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
            _coolTime = 5000;
            List<int> tmp = new List<int>() { 0, 0 };
            _area.Add(tmp);
            Weapon.AdditionalStat.TryGetValue(EquipType.Light, out _addData);
        }

        public override void Update()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            CellPos = Owner.CellPos;
            int level;
            if (Weapon.EquipsA.TryGetValue(EquipType.Light, out level))
                StatInfo.Level = level;

            List<Zone> zones = Room.GetAdjacentZones(Owner.CellPos, 3);
            List<GameObject> targets = new List<GameObject>();

            foreach (Zone zone in zones)
            {
                if (Weapon.Target == null)
                {
                    foreach (Monster monster in zone.Monsters)
                        targets.Add(monster);
                }


                foreach (Player player in zone.Players)
                {
                    if(player != Owner)
                        targets.Add(player);
                }
            }
            Random random = new Random();

            if (targets.Count == 0)
            {
                _job = Room.PushAfter(100, Update);
                return;
            }
            LightInfo data;
            DataManager.LightDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Attack = data.attack;
            _coolTime = (int)(data.cooltime * ((200 - _addData.cooltime - Weapon.PlayerStat.Cooltime) / 100f));

            for (int i = 0; i < Math.Min(data.number + Weapon.PlayerStat.Number, targets.Count); i++)
            {
                Area area = ObjectManager.Instance.Add<Area>();
                {
                    area.Owner = Owner;
                    area.Info.Name = Info.Name;
                    area.Info.Prefab = 2;
                    area.CellPos = targets[random.Next(targets.Count)].CellPos;
                    area.StatInfo.Attack = StatInfo.Attack;
                    area.AttackCount = 1;
                    area.AttackArea = _area;
                    area.AdditionalAttack = _addData.attack;
                }

                Room.Push(Room.EnterRoom, area);
            }

            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

