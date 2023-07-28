﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Contents
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();
        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();

            Vector2Int pos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZones(pos);

            foreach(Zone zone in zones)
            {
                foreach(Player player in zone.Players)
                {
                    int dx = player.CellPos.x - pos.x;
                    int dy = player.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(player);
                }
                foreach (Projectile projectile in zone.Projectiles)
                {
                    int dx = projectile.CellPos.x - pos.x;
                    int dy = projectile.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(projectile);
                }
            }

            return objects;
        }

        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = GatherObjects();

            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if(added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();

                foreach(GameObject go in added)
                {
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(go.Info);
                    spawnPacket.Objects.Add(info);
                }

                Owner.Session.Send(spawnPacket);
            }

            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();

                foreach (GameObject go in removed)
                {
                    despawnPacket.ObjectIds.Add(go.Id);
                }

                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            Owner.Room.PushAfter(500, Update);

            //test
        }
    }
}