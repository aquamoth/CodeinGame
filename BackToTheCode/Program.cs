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
	const char CELL_NEUTRAL = '.';
	const char CELL_PLAYER = '0';
	const string CELL_WALL = "#";
	const int MAP_HEIGHT = 20;
	const int MAP_WIDTH = 35;

	static void Main(string[] args)
	{
		Random r = new Random();

		string[] inputs;
		int opponentCount = int.Parse(Console.ReadLine()); // Opponent count

		// game loop
		while (true)
		{
			int gameRound = int.Parse(Console.ReadLine());
			inputs = Console.ReadLine().Split(' ');
			var player = new Combatant { X = int.Parse(inputs[0]), Y = int.Parse(inputs[1]), BackInTimeLeft = int.Parse(inputs[2]) };

			var opponents = new Combatant[opponentCount];
			for (int i = 0; i < opponentCount; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				opponents[i] = new Combatant { X = int.Parse(inputs[0]), Y = int.Parse(inputs[1]), BackInTimeLeft = int.Parse(inputs[2]) };
			}

			var map = new string[MAP_HEIGHT+2];
			map[0] = string.Join("", Enumerable.Repeat(CELL_WALL, MAP_WIDTH+2).ToArray());
			for (int i = 0; i < MAP_HEIGHT; i++)
			{
				map[i+1] = CELL_WALL +  Console.ReadLine() + CELL_WALL; // One line of the map ('.' = free, '0' = you, otherwise the id of the opponent)
			}
			map[MAP_HEIGHT+1] = string.Join("", Enumerable.Repeat(CELL_WALL, MAP_WIDTH+2).ToArray());

			printMap(map);

			//Prefer any neutral cell
			Console.Error.WriteLine("I'm at " + player);
			var directions = directionsTo(map, player, CELL_NEUTRAL).ToArray();
			Console.Error.WriteLine("Neutral cells at: " + string.Join(", ", (directions.Select(d => d.ToString()).ToArray())));
			if (!directions.Any())
			{
				Console.Error.WriteLine("No neutrals found");
				//No neutral, so prefer an opponents cell, so we cross paths
				var visited = directionsTo(map, player, CELL_PLAYER);
				var walls = directionsTo(map, player, CELL_WALL[0]);
				directions = new Direction[] { Direction.TOP, Direction.RIGHT, Direction.BOTTOM, Direction.LEFT }.Except(walls).Except(visited).ToArray();
			}
			if (directions.Any())
			{
				//Go in one of the available directions
				var nextDirection = (Direction)r.Next(directions.Count());
				var newLocation = player + nextDirection;
				Console.Error.WriteLine("Walking " + nextDirection.ToString() + " to " + newLocation);
				Console.WriteLine(newLocation.ToString());
			}
			else
			{
				//I'm inside my own area. Get out!
				var headTo = r.Next(opponents.Count());
				var point = new Point(opponents[headTo].X, opponents[headTo].Y);
				Console.WriteLine(point); 
			}

			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");

			//Console.WriteLine("17 10"); // action: "x y" to move or "BACK rounds" to go back in time
		}
	}

	private static IEnumerable<Direction> directionsTo(string[] map, Point position, char searchedToken)
	{
		var tokens = tokensAround(map, position).ToArray();
		for (var direction = 0; direction < 4; direction++)
		{
			if (tokens[direction] == searchedToken)
				yield return (Direction)direction;
		}
	}

	private static char[] tokensAround(string[] map, Point position)
	{
		return new[]{
			get(map, position + Direction.TOP),
			get(map, position + Direction.RIGHT),
			get(map, position + Direction.BOTTOM),
			get(map, position + Direction.LEFT)
		};
	}

	private static char get(string[] map, Point position)
	{
		//Console.Error.WriteLine("Map is " + map.Length + " x " + map[0].Length);
		//Console.Error.Write("Token at " + position + " is: ");
		var t = map[position.Y + 1][position.X + 1];
		//Console.Error.WriteLine(t);
		return t;
	}

	private static void printMap(string[] travelMap)
	{
		foreach (var row in travelMap)
		{
			Console.Error.WriteLine(row);
		}
	}
}


class Combatant : Point
{
	public int BackInTimeLeft { get; set; }
	public Combatant() : base(0, 0) 
	{ 
	}

	public override string ToString()
	{
		return base.ToString() + (BackInTimeLeft > 0 ? " - I go back in time" : " - this is it!");
	}

}

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public int Angle
	{
		get
		{
			return (int)Math.Round(Math.Atan((double)X / Y) * 180 / Math.PI);
		}
	}

	public static Point operator +(Point point, Direction direction)
	{
		switch (direction)
		{
			case Direction.TOP: return new Point(point.X, point.Y - 1);
			case Direction.RIGHT: return new Point(point.X + 1, point.Y);
			case Direction.BOTTOM: return new Point(point.X, point.Y + 1);
			case Direction.LEFT: return new Point(point.X - 1, point.Y);
			//case Direction.Crash:
			default:
				throw new NotSupportedException();
		}
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", X, Y);
	}
}

public enum Direction
{
	TOP = 0,
	RIGHT = 1,
	BOTTOM = 2,
	LEFT = 3,
}