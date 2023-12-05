using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Area : GameObject
	{
		public Area()
		{
			ObjectType = GameObjectType.Area;
		}
		public GameObject Owner;
        public List<List<int>> AttackArea;
        public int AttackCount;
        IJob _job;

        public override void Init()
		{
			if (Room == null)
				return;

            _job = Room.PushAfter(100, Update);
        }

        int _attack = 0;

        public override void Update()
        {
            if (_attack < AttackCount)
            {

                _attack++;
                OnAttack();
                _job = Room.PushAfter(200, Update);
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
                    if (target != Owner)
                        target.OnDamaged(this, StatInfo.Attack * Owner.StatInfo.Attack);
                }
            }
        }
    }
}
