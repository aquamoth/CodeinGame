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
		//test();

		var map = new Map(30, 20);
		Point[] players = null;

		// game loop
		while (true)
		{
			var inputs = Console.ReadLine().Split(' ');
			var numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
			int myPlayerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

			var positions = readPositionsFromConsole(numberOfPlayers);
			var firstStep = players == null;

			if (firstStep)
			{
				players = positions;
				foreach (var player in players.Where(p => p.X >= 0))
				{
					//Console.Error.WriteLine("Marking tail on map for {0}: ({1}, {2})" , player.Id, player.X0, player.Y0);
					map.Put(player.X0, player.Y0, player.Id);
				}
			}
			else
			{ 
				for (var i = 0; i < positions.Length; i++)
				{
					if (players[i].IsAlive && positions[i].X == -1)
					{
						Console.Error.WriteLine("Player #{0} died and is removed from the map", i);
						map.RemoveAll(i);
					}
					players[i].MoveTo(positions[i]);
				}
			}

			foreach (var player in players.Where(p => p.X >= 0))
			{
				//Console.Error.WriteLine("Marking player on map: " + player);
				map.Put(player.X, player.Y, player.Id);
			}

			//printMap(map);

			var heading = selectNextHeading(map, players[myPlayerNumber], firstStep);
			Console.WriteLine(heading);
		}
	}

	private static void test()
	{
		var mapString =	@"
........*.
........*.
******.***
........*.
.*......*.
*********.";
		var map = new Map(mapString.Where(c => new[] { '.', '*' }.Contains(c)).Select(c => c == '.' ? null : (int?)29).ToArray(), 10);
		printMap(map);

		var APs = new ArticulationPoints(map, new Point(0, 1, 4, 0, 0), true).Find();
		
	}

	private static Direction selectNextHeading(Map map, Point me, bool firstStep)
	{
		var AP = new ArticulationPoints(map, me, firstStep);

		var apMap = AP.Find();
		printMap(apMap.Select((b, index) => b ? '!' : map[index].HasValue ? '#' : '.').ToArray(), map.Width);
		var articulationPoints = apMap
			.Select((yes, index) => yes ? (int?)index : null)
			.Where(x => x.HasValue)
			.Select(x => x.Value)
			.ToArray();
		if (articulationPoints.Any())
		{
			Console.Error.WriteLine("Identified articulation points at: {0}", string.Join(", ", articulationPoints.Select(idx => Point.From(idx, map.Width))));
		}
		//var articulationPoints = new int[0];

		if (apMap[map.IndexOf(me.X, me.Y)])
		{
			Console.Error.WriteLine("CURRENTLY ON AN AP. CHOOSE DIRECTION WISELY!");
		}


		var allValidMoves = validMoves(me, map, firstStep);
		Console.Error.WriteLine("Valid moves: " + string.Join(", ", allValidMoves));
		var possibleMoves = allValidMoves
			.Where(m => !articulationPoints.Contains(map.IndexOf(m.X, m.Y)));
		Console.Error.WriteLine("Moves-APs: " + string.Join(", ", possibleMoves));
		if (!possibleMoves.Any())
			possibleMoves = allValidMoves;

		
		var scoredMoves = possibleMoves
			.Select(move => new { Heading = move.Heading, Score = score(map, move) })
			.OrderByDescending(x => x.Score)
			.ToArray();

		Console.Error.WriteLine("Scored moves: " + string.Join(", ", scoredMoves.Select(x => string.Format("{0}={1}", x.Heading, x.Score))));

		return scoredMoves
			.Select(p => p.Heading)
			.DefaultIfEmpty(Direction.RIGHT) //If default we are dead anyway and just need to write something to die happily
			.FirstOrDefault();
	}

	private static int score(Map map, Point p)
	{
		return wallsAt(map, p);
	}

	private static int wallsAt(Map map, Point p)
	{
		var walls = 0;
		if (p.X == 0 || map[p.NextPosition(Direction.LEFT)] != null) walls++;
		if (p.X == map.Width - 1 || map[p.NextPosition(Direction.RIGHT)] != null) walls++;
		if (p.Y == 0 || map[p.NextPosition(Direction.UP)] != null) walls++;
		if (p.Y == map.Height - 1 || map[p.NextPosition(Direction.DOWN)] != null) walls++;
		return walls;
	}

	public static IEnumerable<Point> validMoves(Point me, Map map, bool firstStep)
	{
		Point p;

		p = me.NextPosition(me.Heading);
		if (map.IsFree(p))
			yield return p;

		p = me.NextPosition(turn(me.Heading, 1));
		if (map.IsFree(p))
			yield return p;

		if (firstStep)
		{
			p = me.NextPosition(turn(me.Heading, 2));
			if (map.IsFree(p))
				yield return p;
		}

		p = me.NextPosition(turn(me.Heading, 3));
		if (map.IsFree(p))
			yield return p;
	}

	#region Helpers

	#region Map methods

	public static void printMap(Map map)
	{
		printMap(map.Array.Select(value => value.HasValue ? (char)('@' + value.Value) : '.').ToArray(), map.Width);
	}

	public static void printMap(char[] map, int width)
	{
		var height = map.Length / width;
		Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		for (var y = 0; y < height; y++)
		{
			Console.Error.Write('¤');
			for (var x = 0; x < width; x++)
			{
				var token = map[y * width + x];
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("¤");
		}
		Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		Console.Error.WriteLine();
	}

	#endregion Map methods

	private static Direction turn(Direction direction, int stepsToRight)
	{
		return (Direction)(((int)direction + stepsToRight) % 4);
	}

	private static Point[] readPositionsFromConsole(int N)
	{
		var playersTurn = Enumerable
						 .Range(0, N)
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

	#endregion helpers
}

#region Classes and Enums

public class Map
{
	public const int ILLEGAL_INDEX = 29;

	readonly int?[] _array;
	public IEnumerable<int?> Array { get { return _array; } }
	public int Length { get { return _array.Length; } }

	public int Width { get; private set; }
	public int Height { get; private set; }
	
	public Map(int width, int height)
	{
		Width = width;
		Height = height;
		_array = new int?[width * height];
	}

	public Map(int?[] array, int width)
	{
		Width = width;
		Height = array.Length / width;
		_array = array;
	}

	public int? this[int index] 
	{ 
		get {
			if (index < 0 || index > this.Length)
				return ILLEGAL_INDEX;
			return _array[index]; 
		} 
		set {
			if (index < 0 || index > this.Length)
				return;
			_array[index] = value; 
		} 
	}

	public int? this[Point p]
	{
		get { return this[IndexOf(p.X, p.Y)]; }
		set { this[IndexOf(p.X, p.Y)] = value; }
	}

	public int? Get(int x, int y)
	{
		if (x < 0 || x >= Width)
			return ILLEGAL_INDEX;
		if (y < 0 || y >= Height)
			return ILLEGAL_INDEX;
		return _array[IndexOf(x, y)];
	}

	public void Put(int x, int y, int token)
	{
		this[IndexOf(x, y)] = token;
	}

	public int IndexOf(int x , int y) {
		if (x < 0 || x >= this.Width) return -1;
		if (y < 0 || y >= this.Height) return -1;
		return y * this.Width + x; 
	}
	public int IndexOf(Point p) { return IndexOf(p.X, p.Y); }

	public bool IsFree(Point p) { return !Get(p.X, p.Y).HasValue; }

	internal void RemoveAll(int token)
	{
		for (var i = 0; i < this.Length; i++)
		{
			if (_array[i].HasValue && _array[i].Value == token)
				_array[i] = null;
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
	public bool IsAlive { get { return X >= 0; } }

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

	//public int MapIndex { get { return this.X + this.Y * Player.MAP_WIDTH; } }

	public override string ToString()
	{
		return string.Format("#{0} ({1}, {2})", this.Id, this.X, this.Y);
	}

	internal static Point From(int idx, int width)
	{
		return new Point(0, idx % width, idx / width, 0, 0);
	}
}

public enum Direction
{
	LEFT = 0,
	UP = 1,
	RIGHT = 2,
	DOWN = 3
}

public class ArticulationPoints
{
	int time;

	public Vertex[] vertexes { get; private set; }

	public ArticulationPoints(Map map, Point startingPoint, bool firstStep)
	{
		vertexes = Enumerable.Repeat(0, map.Length).Select((x, index) => new Vertex(index)).ToArray();
		Traverse(map, vertexes, startingPoint, firstStep);
		//printApMap(map, vertexes, new Point(0, 100, 100, 0, 0));
	}

	public bool[] Find()
	{
		return vertexes.Select(v => v.IsArticulationPoint).ToArray();
	}

	private void Traverse(Map map, Vertex[] vertexes, Point point, bool firstStep)
	{
		int children = 0;
		var u = vertexes[map.IndexOf(point)];
		u.Visited = true;
		u.Disc = u.Low = ++time;
		if (firstStep) Console.Error.WriteLine("Flagging {0} as visited at {1}", point, time);
		//printApMap(map, vertexes, point);
		foreach (var move in Player.validMoves(point, map, firstStep))
		{
			if (firstStep) Console.Error.WriteLine("Testing AP for " + move);
			var v = vertexes[map.IndexOf(move)];
			if (!v.Visited)
			{
				children++;
				v.Parent = u;
				Traverse(map, vertexes, move, false);
				if (u.Low > v.Low) u.Low = v.Low;

				if (u.Parent == null && children > 1)
				{
					u.IsArticulationPoint = true;
					Console.Error.WriteLine("Thinking that root node is an AP at {0}", u);
				}
				else if (u.Parent != null && v.Low >= u.Disc)
				{
					u.IsArticulationPoint = true;
					Console.Error.WriteLine("{0} is AP because {1} >= {2}", u, v.Low, u.Disc);
				}
				else
				{
					if (firstStep) Console.Error.WriteLine("{0} is NOT AP: {1} < {2}", u, v.Low, u.Disc);
				}
			}
			else if (v != u.Parent)
			{
				if (u.Low > v.Disc) u.Low = v.Disc;
			}
		}
	}

	private static void printApMap(Map map, Vertex[] vertexes, Point point)
	{
		var pointIndex = map.IndexOf(point);
		Player.printMap(vertexes.Select((x, index) =>
			index == pointIndex
				? '*'
				: x.Visited
					? (x.IsArticulationPoint
						? '!'
						: (char)('@' + x.Low))
					: map[index].HasValue ? '#' : '.')
			.ToArray(), map.Width);
	}

	public class Vertex
	{
		public int Index { get; set; }
		public bool Visited { get; set; }
		public int Disc { get; set; }
		public int Low { get; set; }
		public Vertex Parent { get; set; }
		public bool IsArticulationPoint { get; set; }

		public Vertex(int index) { Index = index; }
		public override string ToString()
		{
			return string.Format("#{0}: Visited: {1}, IsAP: {2}, Low: {3}, Disc: {4}", Index, Visited, IsArticulationPoint, Low, Disc);
		}
	}
}

//public class Dijkstra
//{
//	Node[] _nodes;
//	public int From { get; private set; }
//	Queue<Node> unvisitedNodes = new Queue<Node>();

//	public Dijkstra(Zone[] zones, int from)
//	{
//		_nodes = zones.Select(x => new Node { Id = x.Id, Neighbours = x.Neighbours.ToArray() }).ToArray();
//		Reset(from);
//	}

//	public void Reset(int from)
//	{
//		foreach (var node in _nodes)
//		{
//			node.Path = null;
//			//node.Distance = int.MaxValue;
//			node.Visited = false;
//		}
//		unvisitedNodes.Clear();

//		this.From = from;
//		_nodes[from].Path = new int[] { from };
//		unvisitedNodes.Enqueue(_nodes[from]);
//	}

//	public int[] Path(int to)
//	{
//		if (to >= _nodes.Length)
//			return null;//No paths to the destination at all

//		if (_nodes[to].Path != null)
//			return _nodes[to].Path;

//		while (unvisitedNodes.Any())
//		{
//			var currentNode = unvisitedNodes.Dequeue();
//			currentNode.Visited = true;
//			var tentativeDistance = currentNode.Path.Length + 1;

//			foreach (var neighbour in currentNode.Neighbours.Select(id => _nodes[id]).Where(node => !node.Visited))
//			{
//				if (neighbour.Path == null || neighbour.Path.Length > tentativeDistance)
//					neighbour.Path = currentNode.Path.Concat(new[] { neighbour.Id }).ToArray();
//				unvisitedNodes.Enqueue(neighbour);
//			}
//			if (currentNode.Id == to)
//				break;
//		}

//		return _nodes[to].Path;
//	}
//}

#endregion Classes and Enums
