using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents.Object
{
	public class Dagger : Passive
	{
		public Dagger()
		{
            ObjectType = GameObjectType.Dagger;
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
            Skill skillData = null;
            DataManager.SkillDict.TryGetValue(2, out skillData);
            for (int i = 0; i < Owner.DaggerCount; i++)
            {
                Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                projectile.Owner = Owner;
                projectile.Data = skillData;
                projectile.Info.Name = $"Projectile_{projectile.Id}";
                projectile.PosInfo.State = State.Moving;
                projectile.SetDir(PosInfo.Dir);
                projectile.PosInfo.PosX = PosInfo.PosX;
                projectile.PosInfo.PosY = PosInfo.PosY;
                projectile.Speed = skillData.projectile.speed;

                Vector2Int desPos = projectile.GetFrontCellPos();
                int id = Room.Map.FindId(desPos);
                if (Room.Map.CanGo(desPos))
                {
                    Room.Push(Room.EnterRoom, projectile);
                    Console.WriteLine($"Daager_{projectile.Id} Enter By Player_{Owner.Id}");
                }
                    
                else if (id != 1 && id != 0 && id != Owner.Id)
                {
                    GameObject target = Room.Find(id);
                    target.OnDamaged(projectile.Owner, projectile.Data.damage);
                }
                else
                    Console.WriteLine("Cannot Enter Projectile : Wrong Position");
            }
            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

