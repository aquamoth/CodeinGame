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
        var inputs = Console.ReadLine().Split(' ');
        int numberOfPlayers = int.Parse(inputs[0]); // number of players in the game (2 to 4 players)
        int me = int.Parse(inputs[1]); // ID of your player (0, 1, 2, or 3)
        int dronesPerPlayer = int.Parse(inputs[2]); // number of drones in each team (3 to 11)
        int Z = int.Parse(inputs[3]); // number of zones on the map (4 to 8)

        var zones = createZones(Z);
        var drones = createDrones(numberOfPlayers, dronesPerPlayer);

        // game loop
        while (true)
        {
            gameLoop(zones, drones, me);
        }
    }

    private static void gameLoop(Zone[] zones, Drone[] drones, int me)
    {
        updateZonesFromConsole(zones);
        updateDronesFromConsole(drones);
        mapDronesToZones(drones, zones);

        var myDrones = drones.Where(d => d.Team == me).ToArray();

        var dronesRequiredPerZone = zones.Select(z => z
            .Drones
                .Where(d => d.Team != me)
                .GroupBy(d => d.Team)
                .Select(grp => grp.Count())
                .Max()
            + (z.Team == me ? 0 : 1));




        //var dronesOutsideZones = myDrones.Where(d => d.Zone == null);//.ToArray();
        //var dronesAtOpponentsZones = myDrones.Where(d => d.Zone != null && d.Zone.Team != me);
        //var super

        for (int i = 0; i < myDrones.Length; i++)
        {
            Console.WriteLine(zones[i % zones.Length]);
        }
    }

    #region Refresh State

    private static void updateDronesFromConsole(Point[] drones)
    {
        for (int i = 0; i < drones.Length; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            drones[i].X = int.Parse(inputs[0]);
            drones[i].Y = int.Parse(inputs[1]);
        }
    }

    private static void updateZonesFromConsole(Point[] zones)
    {
        for (int i = 0; i < zones.Length; i++)
        {
            int TID = int.Parse(Console.ReadLine()); // ID of the team controlling the zone (0, 1, 2, or 3) or -1 if it is not controlled. The zones are given in the same order as in the initialization.
            zones[i].Team = TID;
        }
    }

    private static void mapDronesToZones(Drone[] drones, Zone[] zones)
    {
        foreach (var d in drones)
            d.Zone = null;

        foreach (var zone in zones)
        {
            zone.Drones = drones.Where(d => d.SquareDistanceTo(zone) < 10000).ToArray();
            foreach (var d in zone.Drones)
                d.Zone = zone;
        }
    }

    #endregion Refresh State

    #region Initialize Game

    private static Zone[] createZones(int Z)
    {
        var zones = new Zone[Z];
        for (int i = 0; i < Z; i++)
        {
            var xinputs = Console.ReadLine().Split(' ');
            int X = int.Parse(xinputs[0]); // corresponds to the position of the center of a zone. A zone is a circle with a radius of 100 units.
            int Y = int.Parse(xinputs[1]);
            zones[i] = new Zone { X = X, Y = Y };
        }

        return zones;
    }

    private static Drone[] createDrones(int numberOfPlayers, int dronesPerPlayer)
    {
        return Enumerable.Range(0, numberOfPlayers)
            .SelectMany(p => Enumerable.Repeat(0, dronesPerPlayer)
            .Select(x => new Drone { Team = p }))
            .ToArray();
    }

    #endregion Initialize Game

    #region Helper Classes

    class Zone : Point
    {
        public Drone[] Drones { get; set; }
    }

    class Drone : Point
    {
        public Zone Zone { get; set; }
    }

    class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int? Team { get; set; }

        //public Point(int x, int y)
        //{
        //    X = x;
        //    Y = y;
        //}

        public override string ToString()
        {
            return string.Format("{0} {1}", X, Y);
        }

        internal int SquareDistanceTo(Zone zone)
        {
            return (int)Math.Pow(this.X - zone.X, 2) + (int)Math.Pow(this.Y - zone.Y, 2);
        }
    }

    #endregion Helper Classes
}