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
	const int MAP_WIDTH = 30;
	const int MAP_HEIGHT = 20;

	static void Main(string[] args)
	{

		var map = new int?[MAP_HEIGHT * MAP_WIDTH];
		Point[] players = null;

		// game loop
		while (true)
		{
			var inputs = Console.ReadLine().Split(' ');
			var numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
			int myPlayerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

			var positions = readPositionsFromConsole(numberOfPlayers);
			if (players == null)
				players = positions;
			else
				for (var i = 0; i < positions.Length; i++)
				{
					players[i].MoveTo(positions[i]);
				}

			foreach (var player in players.Where(p => p.X >= 0))
			{
				map[player.Y * MAP_WIDTH + player.X] = player.Id;
			}

			printMap(map);

			var me = players[myPlayerNumber];
			var heading = turn(me.Heading, 1);
			if (!isFree(map, me.NextPosition(heading)))
			{
				heading = me.Heading;
				if (!isFree(map, me.NextPosition(heading)))
					heading = turn(me.Heading, 3);
				//if this doesn't work either we are dead anyway..
			}

			Console.WriteLine(heading);
		}
	}

	private static bool isFree(int?[] map, Point p)
	{
		if (p.X < 0 || p.X >= MAP_WIDTH)
			return false;
		if (p.Y < 0 || p.Y >= MAP_HEIGHT)
			return false;
		return !map[p.X + p.Y * MAP_WIDTH].HasValue;
	}

	private static Direction turn(Direction direction, int stepsToRight)
	{
		return (Direction)(((int)direction + stepsToRight) % 4);
	}

	private static Point[] readPositionsFromConsole(int N)
	{
		var playersTurn = Enumerable
						 .Repeat(0, N)
						 .Select(i =>
						 {
							 var inputs = Console.ReadLine().Split(' ');
							 int X0 = int.Parse(inputs[0]); // starting X coordinate of lightcycle (or -1)
							 int Y0 = int.Parse(inputs[1]); // starting Y coordinate of lightcycle (or -1)
							 int X1 = int.Parse(inputs[2]); // starting X coordinate of lightcycle (can be the same as X0 if you play before this player)
							 int Y1 = int.Parse(inputs[3]); // starting Y coordinate of lightcycle (can be the same as Y0 if you play before this player)
							 return new Point(i, X1, Y1, X0, Y0);
						 }).ToArray();
		return playersTurn;
	}

	static void printMap(int?[] map)
	{
		for (var y = 0; y < MAP_HEIGHT; y++)
		{
			for (var x = 0; x < MAP_WIDTH; x++)
			{
				var value = map[y * MAP_WIDTH + x];
				var token = value.HasValue ? value.ToString() : ".";
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("");
		}
	}
}

public class Point
{
	public int Id { get; private set; }
	public int X { get; set; }
	public int Y { get; set; }
	public int X0 { get; private set; }
	public int Y0 { get; private set; }

	public Point(int id, int x, int y, int x0, int y0)
	{
		Id = id;
		X = x;
		Y = y;
		X0 = x0;
		Y0 = y0;
	}

	public Direction Heading
	{
		get
		{
			if (X > X0) return Direction.RIGHT;
			if (X < X0) return Direction.LEFT;
			if (Y > Y0) return Direction.DOWN;
			if (Y < Y0) return Direction.UP;
			return Direction.RIGHT;//Only at startup. This defaults to starting to the right...
		}
	}

	public Point NextPosition(Direction d)
	{
		switch (d)
		{
			case Direction.LEFT: return new Point(Id, X - 1, Y, X, Y);
			case Direction.UP: return new Point(Id, X, Y - 1, X, Y);
			case Direction.RIGHT: return new Point(Id, X + 1, Y, X, Y);
			case Direction.DOWN: return new Point(Id, X, Y + 1, X, Y);
			default:
				throw new NotSupportedException();
		}
	}

	public void MoveTo(Point p)
	{
		X0 = X;
		Y0 = Y;
		X = p.X;
		Y = p.Y;
	}
}

public enum Direction
{
	LEFT = 0,
	UP = 1,
	RIGHT = 2,
	DOWN = 3
}