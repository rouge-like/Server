using Google.Protobuf.Protocol;
using System;
using ServerCore;
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

        public static bool operator == (Pos l, Pos s)
        {
            return l.Y == s.Y && l.X == s.X;
        }
        public static bool operator != (Pos l, Pos s)
        {
            return !(l == s);
        }
        public override bool Equals(object obj)
        {
            return (Pos)obj == this;
        }
        public override int GetHashCode()
        {
            long value = (Y << 32) | X;
            
            return value.GetHashCode();
        }
        public override string ToString()
        {
            return base.ToString();
        }
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
        public static Vector2Int operator - (Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }
        public static bool operator == (Vector2Int a, Vector2Int b)
        {
            return (a.x == b.x) && (a.y == b.y);
        }
        public static bool operator != (Vector2Int a, Vector2Int b)
        {
            return (a.x != b.x) || (a.y != b.y);
        }
        public float magnitude { get { return (float)Math.Sqrt(sqrMangnitude); } }
        public int sqrMangnitude { get { return (x * x + y * y); } }
        public int cellDist { get { return Math.Abs(x) + Math.Abs(y); } }

    }
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 operator + (Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        public static Vector2 operator - (Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        public static float operator * (Vector2 a, Vector2 b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }
    }
    public class Map
    {
        public Room Room { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        int[,] _map;
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
        public bool LeaveObject(GameObject go)
        {
            if (go.Room == null)
                return false;
            if (go.Room.Map != this)
                return false;

            PosInfo posInfo = go.PosInfo;
            if (posInfo.PosX >= SizeX || posInfo.PosY >= SizeY || posInfo.PosX < 0 || posInfo.PosY < 0)
                return false;

            Zone zone = go.Room.GetZone(go.CellPos);
            zone.Remove(go);

            if (_map[posInfo.PosX, posInfo.PosY] == go.Id)
                _map[posInfo.PosX, posInfo.PosY] = 0;
            
            return true;
        }
        public void MoveObject(GameObject go,Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;

            if (x >= SizeX || y >= SizeY || x < 0 || y < 0)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(go.Id);
            PosInfo posInfo = go.PosInfo;

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

                _map[go.CellPos.x, go.CellPos.y] = 0;
                _map[x, y] = go.Id;
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
            else if (type == GameObjectType.Monster)
            {
                Monster m = (Monster)go;

                Zone now = Room.GetZone(go.CellPos);
                Zone after = Room.GetZone(pos);
                if(now != after)
                {
                    now.Monsters.Remove(m);
                    after.Monsters.Add(m);
                }
                _map[go.CellPos.x, go.CellPos.y] = 0;
                _map[x, y] = go.Id;
            }

            posInfo.PosX = pos.x;
            posInfo.PosY = pos.y;
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
        public Pos Cell2Pos(Vector2Int v)
        {
            Pos pos = new Pos();
            pos.X = v.x;
            pos.Y = v.y;
            return pos;
        }
        public Vector2Int Pos2Cell(Pos p)
        {
            Vector2Int v = new Vector2Int();
            v.x = p.X;
            v.y = p.Y;
            return v;
        }
        int[] _deltaY = new int[] { 1, -1, 0, 0, 1, 1, -1, -1};
        int[] _deltaX = new int[] { 0, 0, 1, -1, 1, -1, 1, -1};
        int[] _cost = new int[] { 10, 10, 10, 10, 14, 14, 14, 14 };

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            List<Pos> path = new List<Pos>();

            HashSet<Pos> closeList = new HashSet<Pos>();
            Dictionary<Pos, int> openList = new Dictionary<Pos, int>();
            Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

            Pos pos = Cell2Pos(start);
            Pos dest = Cell2Pos(end);

            openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));

            pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X});
            parent.Add(pos, pos);

            while(pq.Count > 0)
            {
                PQNode pqNode = pq.Pop();
                Pos node = new Pos(pqNode.X, pqNode.Y);

                if (closeList.Contains(node))
                    continue;

                closeList.Add(node);

                if (node.Y == dest.Y && node.X == dest.X)
                    break;

                for(int i = 0; i < _deltaY.Length; i++)
                {
                    Pos next = new Pos(node.X + _deltaX[i], node.Y + _deltaY[i]);

                    if(next.Y != dest.Y || next.X != dest.X)
                    {
                        if (CanGo(Pos2Cell(next)) == false)
                            continue;
                    }

                    if (closeList.Contains(next))
                        continue;

                    int g = _cost[i];
                    int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                    int value;
                    if (openList.TryGetValue(next, out value) == false)
                        value = Int32.MaxValue;
                    if (value < g + h)
                        continue;

                    if (openList.TryAdd(next, g + h) == false)
                        openList[next] = g + h;

                    pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                    if (parent.TryAdd(next, node) == false)
                        parent[next] = node;
                }
            }

            return CalcCellPathFromParent(parent, dest);
        }

        List<Vector2Int> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            Pos pos = dest;
            while (parent[pos] != pos)
            {
                cells.Add(Pos2Cell(pos));
                pos = parent[pos];
            }

            cells.Add(Pos2Cell(pos));
            cells.Reverse();

            return cells;
        }
    }
}
