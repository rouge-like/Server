using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Map
    {
        int _lenX = 20;
        int _lenY = 30;
        public int[,] _grid;
        public static Map Grid { get; } = new Map();
        public void Init()
        {
            _grid = new int[_lenX, _lenY];
            for (int y = 0; y < _lenY; y++)
            {
                for(int x = 0; x< _lenX; x++)
                {
                    if (x == 0 || x == _lenX - 1)
                        _grid[x, y] = 1;
                    if (y == 0 || y == _lenY - 1)
                        _grid[x, y] = 1;
                }
            }
        }

    }
}
