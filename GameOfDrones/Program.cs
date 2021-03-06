﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
        //log("Input: {0}", string.Join(" ", inputs));

        var zones = readZonesFromConsole(Z);
        var drones = createDrones(numberOfPlayers, dronesPerPlayer);

        // game loop
        while (true)
        {
            log_time = null;
            gameLoop(zones, drones, me);
        }
    }

    private static void gameLoop(Zone[] zones, Drone[] drones, int me)
    {
        updateZonesFromConsole(zones);
        updateDronesFromConsole(drones);
        mapDronesToZones(drones, zones, me);

        var myDrones = drones.Where(d => d.Team == me).ToArray();

        var commands = solve(myDrones, zones);
        var solution = flatten(commands).ToDictionary(c => c.Drone.Index, c => c);

        for (int i = 0; i < myDrones.Length; i++)
        {
            var destinationZone = solution.ContainsKey(i) ? solution[i].Zone : null;
            log("Moving drone #{0} to zone #{1}", i, destinationZone == null ? "-" : destinationZone.Index.ToString());

            var destination = solution.ContainsKey(i) ? solution[i].Destination : new Point { X = 1000, Y = 500 };
            Console.WriteLine(destination);
        }
    }

    private static IEnumerable<Command> flatten(Command command)
    {
        var walker = command;
        while(walker != null)
        {
            yield return walker;
            walker = walker.Next;
        }
    }

    static Command solve(IEnumerable<Drone> myDrones, Zone[] zones, bool IsFirstLoop = true)
    {
        if (!myDrones.Any())
            return null;

        var drone = myDrones.First();
        Command bestCommand = null;

        foreach (var zone in zones.Where(z => z.RequiredDrones > 0))
        {
            //log("Testing drone #{0} at zone #{1}", drone.Index, zone.Index);
            var oldZoneTurns = zone.Turns;
            zone.RequiredDrones--;

            var overTakesZone = zone.RequiredDrones == 0;
            zone.Turns = Math.Max(zone.Turns, drone.SquareDistanceTo(zone) / 10000 - 1);
            var nextCommand = solve(myDrones.Skip(1), zones, false);

            var isBest = bestCommand == null;
            if (!isBest)
            {
                var bestCommandArray = flatten(bestCommand);
                var bestOverTakenZones = bestCommandArray.Select(c => c.OvertakesZone).Count();

                var commandArray = flatten(nextCommand);
                var overTakenZones = commandArray.Select(c => c.OvertakesZone).Count();

                isBest = overTakenZones > bestOverTakenZones;
                if (overTakenZones == bestOverTakenZones)
                {
                    var bestScore = bestCommandArray.Where(c => c.OvertakesZone).Sum(c => c.Turns);
                    var score = commandArray.Where(c => c.OvertakesZone).Sum(c => c.Turns);
                    isBest = score > bestScore;
                }
            }
            if (isBest)
            {
                //log("Choosing drone #{0} to zone #{1}", drone.Index, zone.Index);
                bestCommand = new Command
                {
                    Drone = drone,
                    Zone = zone,
                    Destination = zone, //TODO: Move to the outer rim of the zone
                    OvertakesZone = overTakesZone,
                    Turns = overTakesZone ? zone.Turns : 0,
                    Next = nextCommand
                };
            }
            zone.RequiredDrones++;
            zone.Turns = oldZoneTurns;
        }

        if (bestCommand == null) //There are NO zones to win
        {
            //TODO: Also choose this if the best zone still can't be captured

            //log("No zone to win for drone #{0}.", drone.Index);
            //TODO: Move to a strategic position instead
            var centerOfZones = new Point
            {
                X = (int)zones.Select(z => z.X).Average(),
                Y = (int)zones.Select(z => z.Y).Average()
            };
            bestCommand = new Command
            {
                Drone = drone,
                Destination = centerOfZones,
                Next = solve(myDrones.Skip(1), zones, false)
            };
        }

        return bestCommand;
    }

    #region Refresh State

    private static void updateZonesFromConsole(Point[] zones)
    {
        for (int i = 0; i < zones.Length; i++)
        {
            int TID = int.Parse(Console.ReadLine()); // ID of the team controlling the zone (0, 1, 2, or 3) or -1 if it is not controlled. The zones are given in the same order as in the initialization.
            zones[i].Team = TID;
            //log("{0}", TID);
        }
    }

    private static void updateDronesFromConsole(Point[] drones)
    {
        for (int i = 0; i < drones.Length; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            drones[i].X = int.Parse(inputs[0]);
            drones[i].Y = int.Parse(inputs[1]);
            //log("{0}", drones[i]);
        }
    }

    private static void mapDronesToZones(Drone[] drones, Zone[] zones, int me)
    {
        foreach (var d in drones)
            d.Zone = null;

        foreach (var zone in zones)
        {
            zone.Drones = drones.Where(d => d.SquareDistanceTo(zone) < 10000).ToArray();
            foreach (var d in zone.Drones)
                d.Zone = zone;

            zone.RequiredDrones = (zone.Team == me ? 0 : 1)
                + zone.Drones
                    .Where(d => d.Team != me)
                    .GroupBy(d => d.Team)
                    .Select(grp => grp.Count())
                    .DefaultIfEmpty(0)
                    .Max();

            //log("Zone {0} has {1} drones, is controlled by {2} and requires {3} drones", zone.Index, (zone.Drones==null?0:zone.Drones.Count()), zone.Team, zone.RequiredDrones);
        }
    }

    static void log(string format, params object[] args)
    {
        if (log_time == null)
        {
            log_time = new Stopwatch();
            log_time.Start();
        }
        Console.Error.WriteLine(log_time.ElapsedMilliseconds + " ms: " + format, args);
    }
    static Stopwatch log_time = null;

    #endregion Refresh State

    #region Initialize Game

    private static Zone[] readZonesFromConsole(int Z)
    {
        var zones = new Zone[Z];
        for (int i = 0; i < Z; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]); // corresponds to the position of the center of a zone. A zone is a circle with a radius of 100 units.
            int Y = int.Parse(inputs[1]);
            zones[i] = new Zone { Index = i, X = X, Y = Y };
            //log("{0}", zones[i]);
        }
        return zones;
    }

    private static Drone[] createDrones(int numberOfTeams, int dronesPerTeam)
    {
        return Enumerable.Range(0, numberOfTeams)
            .SelectMany(team => Enumerable.Range(0, dronesPerTeam)
            .Select(index => new Drone { Team = team, Index = index }))
            .ToArray();
    }

    #endregion Initialize Game

    #region Helper Classes

    class Command
    {
        public Drone Drone { get; set; }
        public Zone Zone { get; set; }
        public bool OvertakesZone { get; set; }
        public int Turns { get; set; }
        public Point Destination { get; set; }
        public Command Next { get; set; }
    }

    class Zone : Point
    {
        public int Index { get; set; }
        public Drone[] Drones { get; set; }
        public int RequiredDrones { get; set; }
        public int Turns { get; set; }
    }

    class Drone : Point
    {
        public int Index { get; set; }
        public Zone Zone { get; set; }
    }

    class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int? Team { get; set; }

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