using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Projectile : GameObject
    {
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public GameObject Owner;
        long _nextMoveTick = 0;
        public virtual void Update()
        {
            if (Owner == null || Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            _nextMoveTick = Environment.TickCount64 + 50;

            Vector2Int desPos = GetFrontCellPos();

            if (Room.Map.CanGo(desPos))
            {
                CellPos = desPos;

                S_Move movePakcet = new S_Move();
                movePakcet.ObjectId = Id;
                movePakcet.PosInfo = PosInfo;
                Room.Broadcast(movePakcet);

                Console.WriteLine("Move Projectile");
            }
            else
            {
                int targetId = Room.Map.FindId(desPos);
                if(targetId != 0)
                {
                    // Find By Id in Room?
                }

                Room.LeaveRoom(Id);
            }
        }
    }
}
