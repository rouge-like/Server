﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Zone
    {
        public int IndexX { get; private set; }
        public int IndexY { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();

        public Zone(int x, int y)
        {
            IndexX = x;
            IndexY = y;
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
    }
}