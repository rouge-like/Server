using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
		public float Speed
		{
			get { return StatInfo.Speed; }
			set { StatInfo.Speed = value; }
		}
		public Room Room { get; set; }
		public ObjectInfo Info { get; set; } = new ObjectInfo() { PosInfo = new PosInfo(), StatInfo = new StatInfo() };
        public PosInfo PosInfo { get { return Info.PosInfo; } set { Info.PosInfo = value; } }
		public StatInfo StatInfo { get { return Info.StatInfo; } set { Info.StatInfo = value; } }



		public Vector2Int CellPos
		{
			get
			{
				return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
			}
		}
		public Vector2Int GetFrontCellPos()
        {
			return GetFrontCellPos(PosInfo.Dir);
        }

		public Vector2Int GetFrontCellPos(Dir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case Dir.Up:
					cellPos += Vector2Int.up;
					break;
				case Dir.Down:
					cellPos += Vector2Int.down;
					break;
				case Dir.Right:
					cellPos += Vector2Int.right;
					break;
				case Dir.Left:
					cellPos += Vector2Int.left;
					break;
				case Dir.Upright:
					cellPos += Vector2Int.upRight;
					break;
				case Dir.Upleft:
					cellPos += Vector2Int.upLeft;
					break;
				case Dir.Downright:
					cellPos += Vector2Int.downRight;
					break;
				case Dir.Downleft:
					cellPos += Vector2Int.downLeft;
					break;

			}

			return cellPos;
		}

		public virtual void OnDamaged(GameObject attacker ,int damage)
        {
			StatInfo.Hp -= damage;
			StatInfo.Hp = Math.Max(StatInfo.Hp, 0);

			S_ChangeHp packet = new S_ChangeHp();
			packet.ObjectId = Id;
			packet.Hp = StatInfo.Hp;
			Room.Broadcast(CellPos, packet);

			if(StatInfo.Hp <= 0)
            {
				OnDead(attacker);
            }

        }
		public virtual void OnDead(GameObject attacker)
        {
            Console.WriteLine($"{Id} DEAD");
        }
	}
}
