using System;
using ServerCore;
using System.Net;
using System.Threading;

namespace Client
{

    class Program
    {
        static int DummyClientCount = 100;

        static void Main(string[] args)
        {
            Thread.Sleep(3000);

            IPAddress ipAddr = IPAddress.Parse("192.168.1.235");  // 172.20.10.6 Iphone 192.168.51.61 
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return SessionManager.Instance.Generate(); },
                DummyClientCount);

            while (true)
            {
                SessionManager.Instance.Update();
                Thread.Sleep(200);
            }
        }
    }
}
