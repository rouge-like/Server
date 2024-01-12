using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Server.Contents
{
    public class Projectile : GameObject
    {
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
            _moved = 0;
        }

        public GameObject Owner;
        public int ProjectileRange;
        public bool Penetrate;
        int _moved;
        public override void Update()
        {
            if (Owner == null || Room == null)
                return;
            if(_moved >= ProjectileRange)
            {
                Room.Push(Room.LeaveRoom, Id);
                return;
            }
            
            int tick = (int)(1000 / StatInfo.Speed);
            Room.PushAfter(tick, Update);

            int obj = Room.Map.FindId(CellPos);
            if (obj != 0 && obj != 1)
            {
                GameObject target = Room.Find(obj);
                if (target != Owner)
                {
                    target.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                    if(Penetrate == false)
                    {
                        Room.Push(Room.LeaveRoom, Id);
                        return;
                    }

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
                Room.Push(Room.Broadcast,CellPos, movePakcet);
                if (dirVector.x == 0 || dirVector.y == 0)
                    _moved += 10;
                else
                    _moved += 14;
                //Console.WriteLine($"Projectile {Id}_Player{Owner.Id} : {PosInfo.PosX}, {PosInfo.PosY} , {PosInfo.Dir}");
            }
            else
            {
                int targetId = Room.Map.FindId(desPos);
                if(targetId != 0 && targetId != 1)
                {
                    GameObject target = Room.Find(targetId);

                    if(target != Owner)
                        target.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                }
                if(Penetrate == false || targetId == 0)
                    Room.Push(Room.LeaveRoom, Id);
                else
                {
                    Room.Map.MoveObject(this, desPos);

                    S_Move movePakcet = new S_Move();
                    movePakcet.ObjectId = Id;
                    movePakcet.PosInfo = PosInfo;
                    Room.Push(Room.Broadcast, CellPos, movePakcet);
                    if (dirVector.x == 0 || dirVector.y == 0)
                        _moved += 10;
                    else
                        _moved += 14;
                }
            }
        }
        public void SetDir(Dir dir)
        {
            PosInfo.Dir = dir;
        }
    }
}
