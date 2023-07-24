using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Projectile : GameObject
    {
        public Data.Skill Data { get; set; }
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public GameObject Owner;
        long _nextMoveTick = 0;
        public virtual void Update()
        {
            if (Data == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

            Vector2Int desPos = GetFrontCellPos();

            if (Room.Map.CanGo(desPos))
            {
                CellPos = desPos;

                S_Move movePakcet = new S_Move();
                movePakcet.ObjectId = Id;
                movePakcet.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePakcet);

                Console.WriteLine($"Projectile {Id}_Player{Owner.Id} : {PosInfo.PosX}, {PosInfo.PosY} , {PosInfo.Dir}");
            }
            else
            {
                int targetId = Room.Map.FindId(desPos);
                if(targetId != 0)
                {
                    GameObject target = Room.Find(targetId);
                    target.OnDamaged(Owner, Data.damage);
                }

                Room.LeaveRoom(Id);
            }
        }
        public void SetDir(Dir dir)
        {
            PosInfo.Dir = dir;
            switch (dir)
            {
                case Dir.Upright:
                    PosInfo.Dir = Dir.Up;
                    break;
                case Dir.Upleft:
                    PosInfo.Dir = Dir.Up;
                    break;
                case Dir.Downright:
                    PosInfo.Dir = Dir.Down;
                    break;
                case Dir.Downleft:
                    PosInfo.Dir = Dir.Down;
                    break;
            }
        }
    }
}
