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
	public const char PASSIVE_NODE = '#';

	static void Main(string[] args)
	{
		var map = new List<string>();

		var nodes = new List<Node>();

		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int width = int.Parse(inputs[0]); // width of the firewall grid
		int height = int.Parse(inputs[1]); // height of the firewall grid
		for (int y = 0; y < height; y++)
		{
			var line = Console.ReadLine();
			map.Add(line);
			for (int x = 0; x < width; x++)
			{
				if (line[x] == '@')
					nodes.Add(new Node(x, y, false));
				else if (line[x] == PASSIVE_NODE)
					nodes.Add(new Node(x, y, true));
			}
		}

		var voxdei = new VoxDei(width, height, nodes, map.ToArray());

		Queue<Point> solution = null;
		// game loop
		//var currentRound = 0;
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int rounds = int.Parse(inputs[0]); // number of rounds left before the end of the game
			int bombs = int.Parse(inputs[1]); // number of bombs left

			if (solution == null)
				solution = new Queue<Point>(voxdei.Solve(bombs));

			if (!solution.Any())
				Console.WriteLine("WAIT");
			else
			{
				var bomb = solution.Dequeue();
				Console.WriteLine(bomb);
			}

			//currentRound++;
		}

	}
}

class VoxDei
{
	int _width, _height;
	List<Node> _nodes;
	//List<MapPoint> _map;
	string[] _map;

	public VoxDei(int width, int height, List<Node> nodes, string[] map)
	{
		_width = width;
		_height = height;
		_nodes = nodes;
		_map = map;
	}

	public Point[] Solve(int maxDepth)
	{
		//_map = createMap();

		var nodesLeft = _nodes.Where(node=>!node.IsPassive).ToList();
		return solve(nodesLeft, maxDepth);
	}

	Point[] solve(List<Node> nodesLeft, int maxDepth)
	{
		if (nodesLeft.Count == 0)
			return new Point[0];

		if (maxDepth == 0)
			return null;

		var nextNode = nodesLeft.First();
		foreach (var p in kills(nextNode))
		{
			var unaffectedNodes = nodesLeft.Where(node => !kills(node).Contains(p)).ToList();//Any(point => point == p)
			var points = solve(unaffectedNodes, maxDepth - 1);
			if (points != null)
			{
				return new[] { p }.Concat(points).ToArray();
			}
		}

		throw new ApplicationException("Unable to find a kill solution");
	}

	private IEnumerable<Point> kills(Node node)
	{
		var minX = Math.Max(0, node.X - 3);
		for (int x = node.X - 1; x >= minX; x--)
		{
			if (_map[node.Y][x] == Player.PASSIVE_NODE)
				break;
			yield return new Point { X = x, Y = node.Y };
		}

		var maxX = Math.Min(_width-1, node.X + 3);
		for (int x = node.X+1; x <= maxX; x++)
		{
			if (_map[node.Y][x] == Player.PASSIVE_NODE)
				break;
			yield return new Point { X = x, Y = node.Y };
		}

		var minY = Math.Max(0, node.Y - 3);
		for (int y = node.Y - 1; y >= minY; y--)
		{
			if (_map[y][node.X] == Player.PASSIVE_NODE)
				break;
			yield return new Point { X = node.X, Y = y };
		}

		var maxY = Math.Min(_height-1, node.Y + 3);
		for (int y = node.Y + 1; y <= maxY; y++)
		{
			if (_map[y][node.X] == Player.PASSIVE_NODE)
				break;
			yield return new Point { X = node.X, Y = y };
		}
	}


	//private List<MapPoint> createMap()
	//{
	//	var map = new List<MapPoint>();
	//	//for (int y = 0; y < _height; y++)
	//	//{
	//	//	for (int x = 0; x < _width; x++)
	//	//	{
	//	//		var nodesInRange = _nodes.Any(node => node.X == x && node.Y == y)
	//	//			? new Node[0]
	//	//			: _nodes
	//	//				.Where(node => !node.IsPassive)
	//	//				.Where(node => node.InRangeOf(x, y))
	//	//				.ToArray();

	//	//		map.Add(new MapPoint(x, y, nodesInRange));
	//	//	}
	//	//}

	//	//map.Sort((a, b) => b.NodesInRange.Length.CompareTo(a.NodesInRange.Length));
	//	return map;
	//}
}

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public override string ToString()
	{
		return X + " " + Y;
	}

	public override bool Equals(object obj)
	{
		var p = obj as Point;
		if (p == null)
			return false;
		return this.X.Equals(p.X) && this.Y.Equals(p.Y);
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}
}

//class MapPoint : Point
//{
//	public Node[] NodesInRange { get; set; }

//	public MapPoint(int x, int y, Node[] nodesInRange)
//	{
//		X = x;
//		Y = y;
//		NodesInRange = nodesInRange;
//	}
//}

class Node : Point
{
	public bool IsPassive { get; set; }

	public bool InRangeOf(int x, int y)
	{
		//TODO: Check if shielded by passive node
		if (X == x)
			return Math.Abs(Y - y) <= 3;
		else if (Y == y)
			return Math.Abs(X - x) <= 3;
		else 
			return false;
	}

	public Node(int x, int y, bool isPassive)
	{
		X = x;
		Y = y;
		IsPassive = isPassive;
	}

	public IEnumerable<Point> KilledFrom(List<Node> _nodes)
	{
		throw new NotImplementedException();
	}
}