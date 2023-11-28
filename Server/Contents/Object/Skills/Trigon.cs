using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
	public class Trigon : GameObject
	{
        public Data.Skill Data { get; set; }

        public Trigon()
		{
			ObjectType = GameObjectType.Trigon;
			R = 3.0f;
			Degree = 0;
			Speed = 30.0f;
			StatInfo.Attack = 5;
			StatInfo.MaxHp = 150;
			StatInfo.Hp = 15000;
			X = 0;
			Y = 0; 
			AfterX = 0;
			AfterY = 0;
		}

		public Player Owner;
		public bool IsSword;
		public float R;
        public float X;
        public float Y;
		public float AfterX;
		public float AfterY;
        public float Degree { get { return Info.Degree; } set { Info.Degree = value; } }
		IJob _job;

		public override void Init()
		{
            Room.Push(Update);
        }

		public override void Update()
		{
			if (Room == null)
				return;
			if (Owner == null || Owner.Room == null)
				return;

            PosInfo.PosX = Owner.CellPos.x;
            PosInfo.PosY = Owner.CellPos.y;

            var pi = Math.PI;
            Degree += Speed;

            X = (float)(Math.Cos(Degree * 2 * pi / 360) * R);
            Y = (float)(Math.Sin(Degree * 2 * pi / 360) * R);

            AfterX = (float)(Math.Cos((Degree + Speed) * 2 * pi / 360) * R);
            AfterY = (float)(Math.Sin((Degree + Speed) * 2 * pi / 360) * R);

            S_MoveFloat packet = new S_MoveFloat();
            packet.Degree = Degree;
            packet.ObjectId = Id;
            packet.Dir = Speed > 0;

            Room.Push(Room.Broadcast, Owner.CellPos, packet);

            if (Degree > 360)
                Degree -= 360;

            if (Degree < 0)
                Degree += 360;

            _job = Room.PushAfter(100, Update);
		}

        public virtual void CheckAttack() { }

		public void Destroy()
		{
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;

            if (_job != null)
			{
                Console.WriteLine("Cancel Job");
				_job.Cancel = true;
				_job = null;
			}
			Owner.Trigons.Remove(Id);
			Room.Push(Room.LeaveRoom, Id);
		}
        protected override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
			Destroy();
        }
    }
}

