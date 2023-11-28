using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Contents.Object
{
	public class Fire : Passive
	{
		public Fire()
		{
            ObjectType = GameObjectType.Area;
            StatInfo.Attack = 10;
        }

        int[,] _area = new int[,] { { 0, 0 }, { 0, 1 }, { 0, -1 }, { 1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 0 }, { -1, 1 }, { -1, -1 } };

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
            Room.Push(Room.EnterRoom, this);
            Attack();

            _job = Room.PushAfter(_coolTime, Update);
        }
        public override void Attack()
        {
            if (Room == null)
                return;

            for (int i = 0; i < _area.GetLength(0); i++)
            {
                Vector2Int pos = new Vector2Int(CellPos.x + _area[i, 0], CellPos.y + _area[i, 1]);

                if (pos.x >= Room.Map.SizeX || pos.y >= Room.Map.SizeY || pos.x < 0 || pos.y < 0)
                    continue;

                int targetId = Room.Map.FindId(pos);
                if (targetId != 0 && targetId != 1)
                {
                    GameObject target = Room.Find(targetId);
                    if (target != Owner)
                        target.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                }
            }

            Room.PushAfter(500, Room.LeaveRoom, Id);
        }
    }
}

