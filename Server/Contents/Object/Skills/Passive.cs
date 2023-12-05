using System;
namespace Server.Contents
{
	public class Passive : GameObject
	{
		public Passive()
		{

		}
		protected int _coolTime;
        protected IJob _job;
        public Player Owner;
        public override void Init()
        {
            base.Init();
            StatInfo.Level = 1;
        }
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
            Owner.Passives.Remove(Id);
            Owner = null;
            Room = null;
        }
		public virtual void Attack() { }
	}
}

