﻿using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class Arrow : Passive
	{
		public Arrow()
		{
		}
        AdditionalWeaponStat _addData = null;

        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
            Weapon.AdditionalStat.TryGetValue(EquipType.Arrow, out _addData);
        }
        public override void Update()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            base.Update();

            PosInfo = Owner.PosInfo;
            int level;
            if (Weapon.EquipsA.TryGetValue(EquipType.Arrow, out level))
                StatInfo.Level = level;

            ArrowInfo data = null;
            DataManager.ArrowDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Range = data.range * ((_addData.range + Weapon.PlayerStat.WeaponRange) / 100f);
            StatInfo.Speed = data.speed * ((_addData.speed + Weapon.PlayerStat.WeaponSpeed) / 100f);
            StatInfo.Attack = data.attack;
            _coolTime = (int)(data.cooltime * ((200 - _addData.cooltime - Weapon.PlayerStat.Cooltime) / 100f));

            S_ShotProjectile packet = new S_ShotProjectile();
            packet.PlayerId = Owner.Id;
            packet.Projectile = EquipType.Arrow;
            Room.Push(Room.Broadcast, CellPos, packet);

            for (int i = 0; i < data.number + Weapon.PlayerStat.Number; i++)
            {
                Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                projectile.Owner = Owner;
                projectile.Info.Name = $"Projectile_{projectile.Id}";
                projectile.Info.Prefab = 0;
                projectile.PosInfo.State = State.Moving;
                projectile.SetDir(PosInfo.Dir);
                projectile.PosInfo.PosX = PosInfo.PosX;
                projectile.PosInfo.PosY = PosInfo.PosY;
                projectile.Speed = StatInfo.Speed ;
                projectile.StatInfo.Attack = StatInfo.Attack;
                projectile.Penetrate = false;
                projectile.ProjectileRange = (int)StatInfo.Range;
                projectile.AdditionalAttack = _addData.attack;


                Vector2Int desPos = projectile.GetFrontCellPos();
                int id = Room.Map.FindId(desPos);
                if (Room.Map.CanGo(desPos))
                {
                    Room.Push(Room.EnterRoom, projectile);
                }          
                else if (id != 1 && id != 0 && id != Owner.Id)
                {
                    GameObject target = Room.Find(id);
                    if (((IWeaponAble)Owner).Target != null && target.ObjectType != GameObjectType.Player)
                        continue;
                    if (target != null)
                        target.OnDamaged(projectile, (int)(StatInfo.Attack * (_addData.attack / 100f) * (Owner.StatInfo.Attack + ((IWeaponAble)Owner).PlayerStat.Attack)));
                }
            }
            _job = Room.PushAfter(_coolTime * 100, Update);
        }
    }
}

