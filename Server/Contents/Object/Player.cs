using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
    }
}
