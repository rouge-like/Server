using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Map.Grid.Init();
            Console.WriteLine($"{Map.Grid._grid}");
        }
    }
}
