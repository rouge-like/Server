using System;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Monster : GameObject
	{
		public Monster()
		{
			ObjectType = GameObjectType.Monster;
		}
        public override void OnDead(GameObject attacker)
        {
			base.OnDead(attacker);
            // 아이템을 떨군
        }

		public override void Respone()
		{
			base.Respone();
		}
    }
}

