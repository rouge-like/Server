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
        AdditionalWeaponStat _addData = null;

        public override void Init()
        {
            base.Init();
            Owner.AdditionalStat.TryGetValue(EquipType.Air, out _addData);
            _job = Room.PushAfter(100, Update);
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
            if (Owner.EquipsA.TryGetValue(EquipType.Air, out level))
                StatInfo.Level = level;

            AirInfo data = null;
            DataManager.AirDict.TryGetValue(StatInfo.Level, out data);

            StatInfo.Range = data.range * ((_addData.range + Owner.PlayerStat.WeaponRange) / 100f);
            StatInfo.Speed = data.speed * ((_addData.speed + Owner.PlayerStat.WeaponSpeed) / 100f);
            StatInfo.Attack = (int)(data.attack * (_addData.attack / 100f));
            _coolTime = (int)(data.cooltime * ((200 - _addData.cooltime - Owner.PlayerStat.Cooltime) / 100f));

            Vector2Int dirVector = DirToVector(Dir);
            if (dirVector.x == 0 || dirVector.y == 0)
            {
                for(int i = 0; i < 4; i++)
                {
                    Dir dir = GetDirFromVec(new Vector2Int(_x[i], _y[i]));
                    Projectile projectile = ObjectManager.Instance.Add<Projectile>();
                    projectile.Owner = Owner;
                    projectile.Info.Name = $"Projectile_{projectile.Id}";
                    projectile.Info.Prefab = 1;
                    projectile.PosInfo.State = State.Moving;
                    projectile.SetDir(dir);
                    projectile.PosInfo.PosX = PosInfo.PosX;
                    projectile.PosInfo.PosY = PosInfo.PosY;
                    projectile.Speed = StatInfo.Speed;
                    projectile.StatInfo.Attack = StatInfo.Attack;
                    projectile.Penetrate = true;
                    projectile.ProjectileRange = (int)StatInfo.Range;

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
                    projectile.Info.Name = $"Projectile_{projectile.Id}";
                    projectile.Info.Prefab = 1;
                    projectile.PosInfo.State = State.Moving;
                    projectile.SetDir(dir);
                    projectile.PosInfo.PosX = PosInfo.PosX;
                    projectile.PosInfo.PosY = PosInfo.PosY;
                    projectile.Speed = StatInfo.Speed;
                    projectile.StatInfo.Attack = StatInfo.Attack;
                    projectile.Penetrate = true;
                    projectile.ProjectileRange = (int)StatInfo.Range;

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
            _job = Room.PushAfter(_coolTime * 100, Update);
        }
    }
}

