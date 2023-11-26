using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Circler : GameObject
	{
		public Data.Skill Data { get; set; }
		int _count;
		public Circler()
		{
			//ObjectType = GameObjectType.Circler;
			_count = 0;
		}
		public Player Owner;

		public override void Update()
		{
			if (Data == null || Owner == null || Room == null)
				return;
			Vector2Int desPos;
			List<int> posList;
			if (_count >= Data.circle.len)
			{
				posList = Data.circle.posList[0];
				Room.PushAfter(50, Destroy);
				_count = 7;
			}
			else
			{
				posList = Data.circle.posList[_count];
				Room.PushAfter(100, Update);
			}
				
			_count++;
			desPos = new Vector2Int(Owner.CellPos.x + posList[0], Owner.CellPos.y + posList[1]);
			if (desPos.x < 0 || desPos.x >= Room.Map.SizeX || desPos.y < 0 || desPos.y >= Room.Map.SizeY)
				return;

			Zone now = Room.GetZone(CellPos);
			Zone after = Room.GetZone(desPos);
			if (now != after)
			{
				//now.Circlers.Remove(this);
				//after.Circlers.Add(this);
			}

			PosInfo.PosX = desPos.x;
			PosInfo.PosY = desPos.y;

			S_Move movePakcet = new S_Move();
			movePakcet.ObjectId = Id;
			movePakcet.PosInfo = PosInfo;
			Room.Broadcast(CellPos, movePakcet);
		}
		public void UpdateByPlayer()
		{
			if (Data == null || Owner == null || Room == null)
				return;

			List<int> posList = Data.circle.posList[_count - 1];
			Vector2Int desPos = new Vector2Int(Owner.CellPos.x + posList[0], Owner.CellPos.y + posList[1]);
			if (desPos.x < 0 || desPos.x >= Room.Map.SizeX || desPos.y < 0 || desPos.y >= Room.Map.SizeY)
				return;

			Zone now = Room.GetZone(CellPos);
			Zone after = Room.GetZone(desPos);
			if (now != after)
			{
				//now.Circlers.Remove(this);
				//after.Circlers.Add(this);
			}

			PosInfo.PosX = desPos.x;
			PosInfo.PosY = desPos.y;

			S_Move movePakcet = new S_Move();
			movePakcet.ObjectId = Id;
			movePakcet.PosInfo = PosInfo;
			Room.Broadcast(CellPos, movePakcet);


		}
		public void Destroy()
		{
			_count = 0;
			Room.Push(Room.LeaveRoom, Id);
		}
		// 몇 프레임 후 다음 구역으로 이동하는가?
		// 한바퀴 도는데 몇 프레임(초)가 걸리는가?
		// 한바퀴 2초 총 8칸  1칸당 0.25초
	}
}
