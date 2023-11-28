using System;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{
	public class Air : Passive
	{
		public Air()
		{
		}
        int[] _x = new int[] { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] _y = new int[] { 1, 0, -1, 0, 1, -1, 1, -1 };
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
            DataManager.SkillDict.TryGetValue(4, out skillData);

            Vector2Int dirVector = DirToVector(Dir);
            if (dirVector.x == 0 || dirVector.y == 0)
            {
                for(int i = 0; i < 4; i++)
                {
                    Dir dir = GetDirFromVec(new Vector2Int(_x[i], _y[i]));
                    Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                    projectile.Owner = Owner;
                    projectile.Data = skillData;
                    projectile.Info.Name = $"Projectile_{projectile.Id}";
                    projectile.Info.Prefab = 1;
                    projectile.PosInfo.State = State.Moving;
                    projectile.SetDir(dir);
                    projectile.PosInfo.PosX = PosInfo.PosX;
                    projectile.PosInfo.PosY = PosInfo.PosY;
                    projectile.Speed = skillData.projectile.speed;

                    Vector2Int desPos = projectile.GetFrontCellPos();
                    int id = Room.Map.FindId(desPos);
                    if (Room.Map.CanGo(desPos))
                    {
                        Room.Push(Room.EnterRoom, projectile);
                    }

                    else if (id != 0)
                    {
                        Room.Push(Room.EnterRoom, projectile);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Dir dir = GetDirFromVec(new Vector2Int(_x[i + 4], _y[i + 4]));
                    Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                    projectile.Owner = Owner;
                    projectile.Data = skillData;
                    projectile.Info.Name = $"Projectile_{projectile.Id}";
                    projectile.Info.Prefab = 1;
                    projectile.PosInfo.State = State.Moving;
                    projectile.SetDir(dir);
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

                    else if (id != 0)
                    {
                        Room.Push(Room.EnterRoom, projectile);
                    }
                }
            }
            _job = Room.PushAfter(_coolTime, Update);
        }
    }
}

