﻿using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents.Object
{
	public class Dark : Trigon
	{
		public Dark()
		{
            Info.Prefab = 2;
            IsSword = false;
        }

        AdditionalWeaponStat _addData = null;

        public override void Init()
        {
            base.Init();

            DarkInfo data = null;
            DataManager.DarkDict.TryGetValue(StatInfo.Level, out data);
            Weapon.AdditionalStat.TryGetValue(EquipType.Dark, out _addData);
            StatInfo.Attack = data.attack;
            StatInfo.Speed = data.speed * ((_addData.speed + Weapon.PlayerStat.WeaponSpeed) / 100f);
            StatInfo.Range = data.range * ((_addData.range + Weapon.PlayerStat.WeaponRange) / 100f);
            _coolTimeTick = (int)(data.cooltime * ((200 - _addData.cooltime - Weapon.PlayerStat.Cooltime) / 100f));
            _durationTick = (int)(data.duration * ((_addData.duraion + Weapon.PlayerStat.Duration) / 100f));

            //Console.WriteLine($"{_coolTimeTick}, {_durationTick}");

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

        public int Tick;
        int _coolTimeTick;
        int _durationTick;
        float _smallX;
        float _smallY;

        public override void Update()
        {
            base.Update();

            var pi = Math.PI;
            _smallX = (float)(Math.Cos(Degree * 2 * pi / 360) * (StatInfo.Range - 0.5f));
            _smallY = (float)(Math.Sin(Degree * 2 * pi / 360) * (StatInfo.Range - 0.5f));

            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            Tick++;
            CheckAttack();
        }
        public override void CheckAttack()
        {
            if (X == 0 && AfterX == 0)
                return;
            base.CheckAttack();

            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;
            GameObject owner = Owner;
            List<Zone> zones = owner.Room.GetAdjacentZones(owner.CellPos);
            int ownerX = owner.PosInfo.PosX;
            int ownerY = owner.PosInfo.PosY;

            int level;
            if (Weapon.EquipsA.TryGetValue(EquipType.Lightning, out level))
                StatInfo.Level = level;


            if (_coolTime == false)
            {
                if (Tick > _durationTick)
                {
                    OffAttack();
                    return;
                }
                Vector2 a = new Vector2(_smallX + ownerX, _smallY + ownerY);
                Vector2 b = new Vector2(X + ownerX, Y + ownerY);
                Vector2 c = new Vector2(AfterX + ownerX, AfterY + ownerY);
                Vector2 test = new Vector2(0, 0);
                if (InTriangle(a, b, c, test))
                {
                    Console.WriteLine("");
                }
                foreach (Zone zone in zones)
                {
                    if (Weapon.Target == null)
                    {
                        foreach (Monster m in zone.Monsters)
                        {
                            Vector2Int dirVec = Owner.CellPos - m.CellPos;
                            Vector2 d = GetRBPos(m.CellPos, dirVec);


                            if (InTriangle(a, b, c, d))
                            {
                                m.OnDamaged(this, (int)(StatInfo.Attack * (_addData.attack / 100f) * (owner.StatInfo.Attack + Weapon.PlayerStat.Attack)));
                                if((owner.CellPos - m.CellPos).magnitude > 4)
                                    Console.WriteLine("");
                            }
                        }
                    }
                    foreach (Player p in zone.Players)
                    {
                        if (p == owner)
                            continue;

                        Vector2Int dirVec = Owner.CellPos - p.CellPos;
                        Vector2 t_OwerPos = GetRBPos(p.CellPos, dirVec);

                        if (InTriangle(a, b, c, t_OwerPos))
                        {
                            p.OnDamaged(this, (int)(StatInfo.Attack * (_addData.attack / 100f) * (owner.StatInfo.Attack + Weapon.PlayerStat.Attack)));
                        }
                    }
                }
            }
            else
            {
                if (Tick > _coolTimeTick)
                    OnAttack();
            }
        }
        public void OnAttack()
        {
            _coolTime = false;
            Tick = 0;
        }
        public void OffAttack()
        {
            _coolTime = true;
            Tick = 0;
        }
    }
}