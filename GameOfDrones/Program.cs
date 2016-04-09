using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int numberOfPlayers = int.Parse(inputs[0]); // number of players in the game (2 to 4 players)
        int me = int.Parse(inputs[1]); // ID of your player (0, 1, 2, or 3)
        int dronesPerPlayer = int.Parse(inputs[2]); // number of drones in each team (3 to 11)
        int Z = int.Parse(inputs[3]); // number of zones on the map (4 to 8)

        var zones = new Point[Z];
        for (int i = 0; i < Z; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]); // corresponds to the position of the center of a zone. A zone is a circle with a radius of 100 units.
            int Y = int.Parse(inputs[1]);
            zones[i] = new Point(X, Y);
        }

        // game loop
        while (true)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                int TID = int.Parse(Console.ReadLine()); // ID of the team controlling the zone (0, 1, 2, or 3) or -1 if it is not controlled. The zones are given in the same order as in the initialization.
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                for (int j = 0; j < dronesPerPlayer; j++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int DX = int.Parse(inputs[0]); // The first D lines contain the coordinates of drones of a player with the ID 0, the following D lines those of the drones of player 1, and thus it continues until the last player.
                    int DY = int.Parse(inputs[1]);
                }
            }
            for (int i = 0; i < dronesPerPlayer; i++)
            {
                Console.WriteLine(zones[i % zones.Length]);
            }
        }
    }

    class Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", X, Y);
        }
    }
}