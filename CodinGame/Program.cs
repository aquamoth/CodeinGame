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
		int height = int.Parse(Console.ReadLine());
		int width = int.Parse(Console.ReadLine());
		int numberOfPlayers = int.Parse(Console.ReadLine());
		var players = new Point[numberOfPlayers];
		Debug("Expecting {0} players", numberOfPlayers);


		// game loop
		var dfs = new Map(width, height);
		char? lastDirection = null;
		while (true)
		{
			string north = Console.ReadLine();
			string east = Console.ReadLine();
			string south = Console.ReadLine();
			string west = Console.ReadLine();
			//Debug("{0} {1} {2} {3}", north, east, south, west);

			for (int i = 0; i < numberOfPlayers; i++)
			{
				string[] inputs = Console.ReadLine().Split(' ');
				int playerX = int.Parse(inputs[0]);
				int playerY = int.Parse(inputs[1]);
				players[i] = new Point(i, playerX, playerY, width, height);
				//TODO: Should be able to map using info on how other players move too
			}
			Debug("Players at: {0}", string.Join(", ", players.Select(p => p.ToString())));
			foreach (var player in players) dfs.Set(player, Map.PATH_UNVISITED);

			var me = players[4]; //TODO: Last?
			var opponents = players.Except(new[] { me });

			dfs.SetVisited(me, north, east, south, west);

			dfs.PrintMap(players);

			var directions = dfs.ExploreDirection(me);
			var validDirections = directions.Where(d => !hits(dfs.Move(me, Point.From(d)), opponents, dfs));

			char direction;
			if (!validDirections.Any())
			{
				Debug("No valid path to explore, so we try to backtrack one step.");
				direction = reverse(lastDirection);
				//Just a sanity-check that possibleDirections contains the reverse of lastDirection
				if (!dfs.IsValidMove(me, direction))
				{
					throw new ApplicationException("Expected " + directions + " to be one of the possible directions.");
				}
			}
			else
			{
				direction = validDirections.FirstOrDefault();
			}

			lastDirection = direction;
			Console.WriteLine(direction);
		}
	}

	private static bool hits(Point me, IEnumerable<Point> opponents, Map dfs)
	{
		var myRange = dfs.NeighboursOf(me).Concat(new[] { me.Index }).ToArray();

		var opponentPositions = opponents.Select(p => p.Index).ToArray();
		
		Debug("Checking intersection of [{0}] and [{1}]", string.Join(", ", myRange), string.Join(", ", opponentPositions));
		return myRange.Intersect(opponentPositions).Any();
	}

	private static char reverse(char? lastDirection)
	{
		switch (lastDirection)
		{
			case Map.DIRECTION_EAST: return Map.DIRECTION_WEST;
			case Map.DIRECTION_SOUTH: return Map.DIRECTION_NORTH;
			case Map.DIRECTION_WEST: return Map.DIRECTION_EAST;
			case Map.DIRECTION_NORTH: return Map.DIRECTION_SOUTH;
			default:
				throw new ApplicationException("Unable to reverse direction of: " + lastDirection ?? "<null>");
		}
	}

	public static void Debug(string format, params object[] args)
	{
		Console.Error.WriteLine(/*Timer.ElapsedMilliseconds + " ms: " +*/ string.Format(format, args));
	}
}

public class Point
{
	public int Id { get; private set; }
	public int X { get; private set; }
	public int Y { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	public int Index { get; private set; }

	public Point(int x, int y)
		: this(0, x, y, 0, 0)
	{
	}
	public Point(int x, int y, int width, int height)
		: this(0, x, y, width, height)
	{
	}
	public Point(int id, int x, int y, int width, int height)
	{
		Id = id;
		X = x;
		Y = y;
		Width = width;
		Height = height;

		if (Width > 0 && (X < 0 || X >= Width)) throw new ApplicationException("Invalid X: " + this.ToString());
		if (Height > 0 && (Y < 0 || Y >= Height)) throw new ApplicationException("Invalid Y: " + this.ToString());
		Index = Y * Width + X;
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}

	public static Point From(char direction)
	{
		switch (direction)
		{
			case Map.DIRECTION_EAST: return new Point(1, 0);
			case Map.DIRECTION_SOUTH: return new Point(0, 1);
			case Map.DIRECTION_WEST: return new Point(-1, 0);
			case Map.DIRECTION_NORTH: return new Point(0, -1);
			default:
				throw new NotSupportedException();
		}
	}
}

public class Map
{
	public const char UNKNOWN_SPACE = '?';
	public const char WALL = '#';
	public const char PATH_UNVISITED = '_';
	public const char PATH_VISITED = '*';
	public const char PATH_DEADEND = '¤';

	public const char DIRECTION_NORTH = 'C';
	public const char DIRECTION_EAST = 'A';
	public const char DIRECTION_SOUTH = 'D';
	public const char DIRECTION_WEST = 'E';
	public const char DIRECTION_WAIT = 'B';

	private char[] _array;

	public IEnumerable<char> Array { get { return _array; } }
	public int Length { get { return _array.Length; } }

	public int Width { get; private set; }
	public int Height { get; private set; }

	public Map(int width, int height)
	{
		Width = width;
		Height = height;
		_array = Enumerable.Repeat<char>(UNKNOWN_SPACE, width * height).ToArray();
	}


	public char this[int index]
	{
		get
		{
			if (index < 0 || index > this.Length)
				throw new ArgumentException("Invalid index");
			return _array[index];
		}
		set
		{
			if (index < 0 || index > this.Length)
				throw new ArgumentException("Invalid index");
			_array[index] = value;
		}
	}

	internal void SetVisited(Point me, string north, string east, string south, string west)
	{
		Set(me, PATH_VISITED);
		Set(Move(me, DIRECTION_NORTH), north);
		Set(Move(me, DIRECTION_EAST), east);
		Set(Move(me, DIRECTION_SOUTH), south);
		Set(Move(me, DIRECTION_WEST), west);
	}

	public void Set(Point point, string type)
	{
		Set(point, Char.Parse(type));
	}
	public void Set(Point point, char type)
	{
		switch (type)
		{
			case WALL:
				if (this[point.Index] == PATH_UNVISITED || this[point.Index] == PATH_VISITED) 
					throw new ApplicationException("Tried to change room into wall");
				break;
			case PATH_UNVISITED:
				if (this[point.Index] == WALL) 
					throw new ApplicationException("Tried to change wall into unvisited");
				if (this[point.Index] != UNKNOWN_SPACE) 
					return;
				break;
			case PATH_VISITED:
				if (this[point.Index] == WALL) 
					throw new ApplicationException("Tried to change wall into visited");
				if (this[point.Index] == PATH_DEADEND) 
					return;
				break;
			case PATH_DEADEND:
				break;
			default:
				throw new NotSupportedException("Unknown area type: " + type);
		}

		this[point.Index] = type;
	}

	internal IEnumerable<char> ExploreDirection(Point me)
	{
		var possibleDirections = directionsTo(me, PATH_UNVISITED);
		if (possibleDirections.Any())
		{
			return possibleDirections;
		}
		else
		{
			Set(me, PATH_DEADEND);
			possibleDirections = directionsTo(me, PATH_VISITED);
			return possibleDirections;
		}
	}

	private IEnumerable<char> directionsTo(Point me, char TOKEN)
	{
		return new[] { DIRECTION_EAST, DIRECTION_SOUTH, DIRECTION_WEST, DIRECTION_NORTH }
			.Select(direction => new { direction, next = Move(me, Point.From(direction)) })
			.Where(x => this[x.next.Index] == TOKEN)
			.Select(x => x.direction);
	}

	public IEnumerable<int> NeighboursOf(Point me)
	{
		yield return Move(me, DIRECTION_NORTH).Index;
		yield return Move(me, DIRECTION_EAST).Index;
		yield return Move(me, DIRECTION_SOUTH).Index;
		yield return Move(me, DIRECTION_WEST).Index;
	}


	internal void PrintMap(IEnumerable<Point> players)
	{
		for (var y = 0; y < Height; y++)
		{
			for (var x = 0; x < Width; x++)
			{
				var token = players.Where(p => p.X == x && p.Y == y).Select(p => (char?)char.Parse(p.Id.ToString())).FirstOrDefault() ?? _array[y * Width + x];
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("");
		}
	}

	internal bool IsValidMove(Point me, char? direction)
	{
		if (!direction.HasValue) return true;
		var next = Move(me, Point.From(direction.Value));
		return this[next.Index] != WALL;
	}

	internal Point Move(Point point, char direction)
	{
		return Move(point, Point.From(direction));
	}
	internal Point Move(Point point, Point relative)
	{
		return new Point(
			point.Id,
			(point.X + relative.X + point.Width) % point.Width,
			(point.Y + relative.Y + point.Height) % point.Height,
			point.Width,
			point.Height
		);
	}
}

