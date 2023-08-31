using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
	public class Trigon : GameObject
	{
		public Trigon()
		{
			ObjectType = GameObjectType.Trigon;
			R = 3.0f;
			_deg = 0;
			Speed = 15.0f;
			StatInfo.Attack = 50;
			_coolTime = false;
		}

		public GameObject Owner;
		public float R;
        public float X;
        public float Y;
		public float AfterX;
		public float AfterY;
        float _deg;
		IJob _job;
		bool _coolTime;


		bool isSame(Vector2 a, Vector2 b, Vector2 p, Vector2 q)
		{
			float c1 = (b - a) * (p - a);
			float c2 = (b - a) * (q - a);

			return c1 * c2 > 0;
		}

		public bool inTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
		{
			bool a1 = isSame(a, b, c, p);
			bool a2 = isSame(b, c, a, p);
			bool a3 = isSame(c, a, b, p);

			return a1 && a2 && a3;
		}

		public bool intersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			bool a1 = isSame(a, b, c, d);
			bool a2 = isSame(c, d, a, b);

			return (a1 == false) && (a2 == false);
		}

		public override void Update()
		{
			if (Owner == null || Owner.Room == null)
				return;

			_deg += Speed;

			var pi = Math.PI;

            Zone now = Room.GetZone(CellPos);
            Zone after = Room.GetZone(Owner.CellPos);
            if (now != after)
            {
                now.Trigons.Remove(this);
                after.Trigons.Add(this);
            }

            PosInfo.PosX = Owner.CellPos.x;
            PosInfo.PosY = Owner.CellPos.y;
            //Console.WriteLine($"{Id} : {CellPos.x}, {CellPos.y}");

            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.CellPos);
			int ownerX = Owner.PosInfo.PosX;
			int ownerY = Owner.PosInfo.PosY;

			if(_coolTime == false) {
                Vector2 a = new Vector2(ownerX, ownerY);
                Vector2 b = new Vector2(X + ownerX, Y + ownerY);
                Vector2 c = new Vector2(AfterX + ownerX, AfterY + ownerY);

                foreach (Zone zone in zones)
                {
                    foreach (Player p in zone.Players)
                    {
						if (p == Owner)
							continue;
                        foreach (Trigon t in p.Drones.Values)
                        {
							if (t.Owner == null)
								continue;

							GameObject t_Owner = t.Owner;
							GameObject my_Owner = Owner;
                            Vector2 d = new Vector2(t.X + t_Owner.PosInfo.PosX, t.Y + t_Owner.PosInfo.PosY);
							Vector2 e = new Vector2(t.AfterX + t_Owner.PosInfo.PosX, t.AfterY + t_Owner.PosInfo.PosY);
							Vector2 pos = new Vector2(t_Owner.PosInfo.PosX, t_Owner.PosInfo.PosY);

							if(intersection(a, b, pos, d) || intersection(a, c, pos, e))
							{
								Console.WriteLine($"{Id} : HIT BY INTERSECTION");
                                Speed = Speed * -1;
                                _coolTime = true;
                                Room.PushAfter(2000, CoolTimeOver);
                                t.Hit();

								S_HitTrigon hit = new S_HitTrigon();
								hit.Trigon1Id = Id;
								hit.Trigon2Id = t.Id;

                                Room.Push(Room.Broadcast, my_Owner.CellPos, hit);

                                my_Owner.OnDamaged(t_Owner, t.StatInfo.Attack);
								t_Owner.OnDamaged(my_Owner, StatInfo.Attack);
                            }
                        }
                    }
                }
            }

			if (Room == null) // Owner 사망시 Room 초기
				return;

			X = (float)(Math.Cos(_deg * 2 * pi / 360) * R);
			Y = (float)(Math.Sin(_deg * 2 * pi / 360) * R);

            AfterX = (float)(Math.Cos((_deg + Speed) * 2 * pi / 360) * R);
            AfterY = (float)(Math.Sin((_deg + Speed) * 2 * pi / 360) * R);
            S_MoveFloat packet = new S_MoveFloat();
			packet.Degree = _deg;
			packet.ObjectId = Id;

			Room.Push(Room.Broadcast, Owner.CellPos, packet);

            if (_deg > 360)
                _deg -= 360;
			Console.WriteLine($"{Id} : {X},{Y}");
            _job = Room.PushAfter(100, Update);
		}
		public void Destroy()
		{
			if(_job != null)
			{
				_job.Cancel = true;
				_job = null;
			}
		}

		public void CoolTimeOver()
		{
			_coolTime = false;
		}

		public void Hit()
		{
			if(_coolTime == false)
			{
				if (Room == null)
					return;

                Speed = Speed * -1;
                _coolTime = true;
                Room.PushAfter(2000, CoolTimeOver);
            }
        }
	}
}

