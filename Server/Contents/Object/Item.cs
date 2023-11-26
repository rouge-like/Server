using System;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Item : GameObject
	{
		public Item()
		{
			ObjectType = GameObjectType.Item;
		}

        int _count;
        int _destoryCount;
        IJob _job;
        public bool Destroyed;
        public ItemType ItemType;
        public int value;

        // 가까이 범위에오면 먹어짐 이건 플레이어에게 다는 편이 좋을듯
        // 아이템 코드 필요 스킬과 유사하게
        // 정보만 담고 있어야하나?
        // 시간 지나면 디스트로이

        public override void Init()
        {
            _count = 0;
            _destoryCount = 100;
            _job = Room.PushAfter(100, Update);
        }
        public override void Update()
        {
            base.Update();
            _count++;
            if (_count > _destoryCount)
            {
                Destroy();
                return;
            }
            /**Zone zone = Room.GetZone(CellPos);
            foreach (Player p in zone.Players)
            {
                if (p.CellPos == CellPos)
                    Room.Push(p.EarnItem, this);
            }**/
            _job = Room.PushAfter(100, Update);
        }

        public void Destroy()
        {
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
            Room.Push(Room.LeaveRoom, Id);
        }
    }
}

