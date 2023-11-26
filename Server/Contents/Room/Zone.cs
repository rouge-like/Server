using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;
using Server.Contents.Object;

namespace Server.Contents
{
    public class Zone
    {
        public int IndexX { get; private set; }
        public int IndexY { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();
        public HashSet<Trigon> Trigons { get; set; } = new HashSet<Trigon>();
        public HashSet<Fire> Fires { get; set; } = new HashSet<Fire>();
        public HashSet<Item> Items { get; set; } = new HashSet<Item>();


        public Zone(int x, int y)
        {
            IndexX = x;
            IndexY = y;
        }

        public void Remove(GameObject go)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(go.Id);

            switch (type)
            {
                case GameObjectType.Player:
                    Players.Remove((Player)go);
                    break;
                case GameObjectType.Monster:
                    Monsters.Remove((Monster)go);
                    break;
                case GameObjectType.Dagger:
                    Projectiles.Remove((Projectile)go);
                    break;
                case GameObjectType.Sword:
                    Trigons.Remove((Trigon)go);
                    break;
                case GameObjectType.Lightning:
                    Trigons.Remove((Trigon)go);
                    break;
                case GameObjectType.Fire:
                    Fires.Remove((Fire)go);
                    break;
                case GameObjectType.Item:
                    Items.Remove((Item)go);
                    break;
            }

        }
        public Player FindOne(Func<Player, bool> condition)
        {
            foreach(Player player in Players)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        public List<Player> FindAll(Func<Player, bool> condition)
        {
            List<Player> players = new List<Player>();
            foreach(Player player in Players)
            {
                if (condition.Invoke(player))
                    players.Add(player);
            }
            return players;
        }
        public List<Player> FindAll()
        {
            List<Player> players = new List<Player>();
            foreach (Player player in Players)
            {
                players.Add(player);
            }
            return players;
        }

    }
}
