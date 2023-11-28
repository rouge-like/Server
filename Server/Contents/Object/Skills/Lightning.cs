using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Contents.Object
{
	public class Lightning : Trigon
	{
		public Lightning()
		{
            StatInfo.Speed = 10.0f;
            StatInfo.Attack = 3;
            R = 6.0f;
            Info.Prefab = 1;
            IsSword = false;
        }
        public override void Init()
        {
            base.Init();
            OnAttack();
        }
        bool IsSame2(Vector2 a, Vector2 b, Vector2 p, Vector2 q)
        {
            float c1 = (b - a) * (p - a);
            float c2 = (b - a) * (q - a);

            return c1 * c2 >= 0;
        }

        bool InTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            bool a1 = IsSame2(a, b, c, p);
            bool a2 = IsSame2(b, c, a, p);
            bool a3 = IsSame2(c, a, b, p);

            return a1 && a2 && a3;
        }


        bool _coolTime;
        int _tick;
        int _onValue = 3;
        int _offValue = 5;

        public override void Update()
        {
            base.Update();

            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            _tick++;
            CheckAttack();
        }
        public override void CheckAttack()
        {
            base.CheckAttack();
            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.CellPos);
            int ownerX = Owner.PosInfo.PosX;
            int ownerY = Owner.PosInfo.PosY;
            if (_coolTime == false)
            {
                if (_tick > _offValue)
                {
                    OffAttack();
                    return;
                }
                Vector2 a = new Vector2(ownerX, ownerY);
                Vector2 b = new Vector2(X + ownerX, Y + ownerY);
                Vector2 c = new Vector2(AfterX + ownerX, AfterY + ownerY);

                foreach (Zone zone in zones)
                {
                    foreach (Monster m in zone.Monsters)
                    {
                        Vector2 d = new Vector2(m.PosInfo.PosX, m.PosInfo.PosY);
                        if (InTriangle(a, b, c, d))
                        {
                            m.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                        }
                    }
                    foreach (Player p in zone.Players)
                    {
                        if (p == Owner)
                            continue;

                        Vector2 t_OwerPos = new Vector2(p.PosInfo.PosX, p.PosInfo.PosY);

                        if (InTriangle(a, b, c, t_OwerPos))
                        {
                            p.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                        }
                    }
                }
            }
            else
            {
                if (_tick > _onValue)
                    OnAttack();
            }
        }
        public void OnAttack()
        {
            _coolTime = false;
            Room.Push(Room.EnterRoom, this);
            _tick = 0;
        }
        public void OffAttack()
        {
            _coolTime = true;
            Room.Push(Room.LeaveRoom, Id);
            _tick = 0;
        }
    }
}