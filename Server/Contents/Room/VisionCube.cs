﻿using Google.Protobuf.Protocol;
using Server.Contents.Object;
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

                    foreach (Trigon trigon in player.Trigons.Values)
                    {
                        objects.Add(trigon);
                    }
                }
                foreach(Monster monster in zone.Monsters)
                {
                    int dx = monster.CellPos.x - pos.x;
                    int dy = monster.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(monster);
                    if(monster.GetWeapon() != null)
                    {
                        TargetMonster targetMonster = (TargetMonster)monster;
                        foreach (Trigon trigon in targetMonster.Trigons.Values)
                        {
                            objects.Add(trigon);
                        }
                    }
                }
                foreach (Projectile projectile in zone.Projectiles)
                {
                    int dx = projectile.CellPos.x - pos.x;
                    int dy = projectile.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(projectile);
                }                
                foreach (Area area in zone.Areas)
                {
                    int dx = area.CellPos.x - pos.x;
                    int dy = area.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(area);
                }
                foreach (Item item in zone.Items)
                {
                    int dx = item.CellPos.x - pos.x;
                    int dy = item.CellPos.y - pos.y;

                    if (Math.Abs(dx) > Room.VisionCells || Math.Abs(dy) > Room.VisionCells)
                        continue;
                    objects.Add(item);
                }
            }

            return objects;
        }
        IJob _job;
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

                if(Owner.Session != null)
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
                if(Owner.Session != null)
                    Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            _job = Owner.Room.PushAfter(100, Update);
        }
        public void UpdateImmediate()
        {
            if (Owner == null || Owner.Room == null)
                return;
            if (_job != null) {
                _job.Cancel = true;
                _job = null;
            }

            HashSet<GameObject> currentObjects = GatherObjects();

            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();

                foreach (GameObject go in added)
                {
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(go.Info);
                    spawnPacket.Objects.Add(info);
                }

                if(Owner.Session != null)
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
                if(Owner.Session != null)
                    Owner.Session.Send(despawnPacket);
            }   

            PreviousObjects = currentObjects;
            _job = Owner.Room.PushAfter(100, Update);
        }
        public void Start()
        {
            if(_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
            _job = Owner.Room.PushAfter(100, Update);
        }
        public void Destroy()
        {
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
            Owner = null;
        }
    }
}
