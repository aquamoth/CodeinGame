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
				players[i] = new Point(i, playerX - 1, playerY - 1);
				//TODO: Should be able to map using info on how other players move too
			}
			Debug("Players at: {0}", string.Join(", ", players.Select(p => p.ToString())));
			foreach (var p in players) dfs.Set(dfs.IndexOf(p), Map.PATH_UNVISITED);

			var me = players[4]; //TODO: Last?
			var opponents = players.Except(new[] { me });

			dfs.SetVisited(me, north, east, south, west);

			dfs.PrintMap(players);

			var directions = dfs.ExploreDirection(me);
			var validDirections = directions.Where(d => !hits(Point.From(me, d), opponents, dfs));

			char direction;
			if (!validDirections.Any())
			{
				Debug("No valid path to explore, so we try to backtrack one step.");
				direction = reverse(lastDirection);
				//Just a sanity-check that possibleDirections contains the reverse of lastDirection
				if (!dfs.IsValid(me, direction))
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
		var index = dfs.IndexOf(me);
		var myRange = dfs.NeighboursOf(me).Concat(new[] { index }).ToArray();

		var opponentPositions = opponents.Select(p => dfs.IndexOf(p)).ToArray();
		
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
	public int Id { get; set; }
	public int X { get; private set; }
	public int Y { get; private set; }

	public Point(int id, int x, int y)
	{
		Id = id;
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}

	internal static Point From(Point me, char direction)
	{
		switch (direction)
		{
			case Map.DIRECTION_EAST: return new Point(me.Id, me.X + 1, me.Y);
			case Map.DIRECTION_SOUTH: return new Point(me.Id, me.X, me.Y + 1);
			case Map.DIRECTION_WEST: return new Point(me.Id, me.X - 1, me.Y);
			case Map.DIRECTION_NORTH: return new Point(me.Id, me.X, me.Y - 1);
			case Map.DIRECTION_WAIT: return new Point(me.Id, me.X, me.Y);
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
				return UNKNOWN_SPACE;
			return _array[index];
		}
		set
		{
			if (index < 0 || index > this.Length)
				return;
			_array[index] = value;
		}
	}

	public int IndexOf(int x, int y)
	{
		if (x < 0 || x >= this.Width) return -1;
		if (y < 0 || y >= this.Height) return -1;
		return y * this.Width + x;
	}
	public int IndexOf(Point p) { return IndexOf(p.X, p.Y); }

	public bool IsVisited(Point p) { return this[IndexOf(p)] == PATH_VISITED; }

	internal void SetVisited(Point me, string north, string east, string south, string west)
	{
		var index = IndexOf(me);
		Set(index, PATH_VISITED);
		if (me.Y > 0) Set(index - Width, north);
		if (me.X < Width) Set(index + 1, east);
		if (me.Y < Height) Set(index + Width, south);
		if (me.X > 0) Set(index - 1, west);
	}

	public void Set(int index, string type)
	{
		Set(index, Char.Parse(type));
	}
	public void Set(int index, char type)
	{
		switch (type)
		{
			case WALL:
				if (this[index] == PATH_UNVISITED || this[index] == PATH_VISITED) throw new ApplicationException("Tried to change room into wall");
				break;
			case PATH_UNVISITED:
				if (this[index] == WALL) throw new ApplicationException("Tried to change wall into unvisited");
				if (this[index] != UNKNOWN_SPACE) return;
				break;
			case PATH_VISITED:
				if (this[index] == WALL) throw new ApplicationException("Tried to change wall into visited");
				if (this[index] == PATH_DEADEND) return;
				break;
			case PATH_DEADEND:
				break;
			default:
				throw new NotSupportedException("Unknown area type: " + type);
		}

		this[index] = type;
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
			Set(IndexOf(me), PATH_DEADEND);
			possibleDirections = directionsTo(me, PATH_VISITED);
			return possibleDirections;
		}
	}

	private IEnumerable<char> directionsTo(Point me, char TOKEN)
	{
		var index = IndexOf(me);
		if (me.Y > 0 && this[index - Width] == TOKEN) yield return DIRECTION_NORTH;
		if (me.X < Width && this[index + 1] == TOKEN) yield return DIRECTION_EAST;
		if (me.Y < Height && this[index + Width] == TOKEN) yield return DIRECTION_SOUTH;
		if (me.X > 0 && this[index - 1] == TOKEN) yield return DIRECTION_WEST;
	}

	public IEnumerable<int> NeighboursOf(Point me)
	{
		var index = IndexOf(me);
		if (me.Y > 0) yield return index - Width;
		if (me.X < Width) yield return index + 1;
		if (me.Y < Height) yield return index + Width;
		if (me.X > 0) yield return index - 1;
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

	internal bool IsValid(Point me, char? direction)
	{
		if (!direction.HasValue) return true;
		switch (direction.Value)
		{
			case DIRECTION_NORTH: return this[IndexOf(me) - Width] != WALL;
			case DIRECTION_EAST: return this[IndexOf(me) + 1] != WALL;
			case DIRECTION_SOUTH: return this[IndexOf(me) + Width] != WALL;
			case DIRECTION_WEST: return this[IndexOf(me) - 1] != WALL;
			default:
				throw new NotSupportedException();
		}
	}
}

