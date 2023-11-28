using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
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
		public State State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}
		public  Dir Dir
		{
			get { return PosInfo.Dir; }
			set { PosInfo.Dir = value; }
		}
		public Room Room { get; set; }
		public ObjectInfo Info { get; set; } = new ObjectInfo() { PosInfo = new PosInfo(), StatInfo = new StatInfo() };
        public PosInfo PosInfo { get { return Info.PosInfo; } set { Info.PosInfo = value; } }
		public StatInfo StatInfo { get { return Info.StatInfo; } set { Info.StatInfo = value; } }

		public virtual void Init() { }
		public virtual void Update() { }

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
		public Vector2Int DirToVector()
		{
			return DirToVector(Dir);
		}
        public Vector2Int DirToVector(Dir dir)
        {
            Vector2Int dirVector = new Vector2Int(0, 1);
            switch (dir)
            {
                case Dir.Up:
                    dirVector = Vector2Int.up;
                    break;
                case Dir.Upright:
                    dirVector = Vector2Int.upRight;
                    break;
                case Dir.Upleft:
                    dirVector = Vector2Int.upLeft;
                    break;
                case Dir.Down:
                    dirVector = Vector2Int.down;
                    break;
                case Dir.Downright:
                    dirVector = Vector2Int.downRight;
                    break;
                case Dir.Downleft:
                    dirVector = Vector2Int.downLeft;
                    break;
                case Dir.Right:
                    dirVector = Vector2Int.right;
                    break;
                case Dir.Left:
                    dirVector = Vector2Int.left;
                    break;
            }

            return dirVector;
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
		public Dir GetDirFromVec(Vector2Int dir)
		{
			if (dir.x > 0 && dir.y > 0)
				return Dir.Upright;
			else if (dir.x > 0 && dir.y < 0)
                return Dir.Downright;
            else if (dir.x < 0 && dir.y > 0)
				return Dir.Upleft;
			else if (dir.x < 0 && dir.y < 0)
				return Dir.Downleft;
            else if (dir.x > 0 && dir.y == 0)
                return Dir.Right;
            else if (dir.x < 0 && dir.y == 0)
                return Dir.Left;
            else if (dir.x == 0 && dir.y > 0)
                return Dir.Up;
            else 
                return Dir.Down;
        }

		public virtual void OnDamaged(GameObject attacker ,int damage)
        {
			if (Room == null)
				return;
			if (_isInvincibility || State == State.Dead)
				return;
            S_ChangeHp packet = new S_ChangeHp();
            switch (attacker.ObjectType)
            {
                case GameObjectType.Player:

                    break;
				case GameObjectType.Monster:
					packet.EffectId = 4;
					break;
                case GameObjectType.Trigon:
					if(attacker.Info.Prefab == 0)
					{
                        packet.EffectId = 0;
                        if (_preSword == attacker.Id)
                            return;
                        _preSword = attacker.Id;
                        Room.PushAfter(500, SwordCooltimeOver);
                    }
					else if(attacker.Info.Prefab == 1)
					{
                        packet.EffectId = 2;
                    }
 
                    break;
                case GameObjectType.Area:
					packet.EffectId = 1;

                    Dir d = GetDirFromVec(CellPos - attacker.CellPos);
                    Vector2Int dirVector = DirToVector(d);

					Room.Push(Room.HandleSlide, this, dirVector, 3);
                    break;
            }

            StatInfo.Hp -= damage;
			StatInfo.Hp = Math.Max(StatInfo.Hp, 0);

			//Console.WriteLine($"{Info.Name} On Damaged, HP : {StatInfo.Hp} by {attacker.Info.Name}");
			packet.ObjectId = Id;
			packet.Hp = StatInfo.Hp;
			Room.Push(Room.Broadcast,CellPos, packet);

			if(StatInfo.Hp <= 0)
            {
				OnDead(attacker);
            }
        }
		protected bool _isInvincibility = false;
		protected int _preSword;
		protected virtual void SwordCooltimeOver()
		{
			_preSword = 0;
		}
		protected virtual void OnDead(GameObject attacker)
        {
			Room.Map.LeaveCollision(this);
        }

		public virtual void Respone()
		{

		}
	}
}
