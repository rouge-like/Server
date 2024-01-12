using System;
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
            Owner.AdditionalStat.TryGetValue(EquipType.Arrow, out _addData);
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
            if (Owner.EquipsA.TryGetValue(EquipType.Arrow, out level))
                StatInfo.Level = level;

            ArrowInfo data = null;
            DataManager.ArrowDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Range = data.range * ((_addData.range + Owner.PlayerStat.WeaponRange) / 100f);
            StatInfo.Speed = data.speed * ((_addData.speed + Owner.PlayerStat.WeaponSpeed) / 100f);
            StatInfo.Attack = (int)(data.attack * (_addData.attack / 100f));
            _coolTime = (int)(data.cooltime * ((200 - _addData.cooltime - Owner.PlayerStat.Cooltime) / 100f));


            for (int i = 0; i < data.number + Owner.PlayerStat.Number; i++)
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


                Vector2Int desPos = projectile.GetFrontCellPos();
                int id = Room.Map.FindId(desPos);
                if (Room.Map.CanGo(desPos))
                {
                    Room.Push(Room.EnterRoom, projectile);
                }          
                else if (id != 1 && id != 0 && id != Owner.Id)
                {
                    GameObject target = Room.Find(id);
                    target.OnDamaged(projectile, StatInfo.Attack * Owner.StatInfo.Attack);
                }
            }
            _job = Room.PushAfter(_coolTime * 100, Update);
        }
    }
}

