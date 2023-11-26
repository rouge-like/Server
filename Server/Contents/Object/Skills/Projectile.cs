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
            ObjectType = GameObjectType.Dagger;
            _moved = 0;
        }

        public GameObject Owner;
        int _moved;
        public override void Update()
        {
            if (Data == null || Owner == null || Room == null)
                return;
            if(_moved > Data.projectile.range)
            {
                Console.WriteLine($"Projectile {_moved}");
                Room.Push(Room.LeaveRoom, Id);
                return;
            }
            
            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

            int obj = Room.Map.FindId(CellPos);
            if (obj != 0 && obj != 1)
            {
                GameObject target = Room.Find(obj);
                if (target != Owner)
                {
                    target.OnDamaged(Owner, Data.damage);
                    Room.Push(Room.LeaveRoom, Id);
                    return;
                }
            }

            Vector2Int desPos = GetFrontCellPos();
            Vector2Int dirVector = DirToVector(Dir);

            if (Room.Map.CanGo(desPos))
            {
                Room.Map.MoveObject(this, desPos);

                S_Move movePakcet = new S_Move();
                movePakcet.ObjectId = Id;
                movePakcet.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePakcet);
                if (dirVector.x == 0 || dirVector.y == 0)
                    _moved += 10;
                else
                    _moved += 14;
                Console.WriteLine($"Projectile {Id}_Player{Owner.Id} : {PosInfo.PosX}, {PosInfo.PosY} , {PosInfo.Dir}");
            }
            else
            {
                int targetId = Room.Map.FindId(desPos);
                if(targetId != 0 && targetId != 1)
                {
                    GameObject target = Room.Find(targetId);

                    if(target != Owner)
                        target.OnDamaged(Owner, Data.damage);
                }

                if(Data.projectile.penetrate == false)
                    Room.Push(Room.LeaveRoom, Id);
            }
        }
        public void SetDir(Dir dir)
        {
            PosInfo.Dir = dir;
        }
    }
}
