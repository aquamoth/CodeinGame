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
		int width = int.Parse(Console.ReadLine());
		int height = int.Parse(Console.ReadLine());
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
				players[i] = new Point(i, playerX, playerY);
				//TODO: Should be able to map using info on how other players move too
			}
			Debug("Players at: {0}", string.Join(", ", players.Select(p => p.ToString())));
			foreach (var p in players) dfs.Set(dfs.IndexOf(p), Map.PATH_UNVISITED);

			var me = players[4]; //TODO: Last?
			dfs.SetVisited(me, north, east, south, west);

			dfs.PrintMap(players);

			var direction = dfs.ExploreDirection(me);

			if (dfs.Array.ToArray()[dfs.IndexOf(me) - dfs.Width] == Map.PATH_DEADEND)
				direction = Map.DIRECTION_WAIT;
			else
			{
				if (direction == null)
				{
					direction = reverse(lastDirection);
					//Just a sanity-check that possibleDirections contains the reverse of lastDirection
					if (!dfs.IsValid(me, direction))
					{
						throw new ApplicationException("Expected " + direction + " to be one of the possible directions.");
					}

				}
			}

			lastDirection = direction;
			Console.WriteLine(direction);
		}
	}

	private static char? reverse(char? lastDirection)
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
	//public int? this[Point p]
	//{
	//	get { return this[IndexOf(p.X, p.Y)]; }
	//	set { this[IndexOf(p.X, p.Y)] = value; }
	//}

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
		Set(index - Width, north);
		Set(index + 1, east);
		Set(index + Width, south);
		Set(index - 1, west);
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
				if (this[index] == WALL) throw new ApplicationException("Tried to change wall into exit");
				if (this[index] != UNKNOWN_SPACE) return;
				break;
			case PATH_VISITED:
				if (this[index] == WALL) throw new ApplicationException("Tried to change wall into exit");
				if (this[index] == PATH_DEADEND) return;
				break;
			case PATH_DEADEND:
				break;
			default:
				throw new NotSupportedException("Unknown area type: " + type);
		}

		this[index] = type;
	}

	internal char? ExploreDirection(Point me)
	{
		var possibleDirections = directionsTo(me, PATH_UNVISITED);
		if (possibleDirections.Any())
		{
			return possibleDirections.First();
		}
		else
		{
			Set(IndexOf(me), PATH_DEADEND);
			possibleDirections = directionsTo(me, PATH_VISITED);
			if (possibleDirections.Count() == 1)
			{
				return possibleDirections.First();
			}
			else
			{
				return null;
			}
		}
	}

	private IEnumerable<char> directionsTo(Point me, char TOKEN)
	{
		var index = IndexOf(me);
		if (this[index - Width] == TOKEN) yield return DIRECTION_NORTH;
		if (this[index + 1] == TOKEN) yield return DIRECTION_EAST;
		if (this[index + Width] == TOKEN) yield return DIRECTION_SOUTH;
		if (this[index - 1] == TOKEN) yield return DIRECTION_WEST;
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

//public class Map
//{
//	public const int TILE_IS_WALL = 29;

//	readonly bool?[] _array;
//	public IEnumerable<bool?> Array { get { return _array; } }

//	public int Length { get { return _array.Length; } }
//	public int Width { get; private set; }
//	public int Height { get; private set; }

//	public Map(int width, int height)
//	{
//		Width = width;
//		Height = height;
//		_array = new bool?[width * height];
//	}

//	//public Map(bool?[] array, int width)
//	//{
//	//	Width = width;
//	//	Height = array.Length / width;
//	//	_array = array;
//	//}

//	public bool? Get(int x, int y)
//	{
//		if (x < 0 || x >= Width)
//			return false;
//		if (y < 0 || y >= Height)
//			return false;
//		return _array[IndexOf(x, y)];
//	}

//	public void Put(int x, int y, bool isRoom)
//	{
//		this[IndexOf(x, y)] = isRoom;
//	}


//	//public bool IsFree(Point p) { return !Get(p.X, p.Y).HasValue; }
//}

