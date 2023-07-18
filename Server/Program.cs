using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Contents;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

		static void TickRoom(Room room, int tick = 100)
        {
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += (sender, e) => { room.Update(); }; // Object sender, ElapsedEventArgs e
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
        }

		static void Main(string[] args)
		{
			Room room = RoomManager.Instance.Add();
			TickRoom(room, 50);

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Host Name : " + host);
            Console.WriteLine("IP Adress : " + ipAddr.ToString());
			Console.WriteLine("Listening...");

			while (true)
			{
				RoomManager.Instance.Find(1).Update();

				Thread.Sleep(100);
			}
		}
	}
}
