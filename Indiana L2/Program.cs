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
		int W = int.Parse(inputs[0]); // number of columns.
		int H = int.Parse(inputs[1]); // number of rows.
		var map = new List<int[]>();
		for (int i = 0; i < H; i++)
		{
			string LINE = Console.ReadLine(); // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
			map.Add(LINE.Split(' ').Select(x => int.Parse(x)).ToArray());
		}
		int EX = int.Parse(Console.ReadLine()); // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).
		var exit = new Point(EX, H-1, Direction.TOP);

		var dfs = new DFS(map);
		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int XI = int.Parse(inputs[0]);
			int YI = int.Parse(inputs[1]);
			string POSI = inputs[2];
			var indy = new Point(XI, YI, directionOf(POSI));

			int R = int.Parse(Console.ReadLine()); // the number of rocks currently in the grid.
			var rocks = new Point[R];
			for (int i = 0; i < R; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				int XR = int.Parse(inputs[0]);
				int YR = int.Parse(inputs[1]);
				string POSR = inputs[2];
				rocks[i] = new Point(XR, YR, directionOf(POSR));
			}

			//TODO: Add rocks to DFS
			Console.Error.WriteLine("Indy is at " + indy + " from the " + indy.Position + " which is tile #" + dfs.RoomTypeAt(indy));
			var path = dfs.To(indy, exit);
	
			Console.WriteLine(path);

			if (path != "WAIT")
			{
				var s = path.Split(' ');
				var pos = new Point(int.Parse(s[0]), int.Parse(s[1]), directionOf(s[2]));

				var oldType = dfs.RoomTypeAt(pos);
				dfs.Rotate(pos, 4 - (int)pos.Position);
				Console.Error.WriteLine("Storing change of tile at " + pos + " from a #" + oldType + " to a #" + dfs.RoomTypeAt(pos));
			}
			else
			{
				Console.Error.WriteLine("Indy waited");
			}
		}
	}


	private static Direction directionOf(string POS)
	{
		return (Direction)Enum.Parse(typeof(Direction), POS);
	}
}

public class DFS
{
	List<int[]> _map;

	public DFS(List<int[]> map)
	{
		_map = map;
	}

	public string To(Point from, Point to)
	{
		var path = TestPaths(from, to, 0).ToArray();
		if (path == null)
		{
			throw new NotImplementedException("Could not find a valid path");
		}
		else
		{
			Console.Error.WriteLine("Total path: " + string.Join(", ", path));
			if (path.Length == 0)
				return "WAIT";
			else
				return path[0];
		}
	}

	private string[] TestPaths(Point from, Point to, int distance)
	{
		Console.Error.WriteLine("TestPaths for " + from + " from " + from.Position);

		if (from.X == to.X && from.Y == to.Y)
			return new string[0];

		var thisRoomType = RoomTypeAt(from);
		var exitDirection = nextStepOf(thisRoomType, from.Position);
		Console.Error.WriteLine("\tExiting to " + exitDirection);
		var nextRoom = from + exitDirection;
		if (!IsInsideMap(nextRoom))
			return null;

		string[] path = null; 

		//No rotation
		if (IsValidEntry(nextRoom))
		{
			//Console.Error.WriteLine("Testing " + nextRoom + " from " + nextRoom.Position + " as tile #" + RoomTypeAt(nextRoom));
			path = TestPaths(nextRoom, to, distance + 1);
			if (path != null)
			{
				//Console.Error.WriteLine("...Unchanged path is a success: " + string.Join(", ", path.ToArray()));
				return path;
			}
		}

		var isNextRoomLocked = nextRoom == to || IsRoomLockedAt(nextRoom);
		if (!isNextRoomLocked)
		{
			//Rotate left
			path = PathWithRotation(nextRoom, 1, to, distance);
			if (path != null)
				return new[] { nextRoom + " LEFT" }.Concat(path).ToArray();

			//Rotate right
			path = PathWithRotation(nextRoom, 3, to, distance);
			if (path != null)
				return new[] { nextRoom + " RIGHT" }.Concat(path).ToArray();

			if (distance > 1)
			{
				//Rotate 180
				path = PathWithRotation(nextRoom, 2, to, distance);
				if (path != null)
					return new[] { nextRoom + " LEFT", nextRoom + " LEFT" }.Concat(path).ToArray();
			}
		}

		Console.Error.WriteLine("This path has no solution");
		return null;
	}

	private string[] PathWithRotation(Point nextRoom, int rotation, Point to, int distance)
	{
		Rotate(nextRoom, rotation);

		//Console.Error.WriteLine("Testing " + nextRoom + " from " + nextRoom.Position + " as tile #" + RoomTypeAt(nextRoom));
		var path = IsValidEntry(nextRoom)
			? TestPaths(nextRoom, to, distance)
			: null;
	
		Rotate(nextRoom, 4 - rotation);

		//if (path != null)
		//{
		//	Console.Error.WriteLine("...Path is a success: " + string.Join(", ", path.ToArray()));
		//}

		return path;
	}

	private bool IsInsideMap(Point position)
	{
		//Check if outside map
		if (position.Y < 0 || position.Y >= _map.Count)
			return false;
		if (position.X < 0 || position.X >= _map[0].Length)
			return false;
		return true;
	}

	//private IEnumerable<string> PathAfterRotate(Point from, Point to, int distance, int rotation)
	//{
	//	rotate(from, rotation);
	//	var path = Path(from, to, distance);
	//	rotate(from, 4 - rotation);//restore rotation above
	//	return path;
	//}

	public void Rotate(Point from, int numberOfTurnsToLeft)
	{
		var type = RoomTypeAt(from);
		var oldType = type;
		for (int i = 0; i < numberOfTurnsToLeft; i++)
		{
			switch (type)
			{
				case 0: type = 0; break;
				
				case 1: type = 1; break;
				
				case 2: type = 3; break;
				case 3: type = 2; break;
				
				case 4: type = 5; break;
				case 5: type = 4; break;
				
				case 6: type = 9; break;
				case 7: type = 6; break;
				case 8: type = 7; break;
				case 9: type = 8; break;
				
				case 10: type = 13; break;
				case 11: type = 10; break;
				case 12: type = 11; break;
				case 13: type = 12; break;
				
				default:
					throw new NotSupportedException();
			}
		}
		//if (numberOfTurnsToLeft > 0)
		//	Console.Error.WriteLine("Rotating tile at " + from + " from a #" + oldType + " to a #" + type);
		_map[from.Y][from.X] = type;
	}

	

	//private IEnumerable<string> Path(Point walker, Point to, int distance)
	//{
	//	var type = RoomTypeAt(walker);
	//	var direction = nextStepOf(type, walker.Position);
	//	var exitTo = walker + direction;

	//	if (!IsValidEntry(exitTo))
	//		return null;

	//	if (exitTo == to)
	//	{
	//		return new string[0];
	//	}
	//	else
	//	{
	//		return TestPaths(exitTo, to, distance + 1).ToArray();
	//	}
	//}

	public int RoomTypeAt(Point position)
	{
		return Math.Abs(RawRoomType(position));
	}

	private bool IsRoomLockedAt(Point position)
	{
		return RawRoomType(position) < 0;
	}

	private int RawRoomType(Point walker)
	{
		var type = _map[walker.Y][walker.X];
		return type;
	}

	private bool IsValidEntry(Point position)
	{
		var type = RoomTypeAt(position);
		var isValid = nextStepOf(type, position.Position) != Direction.NoEntry;
		return isValid;
	}

	private static Direction nextStepOf(int type, Direction position)
	{
		var index = type * 4 + (int)position;
		switch (index)
		{
			//type 1
			case 4: return Direction.BOTTOM;
			case 5: return Direction.BOTTOM;
			//case 6: return Direction.NoEntry;
			case 7: return Direction.BOTTOM;

			//type 2
			//case 8: return Direction.NoEntry;
			case 9: return Direction.LEFT;
			//case 10: return Direction.NoEntry;
			case 11: return Direction.RIGHT;

			//type 3
			case 12: return Direction.BOTTOM;
			//case 13: return Direction.NoEntry;
			//case 14: return Direction.NoEntry;
			//case 15: return Direction.NoEntry;
			
			//type 4
			case 16: return Direction.LEFT;
			case 17: return Direction.BOTTOM;
			//case 18: return Direction.NoEntry;
			//case 19: return Direction.Crash;

			//type 5
			case 20: return Direction.RIGHT;
			//case 21: return Direction.Crash;
			//case 22: return Direction.NoEntry;
			case 23: return Direction.BOTTOM;

			//type 6
			//case 24: return Direction.Crash;
			case 25: return Direction.LEFT;
			//case 26: return Direction.NoEntry;
			case 27: return Direction.RIGHT;

			// type 7
			case 28: 
			case 29:
				return Direction.BOTTOM;

			// type 8
			//case 32: return Direction.NoEntry;
			case 33: 
			case 35:
				return Direction.BOTTOM;

			// type 9
			case 36: 
			case 39:
				return Direction.BOTTOM;

			// type 10
			case 40: return Direction.LEFT;
			//case 43: return Direction.Crash;

			// type 11
			case 44: return Direction.RIGHT;
			//case 45: return Direction.Crash;

			// type 12
			case 49: return Direction.BOTTOM;

			// type 13
			case 55: return Direction.BOTTOM;

			default:
				Console.Error.WriteLine("Next step from " + position + " of tile #" + type + " is not defined!");
				return Direction.NoEntry;
		}
	}
}

public enum Direction
{
	TOP = 0,
	RIGHT = 1,
	BOTTOM = 2,
	LEFT = 3,

	Crash = 1000,
	NoEntry = 1001
}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }
	public Direction Position { get; set; }

	public Point(int x, int y, Direction position)
	{
		X = x;
		Y = y;
		Position = position;
	}

	public static Point operator +(Point point, Direction direction)
	{
		switch (direction)
		{
			case Direction.TOP: return new Point(point.X, point.Y - 1, Direction.BOTTOM);
			case Direction.RIGHT: return new Point(point.X + 1, point.Y, Direction.LEFT);
			case Direction.BOTTOM: return new Point(point.X, point.Y + 1, Direction.TOP);
			case Direction.LEFT: return new Point(point.X - 1, point.Y, Direction.RIGHT);
			case Direction.Crash:
			case Direction.NoEntry:
			default:
				throw new NotSupportedException();
		}
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", X, Y);
	}
}
