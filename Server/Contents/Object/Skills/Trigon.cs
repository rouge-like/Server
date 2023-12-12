using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
	public class Trigon : GameObject
	{
        public Trigon()
		{
			ObjectType = GameObjectType.Trigon;
			R = 3.0f;
			Degree = 0;
			Speed = 30.0f;
			StatInfo.Attack = 5;
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
        bool _test = false;

		public override void Init()
		{
            Room.Push(Update);
			StatInfo.Hp = (int)R;
            StatInfo.Level = 1;
        }

		public override void Update()
		{
            if (_test)
                Console.WriteLine($"{Id} is bug");
            if (Room == null)
				return;
			if (Owner == null || Owner.Room == null)
				return;
            Player owner = Owner;
            PosInfo.PosX = owner.CellPos.x;
            PosInfo.PosY = owner.CellPos.y;

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
            owner.Room.Push(owner.Room.Broadcast, owner.CellPos, packet);

            if (Degree > 360)
                Degree -= 360;

            if (Degree < 0)
                Degree += 360;


            _job = Room.PushAfter(100, Update);
		}

        public virtual void CheckAttack() { }

		public virtual void Destroy()
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
            _test = true;
            Console.WriteLine($"{Id} is Destroy");
            Owner.Trigons.Remove(Id);
			Room.Push(Room.LeaveRoom, Id);
            Room = null;
            Owner = null;
        }
        protected override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
			Destroy();
        }
    }
}

