using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Contents.Object
{
	public class Direct : Passive
	{
        public Direct()
        {
            //ObjectType = GameObjectType.Area;
        }

        GameObject _target;
        public int Range;

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

            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.CellPos);
            int d = int.MaxValue;
            foreach (Zone zone in zones)
            {
                foreach (Player p in zone.Players)
                {
                    int dx = Math.Abs(p.CellPos.x - Owner.CellPos.x);
                    int dy = Math.Abs(p.CellPos.y - Owner.CellPos.y);
                    int distance = dx + dy;

                    if (p == Owner)
                        continue;
                    if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2 || distance > d || p.State == State.Dead)
                        continue;

                    _target = p;
                    d = distance;
                }
                foreach (Monster m in zone.Monsters)
                {
                    int dx = Math.Abs(m.CellPos.x - Owner.CellPos.x);
                    int dy = Math.Abs(m.CellPos.y - Owner.CellPos.y);
                    int distance = dx + dy;

                    if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2 || distance > d || m.State == State.Dead)
                        continue;

                    _target = m;
                    d = distance;
                }
            }

            if (_target != null)
            {
                CellPos = Owner.CellPos;
                Console.WriteLine($"Target On Fire : {_target.Info.Name}");
                Dir = GetDirFromVec(_target.CellPos - Owner.CellPos);
                Room.Push(Room.EnterRoom, this);
                Attack();

                _target = null;
                _job = Room.PushAfter(_coolTime, Update);
                return;
            }
            else
            {
                CellPos = Owner.CellPos;
                Console.WriteLine($"Target On Fire : {Owner.Info.Name}");
                Dir = Owner.Dir;
                Room.Push(Room.EnterRoom, this);
                Attack();

                _target = null;
                _job = Room.PushAfter(_coolTime, Update);
                return;
            }
        }
        public override void Attack()
        {
            if (Room == null)
                return;

            Vector2Int tmp = DirToVector();
            Vector2Int endPoint;

            if (tmp.x == 0 || tmp.y == 0)
                endPoint = new Vector2Int((tmp.x * (Range / 10)) + CellPos.x, (tmp.y * (Range / 10)) + CellPos.y);
            else
                endPoint = new Vector2Int((tmp.x * (Range / 14)) + CellPos.x, (tmp.y * (Range / 14)) + CellPos.y);

            while (endPoint != CellPos)
            {
                int targetId = Room.Map.FindId(endPoint);
                if (targetId != 0 && targetId != 1)
                {
                    GameObject target = Room.Find(targetId);
                    if (target != Owner)
                        target.OnDamaged(this, StatInfo.Attack);
                }
                endPoint -= tmp;
            }

            Room.PushAfter(500, Room.LeaveRoom, Id);
        }
    }
}

