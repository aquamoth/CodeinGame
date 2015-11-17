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
		var map = new List<string[]>();
		for (int i = 0; i < H; i++)
		{
			string LINE = Console.ReadLine(); // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
			map.Add(LINE.Split(' '));
		}
		int EX = int.Parse(Console.ReadLine()); // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).

		//var tree = buildTree(map);

		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int XI = int.Parse(inputs[0]);
			int YI = int.Parse(inputs[1]);
			string POS = inputs[2];

			var type = int.Parse(map[YI][XI]);
			var direction = nextStepOf(type, POS);
			
			var expectedPosition = new Point(XI, YI) + direction;
			Console.WriteLine(expectedPosition.ToString());

			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");

			//Console.WriteLine("0 0"); // One line containing the X Y coordinates of the room in which you believe Indy will be on the next turn.
		}
	}

	private static Direction nextStepOf(int type, string POS)
	{
		var posIndex = directionIndexOf(POS);
		var index = type * 4 + (int)posIndex;
		switch (index)
		{
			case 4: //type 1, top
			case 5:
			case 7:
				return Direction.BOTTOM;

			case 9: //type 2, right
				return Direction.LEFT;
			case 11:
				return Direction.RIGHT;
	
			case 12: //type 3, top
				return Direction.BOTTOM;

			case 16: //type 4, top
				return Direction.LEFT;
			case 17: 
				return Direction.BOTTOM;
			case 19: 
				return Direction.Crash;

			case 20: //type 5, top
				return Direction.RIGHT;
			case 21:
				return Direction.Crash;
			case 23:
				return Direction.BOTTOM;

			case 24: //type 6, top
				return Direction.Crash;
			case 25:
				return Direction.LEFT;
			case 27:
				return Direction.RIGHT;

			case 28: // type 7, top
			case 29:
				return Direction.BOTTOM;

			case 33: // type 8, right
			case 35:
				return Direction.BOTTOM;

			case 36: // type 9
			case 39:
				return Direction.BOTTOM;

			case 40: // type 10
				return Direction.LEFT;
			case 43:
				return Direction.Crash;

			case 44: // type 11
				return Direction.RIGHT;
			case 45:
				return Direction.Crash;

			case 49: // type 12, right
				return Direction.BOTTOM;

			case 55: // type 13, left
				return Direction.BOTTOM;
	
			default:
				throw new NotSupportedException();
		}
	}

	private static Direction directionIndexOf(string POS)
	{
		return (Direction)Enum.Parse(typeof(Direction), POS);
		//switch (POS)
		//{
		//	case "TOP": return 0;
		//	case "RIGHT": return 1;
		//	case "BOTTOM": return 2;//Can never happen
		//	case "LEFT": return 3;
		//	default:
		//		throw new NotSupportedException();
		//}
	}

	//public static Node[] buildTree(List<string[]> map)
	//{
	//	var nodes = new List<Node>();

	//	for (int y = 0; y < map.Count; y++)
	//	{
	//		var row = map[y];
	//		for (int x = 0; x < row.Length; x++)
	//		{
	//			var type = int.Parse(row[x]);
	//			var node = new Node(x, y);
	//			switch (type)
	//			{
	//				case 0:
	//					node = null;
	//					break;
	//				case 1:
	//					node.Connect(x, y - 1, x, y + 1);
	//					node.Connect(x - 1, y, x, y + 1);
	//					node.Connect(x + 1, y - 1, x, y + 1);
	//					break;
	//				case 2:
	//					node.Connect(x - 1, y, x + 1, y);
	//					node.Connect(x + 1, y, x - 1, y);
	//					break;
	//				case 3:
	//					node.Connect(x, y - 1, x, y + 1);
	//					break;
	//				case 4:
	//					node.Connect(x, y - 1, x - 1, y);
	//					node.Crash(x - 1, y);
	//					node.Connect(x + 1, y, x, y + 1);
	//					break;
	//				case 10:
	//					node.Connect(x, y - 1, x - 1, y);
	//					node.Crash(x - 1, y);
	//					break;
	//				case 11:
	//					node.Connect(x, y - 1, x + 1, y);
	//					node.Crash(x + 1, y);
	//					break;
	//				case 12:
	//					node.Connect(x + 1, y, x, y + 1);
	//					break;
	//				case 13:
	//					node.Connect(x - 1, y, x, y + 1);
	//					break;
	//				default:
	//					throw new NotImplementedException();
	//			}

	//			if (node != null)
	//				nodes.Add(node);
	//		}
	//	}

	//	return nodes.ToArray();
	//}
}

public enum Direction
{
	TOP = 0,
	RIGHT = 1,
	BOTTOM = 2,
	LEFT = 3,

	Crash = 1000,
}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static Point operator+(Point point, Direction direction)
	{
		switch (direction)
		{
			case Direction.TOP: return new Point(point.X, point.Y - 1);
			case Direction.RIGHT: return new Point(point.X + 1, point.Y);
			case Direction.BOTTOM: return new Point(point.X, point.Y + 1);
			case Direction.LEFT: return new Point(point.X - 1, point.Y);
			case Direction.Crash:
			default:
				throw new NotSupportedException();
		}
	}
	
	public override string ToString()
	{
		return string.Format("{0} {1}", X, Y);
	}
}

//public class Node
//{
//	public Node(int x, int y)
//	{
//		Position = new Point(x, y);
//		Connections = new List<Tuple<Point, Point>>();
//	}

//	public Point Position { get; private set; }
//	public List<Tuple<Point, Point>> Connections { get; private set; }

//	//public string Name { get; private set; }
//	//public Node[] Neighbours { get; set; }
//	//public string ShortestPath { get; set; }
//	//public int Distance { get; set; }
//	//public bool Visited { get; set; }

//	internal void Connect(int x1, int y1, int x2, int y2)
//	{
//		this.Connections.Add(new Tuple<Point, Point>(new Point(x1, y1), new Point(x2, y2)));
//	}

//	internal void Crash(int x, int y)
//	{
//		this.Connections.Add(new Tuple<Point, Point>(new Point(x, y), null));
//	}
//}
