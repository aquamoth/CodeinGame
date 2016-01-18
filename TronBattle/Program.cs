using System;
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
	static Stopwatch Timer = new Stopwatch();

	static void Main(string[] args)
	{
		test();

		var map = new Map(30, 20);
		Point[] players = null;

		// game loop
		var gameTurn = 0;
		var meanMilliseconds = 0.0;
		while (true)
		{
			var inputs = Console.ReadLine().Split(' ');
			var numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
			int myPlayerNumber = int.Parse(inputs[1]); // your player number (0 to 3).
			var positions = readPositionsFromConsole(numberOfPlayers);
			var firstStep = players == null;
			Timer.Restart();

			if (firstStep)
			{
				players = positions;
				putPlayerTailsOn(map, players);
				//if (gameTurn == 0) Console.Error.WriteLine("Put tails: {0} ms", sw1.ElapsedMilliseconds);
				putPlayersOn(map, players);
				//if (gameTurn == 0) Console.Error.WriteLine("Put players: {0} ms", sw1.ElapsedMilliseconds);
			}
			else
			{
				updatePlayerPositions(map, players, positions);
				putPlayersOn(map, players);
			}

			printMap(map);

			var heading = selectNextHeading(map, players, myPlayerNumber, firstStep);

			Timer.Stop();
			gameTurn++;
			meanMilliseconds += (Timer.ElapsedMilliseconds - meanMilliseconds) / gameTurn;
			Debug("End of turn. Mean is {2} ms.", gameTurn, Timer.ElapsedMilliseconds, meanMilliseconds);

			Console.WriteLine(heading);
		}
	}

	#region AI

	private static Direction selectNextHeading(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		//Debug("Determine controlled regions");
		var nodes = nodesFrom(map);
		//Debug("Created nodes from map");
		var ownedTiles = calculateControlledAreas(map, players, nodes);
		//printMap(ownedTiles.Select(x => x.HasValue ? char.Parse(x.Value.ToString()) : '#').ToArray(), map.Width);




		Debug("Identify rooms and articulation points");
		foreach (var player in players) map[map.IndexOf(player)] = null;
		var vertexes = vertexesFrom(map);
		foreach (var player in players) map[map.IndexOf(player)] = player.Id;
		Debug("Created vertexes from map");
		var tarjan = new HopcraftTarjan(vertexes);

		if (players.Length > 2)
		{
			throw new NotImplementedException("Three-player strategy");
		}
		else
		{
			//Debug("We found {0} components with a total of {1} rooms.", tarjan.Components.Count(), tarjan.Components.SelectMany(x => x).Count());
			//Debug("{0}", string.Join(", ", tarjan.Components.Select((x, index) => "C" + index + ": " + x.Count()).ToArray()));
			//var dic = dictionaryFrom(tarjan.Components);
			//if (players.Select(p => dic[map.IndexOf(p)]).Distinct().Count() == 1)
			//{
				return selectNextHeading_SameRoomStrategy(map, players, myPlayerNumber, firstStep);
			//}
			//else
			//{
			//	Debug("Players are in different rooms");
			//	Debug("{0}", string.Join(", ", players.Select(p => "#" + p.Id + ": " + dic[map.IndexOf(p)]).ToArray()));
			//	printMap(map, tarjan.Components);
			//	if (false) //Are we in a disjoint world?
			//	{
			//		throw new NotImplementedException("Wall-hugging strategy");
			//	}
			//	else
			//	{
			//		var me = players[myPlayerNumber];
			//		var myVertex = vertexes.Where(v => v.Id == map.IndexOf(me)).Single();
			//		if (myVertex.IsArticulationPoint)
			//		{
			//			throw new NotImplementedException("Stay in the current room if we are in a bigger world than our opponent, otherwise move into his room");
			//		}
			//		else
			//		{
			//			throw new NotImplementedException("Move towards the other player until we reach an articulation point");
			//		}
			//	}
			//}
		}
	
		//tarjan.vertexes.Select((v, index) => { Console.Error.WriteLine("{0} vertex is: {1}", Point.From(index, map.Width), v); return 0; }).ToArray();
		//var playerVertexes = players.Select(p => tarjan.vertexes[map.IndexOf(p)]);
		//playerVertexes.Select((v, index) => { Console.Error.WriteLine("P #{0} vertex is: {1}", index, v); return 0; }).ToArray();

		//printMap(map);

		//var apMap = tarjan.ArticulationPoints();
		//printApMap(map, tarjan.vertexes, me);
		//printMap(apMap.Select((b, index) => b ? '!' : map[index].HasValue ? '#' : '.').ToArray(), map.Width);
		//var articulationPoints = apMap
		//	.Select((yes, index) => yes ? (int?)index : null)
		//	.Where(x => x.HasValue)
		//	.Select(x => x.Value)
		//	.ToArray();
		//if (articulationPoints.Any())
		//{
		//	Console.Error.WriteLine("Identified articulation points at: {0}", string.Join(", ", articulationPoints.Select(idx => Point.From(idx, map.Width))));
		//}

		//if (apMap[map.IndexOf(me.X, me.Y)])
		//{
		//	Console.Error.WriteLine("CURRENTLY ON AN AP. CHOOSE DIRECTION WISELY!");
		//}


	}

	private static Direction selectNextHeading_SameRoomStrategy(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		Debug("Players are in same room");
		Debug("Find Volornov line, go there and cut off the other player");



		var me = players[myPlayerNumber];
		var allValidMoves = validMoves(me, map, firstStep);
		//Console.Error.WriteLine("Valid moves: " + string.Join(", ", allValidMoves));
		var possibleMoves = allValidMoves
			;//.Where(m => !articulationPoints.Contains(map.IndexOf(m.X, m.Y)));
		//Console.Error.WriteLine("Moves-APs: " + string.Join(", ", possibleMoves));
		if (!possibleMoves.Any())
			possibleMoves = allValidMoves;


		var scoredMoves = possibleMoves
			.Select(move => new { Heading = move.Heading, Score = score(map, move) })
			.OrderByDescending(x => x.Score)
			.ToArray();

		//Console.Error.WriteLine("Scored moves: " + string.Join(", ", scoredMoves.Select(x => string.Format("{0}={1}", x.Heading, x.Score))));

		return scoredMoves
			.Select(p => p.Heading)
			.DefaultIfEmpty(Direction.RIGHT) //If default we are dead anyway and just need to write something to die happily
			.FirstOrDefault();
	}

	private static int?[] calculateControlledAreas(Map map, Point[] players, Dijkstra.Node[] nodes)
	{
		var distances = players.Select(p =>
		{
			var d = new Dijkstra(nodes, map.IndexOf(p));
			var m = new { Player = p, DistanceMap = map.Array.Select((x, index) => x.HasValue ? null : (int?)d.Path(index).Length).ToArray() };
			return m;
		}).ToArray();
		Debug("Calculated distances");
		var ownedTiles = map.Array.Select((x, index) =>
			x.HasValue
				? null
			: (int?)distances.Select((d, playerIndex) => new { PlayerId = playerIndex, distance = d.DistanceMap[index] }).OrderBy(d => d.distance).First().PlayerId
			).ToArray();
		Debug("Created map of owned tiles");
		var playerAreas = players.Select((p, index) => new { p.Id, Count = ownedTiles.Where(x => x == index).Count() }).OrderBy(x => x.Id).Select(x => x.Count).ToArray();
		Debug("Calculated player areas");

		for (var i = 0; i < playerAreas.Length;i++ )
		{
			Debug("Player #{0} controls {1} tiles.", i, playerAreas[i]);
		}
		return ownedTiles;
	}

	public static void Debug(string format, params object[] args)
	{
		Console.Error.WriteLine(Timer.ElapsedMilliseconds + " ms: " + string.Format(format, args));
	}

	private static int score(Map map, Point p)
	{
		return wallsAt(map, p);
	}

	#endregion AI

	#region Business Logic

	private static void updatePlayerPositions(Map map, Point[] players, Point[] positions)
	{
		for (var i = 0; i < positions.Length; i++)
		{
			if (players[i].IsAlive && positions[i].X == -1)
			{
				Debug("Player #{0} died and is removed from the map", i);
				map.RemoveAll(i);
			}
			players[i].MoveTo(positions[i]);
		}
	}

	private static void putPlayersOn(Map map, Point[] players)
	{
		foreach (var player in players.Where(p => p.X >= 0))
		{
			//Console.Error.WriteLine("Marking player on map: " + player);
			map.Put(player.X, player.Y, player.Id);
		}
	}

	private static void putPlayerTailsOn(Map map, Point[] players)
	{
		foreach (var player in players.Where(p => p.X >= 0))
		{
			//Console.Error.WriteLine("Marking tail on map for {0}: ({1}, {2})" , player.Id, player.X0, player.Y0);
			map.Put(player.X0, player.Y0, player.Id);
		}
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

	#endregion

	#region Print Map methods

	public static void printMap(Map map, IEnumerable<HopcraftTarjan.Vertex[]> components)
	{
		var dic = dictionaryFrom(components);
		var charArray = map.Array.Select((c, index) => c.HasValue ? ((char)('a' + c.Value)) : char.Parse(dic[index].ToString())).ToArray();
		Player.printMap(charArray, map.Width);
	}

	private static Dictionary<int, int> dictionaryFrom(IEnumerable<HopcraftTarjan.Vertex[]> components)
	{
		var dic = components.SelectMany((array, index) => array.Select(v => new { v.Id, index })).ToDictionary(x => x.Id, x => x.index);
		return dic;
	}

	public static void printMap(Map map)
	{
		printMap(map.Array.Select(value => value.HasValue ? (char)('@' + value.Value) : '.').ToArray(), map.Width);
	}

	public static void printMap(char[] map, int width)
	{
		var height = map.Length / width;
		//Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		for (var y = 0; y < height; y++)
		{
			//Console.Error.Write('¤');
			for (var x = 0; x < width; x++)
			{
				var token = map[y * width + x];
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("");//¤
		}
		//Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		Console.Error.WriteLine();
	}

	#endregion Print Map methods

	#region Helpers

	private static Direction turn(Direction direction, int stepsToRight)
	{
		return (Direction)(((int)direction + stepsToRight) % 4);
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

	private static HopcraftTarjan.Vertex[] vertexesFrom(Map map)
	{
		var vertices = map.Array
			.Select((cell, index) => new { Id = index, IsWall = cell.HasValue })
			.Where(x => !x.IsWall)
			.Select(x => new HopcraftTarjan.Vertex(x.Id))
			.ToArray();
		foreach (var v in vertices)
		{
			v.Dependents = validMoves(Point.From(v.Id, map.Width), map, true)
				.Select(move => vertices.Where(x => x.Id == map.IndexOf(move)).Single())
				.ToArray();
		}
		return vertices;
	}

	private static Dijkstra.Node[] nodesFrom(Map map)
	{
		var nodes = map.Array
			.Select((cell, index) => new { Id = index, IsWall = cell.HasValue && cell.Value == Map.TILE_IS_WALL })
			.Where(x => !x.IsWall)
			.Select(x => new Dijkstra.Node { Id = x.Id })
			.ToArray();
		foreach (var v in nodes)
		{
			v.Neighbours = validMoves(Point.From(v.Id, map.Width), map, true)
				.Select(move => nodes.Where(x => x.Id == map.IndexOf(move)).Single())
				.ToArray();
		}
		return nodes;
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

	#region Testing

	private static void test()
	{
		var mapString = @"
..............................
..............................
..............................
..............................
..............................
..............................
........*************.........
..............................
..............................
..............................
..............................
.A............................
..............................
...............@..............
..............................
..............................
..............................
..............................
..............................
..............................";
		var width = mapString.IndexOf('\r', 1) - 2;
		var mapString2 = mapString.Replace("\r\n", "");
		var map = new Map(mapString2.Select(c => c == '.' ? null : (int?)c).ToArray(), width);
		//printMap(map);
		var players = new[]{ 
			Point.From(mapString2.IndexOf('@'), width, 0),
			Point.From(mapString2.IndexOf('A'), width, 1)
		};


		//var me = 1;

		var nodes = nodesFrom(map);

		var ownedTiles = calculateControlledAreas(map, players, nodes);
		printMap(ownedTiles.Select(x => x.HasValue ? char.Parse(x.Value.ToString()) : '#').ToArray(), map.Width);

		//var distances = players.Select(p =>
		//{
		//	var d = new Dijkstra(nodes, map.IndexOf(p));
		//	return new { Player = p, DistanceMap = map.Array.Select((x, index) => x.HasValue ? null : (int?)d.Path(index).Length).ToArray() };
		//}).ToArray();


		foreach (var player in players) map[map.IndexOf(player)] = null;
		var tarjan = new HopcraftTarjan(vertexesFrom(map));
		foreach (var player in players) map[map.IndexOf(player)] = player.Id;

		Debug("Found articulation points:");
		foreach (var ap in tarjan.ArticulationPoints)
		{
			Debug("{0}", ap);

		}
		printMap(map, tarjan.Components);

		var dic = dictionaryFrom(tarjan.Components);
		var room0 = dic[map.IndexOf(players[0])];
		var room1 = dic[map.IndexOf(players[1])];
		var isInSameRoom = room0 == room1;
		Debug("Player 1 is in room {0} and player 2 is in room {1}. Same={2}", room0, room1, isInSameRoom);


		var heading = selectNextHeading(map, players, 1, false);
		Debug("Heading: {0}", heading);
	}

	#endregion Testing
}

#region Classes and Enums

public class Map
{
	public const int TILE_IS_WALL = 29;

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
				return TILE_IS_WALL;
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
			return TILE_IS_WALL;
		if (y < 0 || y >= Height)
			return TILE_IS_WALL;
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

	internal static Point From(int idx, int width, int id = 0)
	{
		return new Point(id, idx % width, idx / width, 0, 0);
	}
}

public enum Direction
{
	LEFT = 0,
	UP = 1,
	RIGHT = 2,
	DOWN = 3
}

#region Hopcraft/Tarjan algorithm for finding biconnected components and articulation points

public class HopcraftTarjan
{
	List<Vertex> _component;

	public IEnumerable<Vertex[]> Components { get { return _components.AsEnumerable(); } }
	List<Vertex[]> _components;

	public IEnumerable<Vertex> ArticulationPoints { get; private set; }

	public HopcraftTarjan(Vertex[] vertexes)
	{
		_component = new List<Vertex>();
		_components = new List<Vertex[]>();

		Traverse(vertexes);

		ArticulationPoints = vertexes.Where(v => v.IsArticulationPoint).ToArray();
	}

	private void Traverse(IEnumerable<Vertex> vertexes)
	{
		foreach (var v in vertexes)
		{
			if (!v.Depth.HasValue)
			{
				Traverse(v, 0);
			}
		}

		_components.Add(_component.ToArray());
		_component = null;
	}

	private void Traverse(Vertex vertex, int depth)
	{
		vertex.Depth = depth;
		vertex.Low = depth;
		var childCount = 0;
		foreach (var nextVertex in vertex.Dependents)
		{
			if (!nextVertex.Depth.HasValue)
			{
				nextVertex.Parent = vertex;
				Traverse(nextVertex, depth + 1);
				childCount++;
				if (vertex.Parent != null && nextVertex.Low >= vertex.Depth) vertex.IsArticulationPoint = true;
				if (vertex.Low.Value > nextVertex.Low.Value) vertex.Low = nextVertex.Low.Value;
			}
			else if (nextVertex != vertex.Parent)
			{
				if (vertex.Low.Value > nextVertex.Depth.Value) vertex.Low = nextVertex.Depth.Value;
			}
		}


		_component.Add(vertex);
		if (vertex.IsArticulationPoint || (vertex.Parent == null && childCount > 1))
		{
			// Form a component of tracked vertexes
			if (vertex.Low == vertex.Depth)
			{
				_components.Add(_component.ToArray());
				_component = new List<Vertex>();
			}
		}
	}

	public class Vertex
	{
		public int Id { get; set; }
		public int? Depth { get; set; }
		public int? Low { get; set; }
		public Vertex Parent { get; set; }
		public bool IsArticulationPoint { get; set; }
		public Vertex[] Dependents { get; set; }

		public Vertex(int id) { Id = id; }
		public override string ToString()
		{
			return string.Format("#{0}: Low: {1}, Disc: {2}, Parent: {3}", Id, Low, Depth, Parent == null ? "" : Parent.Id.ToString());
		}
	}
}

#endregion Hopcraft/Tarjan algorithm for finding biconnected components and articulation points

#region Dijkstras algorithm for finding shortest path

public class Dijkstra
{
	IDictionary<int, Node> _nodes;
	public int From { get; private set; }
	Queue<Node> unvisitedNodes = new Queue<Node>();

	public Dijkstra(Node[] nodes, int from)
	{
		_nodes = nodes.ToDictionary(x => x.Id);
		//_nodes = zones.Select(x => new Node { Id = x.Id, Neighbours = x.Neighbours.ToArray() }).ToArray();
		Reset(from);
	}

	public void Reset(int from)
	{
		foreach (var node in _nodes.Values)
		{
			node.Path = null;
			//node.Distance = int.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		_nodes[from].Path = new int[] { from };
		unvisitedNodes.Enqueue(_nodes[from]);
	}

	public int[] Path(int to)
	{
		if (!_nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (_nodes[to].Path != null)
			return _nodes[to].Path;

		while (unvisitedNodes.Any())
		{
			var currentNode = unvisitedNodes.Dequeue();
			currentNode.Visited = true;
			var tentativeDistance = currentNode.Path.Length + 1;

			foreach (var neighbour in currentNode.Neighbours.Where(node => !node.Visited))
			{
				bool isUnvisited = neighbour.Path == null;
				if (isUnvisited || neighbour.Path.Length > tentativeDistance)
					neighbour.Path = currentNode.Path.Concat(new[] { neighbour.Id }).ToArray();
				if (isUnvisited)
					unvisitedNodes.Enqueue(neighbour);
			}
			if (currentNode.Id == to)
				break;
		}

		return _nodes[to].Path;
	}


	public class Node
	{
		public int Id { get; set; }
		public Node[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public int[] Path { get; set; }
	}
}

#endregion Dijkstras algorithm for finding shortest path

#endregion Classes and Enums
