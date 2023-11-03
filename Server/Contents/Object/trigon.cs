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
			Speed = 30.0f;
			StatInfo.Attack = 50;
			StatInfo.MaxHp = 150;
			StatInfo.Hp = 15000;
            _coolTime = true;
			X = 0;
			Y = 0; 
			AfterX = 0;
			AfterY = 0;
		}

		public Player Owner;
		public float R;
        public float X;
        public float Y;
		public float AfterX;
		public float AfterY;

        float _deg;
		IJob _job;
		bool _coolTime;


		bool IsSame1(Vector2 a, Vector2 b, Vector2 p, Vector2 q)
		{
			float c1 = (b - a) * (p - a);
			float c2 = (b - a) * (q - a);

			return c1 * c2 > 0;
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

		bool Intersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			bool a1 = IsSame1(a, b, c, d);
			bool a2 = IsSame1(c, d, a, b);

			return (a1 == false) && (a2 == false);
		}

		bool AfterColision(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 e, Vector2 f)
		{
			return InTriangle(a, b, c, e) && !IsSame1(d ,e, f, c);
		}
		
		public override void Init()
		{
            Room.PushAfter(200, CoolTimeOver);
            _job = Room.PushAfter(100, Update);
        }

		public override void Update()
		{
			if (Room == null)
				return;
			if (Owner == null || Owner.Room == null)
				return;

			var pi = Math.PI;
            //Console.WriteLine($"{Id} : {CellPos.x}, {CellPos.y}");

            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.CellPos);
			int ownerX = Owner.PosInfo.PosX;
			int ownerY = Owner.PosInfo.PosY;

			if(_coolTime == false)
			{
                Vector2 a = new Vector2(ownerX, ownerY);
                Vector2 b = new Vector2(X + ownerX, Y + ownerY);
                Vector2 c = new Vector2(AfterX + ownerX, AfterY + ownerY);

                foreach (Zone zone in zones)
                {
                    foreach (Item i in zone.Items)
                    {
                        Vector2 d = new Vector2(i.PosInfo.PosX, i.PosInfo.PosY);
                        if (InTriangle(a, b, c, d))
                        {
                            Console.WriteLine($"{Owner.Id} Get Item");
							Room.Push(Owner.EarnItem, i);
                        }
                    }
                    foreach (Player p in zone.Players)
                    {
						if (p == Owner)
							continue;

                        Vector2 t_OwerPos = new Vector2(p.PosInfo.PosX, p.PosInfo.PosY);

                        if (InTriangle(a, b, c, t_OwerPos))
                        {
                            //Console.WriteLine($"{Id} : HIT Player{p.Id}!");
                            p.OnDamaged(Owner, StatInfo.Attack * 4);

                            break;
                        }
                        foreach (Trigon t in p.Drones.Values)
                        {
							if (t.Owner == null)
								continue;

                            Vector2 d = new Vector2(t.X + p.PosInfo.PosX, t.Y + p.PosInfo.PosY);
							Vector2 e = new Vector2(t.AfterX + p.PosInfo.PosX, t.AfterY + p.PosInfo.PosY);
							Vector2 pos = new Vector2(p.PosInfo.PosX, p.PosInfo.PosY);

							if(Intersection(a, b, pos, d)) //|| AfterColision(a, b, c, pos, d, e))
							{
                                Speed = Speed * -1;
                                _coolTime = true;
                                Room.PushAfter(2000, CoolTimeOver);
                                t.Hit();

								S_HitTrigon hit = new S_HitTrigon();
								hit.Trigon1Id = Id;
								hit.Trigon2Id = t.Id;

                                Room.Push(Room.Broadcast, Owner.CellPos, hit);

                                OnDamaged(p, t.StatInfo.Attack);
								t.OnDamaged(Owner, StatInfo.Attack);

								break;
                            }
                        }
                    }
                }
            }

			if (Room == null) // Owner 사망시 Room 초기
				return;

            _deg += Speed;

            X = (float)(Math.Cos(_deg * 2 * pi / 360) * R);
			Y = (float)(Math.Sin(_deg * 2 * pi / 360) * R);

            AfterX = (float)(Math.Cos((_deg + Speed) * 2 * pi / 360) * R);
            AfterY = (float)(Math.Sin((_deg + Speed) * 2 * pi / 360) * R);

            S_MoveFloat packet = new S_MoveFloat();
			packet.Degree = _deg;
			packet.ObjectId = Id;
			packet.Dir = Speed > 0;

			Room.Push(Room.Broadcast, Owner.CellPos, packet);

            if (_deg > 360)
                _deg -= 360;

            if (_deg < 0)
                _deg += 360;

            _job = Room.PushAfter(100, Update);
		}
		public void MoveByPlayer()
		{
            Zone now = Room.GetZone(CellPos);
            Zone after = Room.GetZone(Owner.CellPos);
            if (now != after)
            {
                now.Trigons.Remove(this);
                after.Trigons.Add(this);
            }
            PosInfo.PosX = Owner.CellPos.x;
            PosInfo.PosY = Owner.CellPos.y;
        }
		public void Destroy()
		{
            if (_job != null)
			{
                Console.WriteLine("Cancel Job");
				_job.Cancel = true;
				_job = null;
			}
			Owner.Drones.Remove(Id);
			Room.Push(Room.LeaveRoom, Id);
		}

		public void CoolTimeOver()
		{
			_coolTime = false;
		}

		public void Hit()
		{
			if (_coolTime == false)
			{
				if (Room == null)
					return;

				Speed = Speed * -1;
				_coolTime = true;
				Room.PushAfter(2000, CoolTimeOver);
			}
		}
        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
			Destroy();
        }
    }
}

