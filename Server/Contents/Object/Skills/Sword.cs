using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Contents.Object
{
	public class Sword : Trigon
	{
		public Sword()
		{
            IsSword = true;
            Info.Prefab = 0;
		}

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
            bool a1 = IsSame2(a, b, c, d);
            bool a2 = IsSame2(c, d, a, b);

            return (a1 == false) && (a2 == false);
        }

        bool AfterColision(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 e, Vector2 f)
        {
            return InTriangle(a, b, c, e) && !IsSame1(d, e, f, c);
        }

        bool _coolTime;

        public override void Update()
        {
            base.Update();

            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

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
                            if (m.IsMetal && m.State != State.Dead)
                            {
                                Speed = Speed * -1;
                                _coolTime = true;
                                Room.PushAfter(500, CoolTimeOver);

                                S_HitTrigon hit = new S_HitTrigon();
                                hit.Trigon1Id = Id;

                                Room.Push(Room.Broadcast, Owner.CellPos, hit);
                            }
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
                            p.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                        }
                        foreach (Trigon t in p.Trigons.Values)
                        {
                            if (t.Owner == null)
                                continue;
                            if (t.IsSword == false)
                                continue;

                            Sword s = t as Sword;

                            Vector2 d = new Vector2(s.X + p.PosInfo.PosX, s.Y + p.PosInfo.PosY);
                            Vector2 pos = new Vector2(p.PosInfo.PosX, p.PosInfo.PosY);

                            if (Intersection(a, b, pos, d)) //|| AfterColision(a, b, c, pos, d, e))
                            {
                                if (s.Hit() == false)
                                    continue;

                                Speed = Speed * -1;
                                _coolTime = true;
                                Room.PushAfter(500, CoolTimeOver);

                                S_HitTrigon hit = new S_HitTrigon();
                                hit.Trigon1Id = Id;
                                hit.Trigon2Id = s.Id;

                                Room.Push(Room.Broadcast, Owner.CellPos, hit);

                                OnDamaged(t, s.StatInfo.Attack);
                                s.OnDamaged(this, StatInfo.Attack);

                                return;
                            }
                        }
                    }
                }
            }

        }
        public void CoolTimeOver()
        {
            _coolTime = false;
        }

        public bool Hit()
        {
            if (_coolTime == false)
            {
                if (Room == null)
                    return false;

                Speed = Speed * -1;
                _coolTime = true;
                Room.PushAfter(500, CoolTimeOver);
                return true;
            }
            else
                return false;
        }
    }
}