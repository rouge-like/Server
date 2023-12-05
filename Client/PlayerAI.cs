using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using ServerCore;

namespace Client
{
	public class PlayerAI
	{
        public int Id;
        public struct Vector2Int
        {
            public int x;
            public int y;

            public Vector2Int(int x, int y) { this.x = x; this.y = y; }

            public static Vector2Int up { get { return new Vector2Int(0, 1); } }
            public static Vector2Int down { get { return new Vector2Int(0, -1); } }
            public static Vector2Int right { get { return new Vector2Int(1, 0); } }
            public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
            public static Vector2Int upRight { get { return new Vector2Int(1, 1); } }
            public static Vector2Int upLeft { get { return new Vector2Int(-1, 1); } }
            public static Vector2Int downRight { get { return new Vector2Int(1, -1); } }
            public static Vector2Int downLeft { get { return new Vector2Int(-1, -1); } }

            public static Vector2Int operator +(Vector2Int a, Vector2Int b)
            {
                return new Vector2Int(a.x + b.x, a.y + b.y);
            }
            public static Vector2Int operator -(Vector2Int a, Vector2Int b)
            {
                return new Vector2Int(a.x - b.x, a.y - b.y);
            }
            public static bool operator ==(Vector2Int a, Vector2Int b)
            {
                return (a.x == b.x) && (a.y == b.y);
            }
            public static bool operator !=(Vector2Int a, Vector2Int b)
            {
                return (a.x != b.x) || (a.y != b.y);
            }
            public float magnitude { get { return (float)Math.Sqrt(sqrMangnitude); } }
            public int sqrMangnitude { get { return (x * x + y * y); } }
            public int cellDist { get { return Math.Abs(x) + Math.Abs(y); } }

        }
        public ServerSession Session;
        public PosInfo PosInfo = new PosInfo();
        public Dir Dir
        {
            get { return PosInfo.Dir; }
            set { PosInfo.Dir = value; }
        }
        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }

            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }
        public Vector2Int GetFrontCellPos(Dir dir)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case Dir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case Dir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case Dir.Right:
                    cellPos += Vector2Int.right;
                    break;
                case Dir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case Dir.Upright:
                    cellPos += Vector2Int.upRight;
                    break;
                case Dir.Upleft:
                    cellPos += Vector2Int.upLeft;
                    break;
                case Dir.Downright:
                    cellPos += Vector2Int.downRight;
                    break;
                case Dir.Downleft:
                    cellPos += Vector2Int.downLeft;
                    break;

            }

            return cellPos;
        }
        public void Init()
        {
            PosInfo.State = State.Moving;
            Change();
        }
        int _updateCount;
        int _updateChange;

        public void Update()
        {
            _updateCount++;
            PosInfo.State = State.Moving;
            switch (PosInfo.State)
            {
                case State.Idle:
                    {
                        UpdateIdle();
                    }
                    break;
                case State.Moving:
                    {
                        UpdateMoving();
                    }
                    break;
            }
        }
        void Change()
        {
            _updateCount = 0;
            Random rand = new Random();
            int value = rand.Next(10);
            _updateChange = value;
            if (value % 2 == 0)
            {
                PosInfo.State = State.Idle;
                SendStop();
            }

            else
                PosInfo.State = State.Moving;
            Dir = (Dir)rand.Next(8);
        }
        void UpdateIdle()
        {
            if (_updateCount > _updateChange)
                Change();
        }
        void UpdateMoving()
		{
            if (_updateCount > _updateChange)
            {
                Change();
                return;
            }
            if(_updateCount > _updateChange - 3)
            {
                C_Skill skillPacket = new C_Skill() { Info = new SkillInfo() };
                skillPacket.Info.SkillId = 1;
                Send(skillPacket);
                return;
            }
            SendMove(GetFrontCellPos(Dir));
        }
        void SendMove(Vector2Int pos)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = new PosInfo();
            movePacket.PosInfo.PosX = pos.x;
            movePacket.PosInfo.PosY = pos.y;
            movePacket.PosInfo.State = State.Moving;
            movePacket.PosInfo.Dir = Dir;
            Send(movePacket);
        }
        void SendStop()
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Send(movePacket);
        }
        public void Send(IMessage packet)
        {
            if (Session == null)
                return;
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)System.Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Session.Send(new ArraySegment<byte>(sendBuffer));
        }

    }
}

