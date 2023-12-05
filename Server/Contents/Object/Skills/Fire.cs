﻿using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents.Object
{
    public class Fire : Passive
    {
        public Fire()
        {
            ObjectType = GameObjectType.Area;
        }

        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
        }

        public override void Update()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            CellPos = Owner.CellPos;

            int level;
            if (Owner.EquipsA.TryGetValue(EquipType.Fire, out level))
                StatInfo.Level = level;

            FireInfo data;
            DataManager.FireDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Attack = data.attack;
            _coolTime = data.cooltime;

            Area area = ObjectManager.Instance.Add<Area>();
            {
                area.Owner = Owner;
                area.Info.Name = Info.Name;
                area.Info.Prefab = 0;
                area.CellPos = CellPos;
                area.StatInfo.Attack = StatInfo.Attack;
                area.AttackCount = 1;
                area.AttackArea = data.area;
            }

            Room.Push(Room.EnterRoom, area);
            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

