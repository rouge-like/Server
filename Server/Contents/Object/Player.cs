using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }
      
        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            Console.WriteLine($"{Info.Name} On Damaged, HP : {StatInfo.Hp} by {attacker.Info.Name}");
        }
    }
}
