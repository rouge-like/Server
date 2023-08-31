using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
    public class Zone
    {
        public int IndexX { get; private set; }
        public int IndexY { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();
        public HashSet<Area> Areas { get; set; } = new HashSet<Area>();
        public HashSet<Circler> Circlers { get; set; } = new HashSet<Circler>();
        public HashSet<Trigon> Trigons { get; set; } = new HashSet<Trigon>();

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
                    break;
                case GameObjectType.Projectile:
                    Projectiles.Remove((Projectile)go);
                    break;
                case GameObjectType.Area:
                    Areas.Remove((Area)go);
                    break;
                case GameObjectType.Circler:
                    Circlers.Remove((Circler)go);
                    break;
                case GameObjectType.Trigon:
                    Trigons.Remove((Trigon)go);
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
