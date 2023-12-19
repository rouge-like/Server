using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class ProjectileShotter : Passive
	{
		public ProjectileShotter()
		{
		}
        public override void Init()
        {
            base.Init();
            _job = Room.PushAfter(100, Update);
            _coolTime = 1000;
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
            if (Owner.EquipsA.TryGetValue(EquipType.Dagger, out level))
                StatInfo.Level = level;

            ArrowInfo data = null;
            DataManager.ArrowDict.TryGetValue(StatInfo.Level, out data);
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
                projectile.Speed = data.speed;
                projectile.StatInfo.Attack = data.attack;
                projectile.Penetrate = false;
                projectile.ProjectileRange = data.range;


                Vector2Int desPos = projectile.GetFrontCellPos();
                int id = Room.Map.FindId(desPos);
                if (Room.Map.CanGo(desPos))
                {
                    Room.Push(Room.EnterRoom, projectile);
                    //Console.WriteLine($"Daager_{projectile.Id} Enter By Player_{Owner.Id}");
                }
                    
                else if (id != 1 && id != 0 && id != Owner.Id)
                {
                    GameObject target = Room.Find(id);
                    target.OnDamaged(projectile, data.attack * Owner.StatInfo.Attack);
                }
                else
                    Console.WriteLine("Cannot Enter Projectile : Wrong Position");
            }
            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

