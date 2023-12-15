using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Contents;
using Server.Data;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }
		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		int _reservedSendBytes = 0;
		long _lastSendTick = 0;

		long _pingpongTick = 0;
		int _pingInfo = 0;
		public void Ping()
		{
			if (_pingpongTick > 0)
			{
				long delta = System.Environment.TickCount64 - _pingpongTick;
				if (delta > 30 * 1000)
				{
					Console.WriteLine("Disconnected by PingCheck");
					Disconnect();
					return;
				}
			}

			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);
			_pingInfo = Environment.TickCount;

            RoomManager.Instance.PushAfter(5000, Ping);
		}
		public void HandlePong() 
		{
			S_PingInfo packet = new S_PingInfo();
			packet.Ms = Environment.TickCount - _pingInfo;
			Send(packet);
            _pingpongTick = System.Environment.TickCount64;
			_pingInfo = Environment.TickCount;
        }
		public void Send(IMessage packet)
        {
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (_lock)
            {
				_reserveQueue.Add(sendBuffer);
				_reservedSendBytes += sendBuffer.Length;
            }

		}
		public void Flush()
        {
			List<ArraySegment<byte>> sendList = null;
            lock (_lock)
            {
				if (_reserveQueue.Count == 0)
					return;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
            }
			Send(sendList);
        }
		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			// PROTO Test
			/*
			MyPlayer = ObjectManager.Instance.Add<Player>();
            {
				MyPlayer.Info.Name = $"P{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo = new PosInfo();

				MyPlayer.Session = this;
			}

			RoomManager.Instance.Push(() =>
			{
				Room room = RoomManager.Instance.Find(1);
				room.Push(room.EnterRoom, MyPlayer);
			});*/
			{
				S_Connected packet = new S_Connected();
				Send(packet);
			}

			RoomManager.Instance.PushAfter(5000, Ping);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Push(() =>
			{
				if (MyPlayer == null)
					return;

				Room room = RoomManager.Instance.Find(1);				
				room.Push(room.LeaveRoom, MyPlayer.Info.ObjectId);
				room.Push(MyPlayer.Vision.Destroy);
			});

			SessionManager.Instance.Remove(this);
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
