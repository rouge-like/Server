using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Area : Weapon
	{
		public Area()
		{
			ObjectType = GameObjectType.Area;
		}
		public GameObject Owner;
        public List<List<int>> AttackArea;
        public GameObject Target;
        public int AttackCount;
        public int AdditionalAttack;
        IJob _job;

        public override void Init()
		{
			if (Room == null)
				return;

            Room.Push(Update);
        }

        int _attack = 0;

        public override void Update()
        {
            if (_attack < AttackCount)
            {

                _attack++;
                OnAttack();
                _job = Room.PushAfter(100, Update);
            }
            else
            {
                Room.Push(Room.LeaveRoom, Id);
            }
        }

        public void OnAttack()
		{
			if (Room == null)
				return;
            foreach(List<int> list in AttackArea)
            {
                Vector2Int pos = new Vector2Int(CellPos.x + list[0], CellPos.y + list[1]);

                int targetId = Room.Map.FindId(pos);
                if (targetId != 0 && targetId != 1)
                {
                    GameObject target = Room.Find(targetId);
                    if (target == null)
                        continue;
                    if(((IWeaponAble)Owner).Target != null && target.ObjectType != GameObjectType.Player)
                        continue;
                    
                    if (target != Owner)
                        target.OnDamaged(this, (int)(StatInfo.Attack * (AdditionalAttack / 100f) * (Owner.StatInfo.Attack + ((IWeaponAble)Owner).PlayerStat.Attack)));
                }
            }
        }
    }
}
