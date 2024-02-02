using System;
namespace Server.Contents
{
	public class Passive : Weapon
	{
		public Passive()
		{

		}
		protected int _coolTime;
        protected IJob _job;
        public GameObject Owner;
        protected IWeaponAble Weapon;
        public override void Init()
        {
            base.Init();
            StatInfo.Level = 1;
            Weapon = (IWeaponAble)Owner;
        }
        public virtual void Destroy()
        {
            if (Room == null)
                return;
            if (Owner == null || Owner.Room == null)
                return;
            if (_job != null)
            {
                //Console.WriteLine("Cancel Job");
                _job.Cancel = true;
                _job = null;
            }
            Weapon.Passives.Remove(Id);
            Owner = null;
            Room = null;
        }
		public virtual void Attack() { }
	}
}

