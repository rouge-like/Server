using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Area : GameObject
	{
		public Data.Skill Data { get; set; }
		public Area()
		{
			ObjectType = GameObjectType.Area;
		}
		public GameObject Owner;

		public void Init()
		{
			if (Room == null)
				return;

			Room.PushAfter(300, Destroy);
			OnAttack();
			
		}

		public void OnAttack()
		{
			if (Room == null)
				return;

			int targetId = Room.Map.FindId(CellPos);
			if (targetId != 0 && targetId != 1)
			{
				GameObject target = Room.Find(targetId);
				if(target != Owner)
					target.OnDamaged(Owner, Data.damage);
			}
		}

		public void Destroy()
		{
			Room.Push(Room.LeaveRoom, Id);
		}
	}
}
