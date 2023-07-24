using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Server.Contents
{
    public struct Pos
    {
        public Pos(int x, int y) { X = x; Y = y; }
        public int X;
        public int Y;
    }
    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        public int X;
        public int Y;

        public int CompareTo(PQNode other)
        {
            if (F == other.F)
                return 0;
            return F < other.F ? 1 : -1;
        }
    }
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) { this.x = x; this.y = y; }

        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int upRight { get { return new Vector2Int(1, 1); } }
        public static Vector2Int upLeft { get { return new Vector2Int(-1, 1); } }
        public static Vector2Int downRight { get { return new Vector2Int(1, -1); } }
        public static Vector2Int downLeft { get { return new Vector2Int(-1, -1); } }

        public static Vector2Int operator + (Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }
    }
    public class Map
    {
        public Room Room { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        int[,] _map;
        public bool CanGo(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;

            if (x >= SizeX || y >= SizeY || x < 0 || y < 0)
                return false;

            if (_map[x, y] == 0)
                return true;
            else
                return false;
        }
        public void MoveObject(GameObject go,Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;

            if (x >= SizeX || y >= SizeY || x < 0 || y < 0)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(go.Id);

            if(type == GameObjectType.Player)
            {
                Player p = (Player)go;

                Zone now = Room.GetZone(go.CellPos);
                Zone after = Room.GetZone(pos);
                if (now != after)
                {
                    now.Players.Remove(p);
                    after.Players.Add(p);
                }
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile p = (Projectile)go;

                Zone now = Room.GetZone(go.CellPos);
                Zone after = Room.GetZone(pos);
                if (now != after)
                {
                    now.Projectiles.Remove(p);
                    after.Projectiles.Add(p);
                }
            }

            _map[go.CellPos.x, go.CellPos.y] = 0;
            _map[x, y] = go.Id;
        }

        public int FindId(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;

            if (x >= SizeX || y >= SizeY || x < 0 || y < 0)
                return 0;

            int id = _map[x, y];

            return id;
        }

        public void LoadMap(int mapid)
        {
            string text = File.ReadAllText("../../../Data/map.txt");
            StringReader reader = new StringReader(text);

            SizeX = int.Parse(reader.ReadLine());
            SizeY = int.Parse(reader.ReadLine());

            _map = new int[SizeX, SizeY];

            for (int y = 0; y < SizeY; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < SizeX; x++)
                {
                    _map[x, y] = (int)Char.GetNumericValue(line[x]);
                }
            }
        }

    }
}
