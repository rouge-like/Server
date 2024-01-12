using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Protobuf.Protocol;
using Server.Contents;
using Server.Data;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static void RoomTask()
        {
            while (true)
            {
				RoomManager.Instance.Update();
				Thread.Sleep(0);
            }
        }
		static void NetworkTask()
        {
            while (true)
            {

			}
        }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			RoomManager.Instance.Push(() => { RoomManager.Instance.Add(0); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[3];// ipHost.AddressList[3];// IPAddress.Parse("192.168.51.30");
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Host Name : " + host);
            Console.WriteLine("IP Adress : " + ipAddr.ToString());
			Console.WriteLine("End Point : " + endPoint.ToString());
			Console.WriteLine("Listening...");

			FirebaseApp.Create(new AppOptions()
			{
				Credential = GoogleCredential.FromFile("../../../luckysurvior-5e2d9-36f0d4345e3c.json")
			});

			Task roomTask = new Task(RoomTask, TaskCreationOptions.LongRunning);
			roomTask.Start();

			while (true)
			{
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
				foreach (ClientSession session in sessions)
					session.Flush();
				Thread.Sleep(0);
			}
		}
	}
}
