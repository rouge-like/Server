using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
	public class trigon : GameObject
	{
		public trigon()
		{
			
		}

		public GameObject Owner;
		public float R;
        public float x;
        public float y;
        float deg;


		bool isSame(Vector2 a, Vector2 b, Vector2 p, Vector2 q)
		{
			float c1 = (b - a) * (p - a);
			float c2 = (b - a) * (q - a);

			return c1 * c2 > 0;
		}

		public bool inTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
		{
			bool a1 = isSame(a, b, c, p);
			bool a2 = isSame(b, c, a, p);
			bool a3 = isSame(c, a, b, p);

			return a1 && a2 && a3;
		}

		public override void Update()
		{
			deg += Speed;

			x = PosInfo.PosX + (float)(Math.Cos(deg) * R);
			y = PosInfo.PosY + (float)(Math.Sin(deg) * R);
			
			Room.PushAfter(500, Update);
		}
	}
}

