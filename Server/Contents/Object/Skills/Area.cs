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
			//ObjectType = GameObjectType.Area;
		}
		public GameObject Owner;

		public override void Init()
		{
			if (Room == null)
				return;

			Room.PushAfter(300, Destroy);
			foreach(List<int> list in Data.area.posList)
			{
				Vector2Int pos = new Vector2Int(list[0], list[1]);
				OnAttack(pos);
			}
		}

		public void OnAttack(Vector2Int pos)
		{
			if (Room == null)
				return;

			int targetId = Room.Map.FindId(pos);
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
