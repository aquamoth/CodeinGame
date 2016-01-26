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
		var game = new Game();
		while (true)
		{
			game.Execute();
		}
	}

	public static void Debug(string format, params object[] args)
	{
		Console.Error.WriteLine(/*Timer.ElapsedMilliseconds + " ms: " +*/ string.Format(format, args));
	}
}


public class Game
{
	Random random = new Random();

	int numberOfPlayers;
	Map map;
	char? lastDirection = null;

	public Game()
	{
		var height = int.Parse(Console.ReadLine());
		var width = int.Parse(Console.ReadLine());
		numberOfPlayers = int.Parse(Console.ReadLine());

		map = new Map(width, height);
	}

	public void Execute()
	{
		try
		{
			string north = Console.ReadLine();
			string east = Console.ReadLine();
			string south = Console.ReadLine();
			string west = Console.ReadLine();

			var players = readPlayersFromConsole();
			var me = players[4]; //TODO: Last?
			var opponents = players.Except(new[] { me });

			positionPlayersOnMap(players);
			map.SetVisited(me, north, east, south, west);

			map.PrintMap(players);

			var direction = selectNextDirection(me, opponents);
			lastDirection = direction;
			Console.WriteLine(direction);
		}
		catch (Exception ex)
		{
			Player.Debug("{0}", ex);
			Console.WriteLine(Map.DIRECTION_WAIT);
		}
	}

	private char selectNextDirection(Point me, IEnumerable<Point> opponents)
	{
		var nodes = nodesFrom(map);
		var myPaths = new Dijkstra(nodes, me.Index, me.Index);
		var closeOpponents = opponents
			.Select(opp => new { opp, distance = nodes[opp.Index].Distance })
			.Where(x => x.distance < 10)
			.OrderBy(x => x.distance)
			.ToArray();

		if (closeOpponents.Any())
		{
			Player.Debug("Found {0} opponents close by. Taking evasive action", closeOpponents.Length);
			//foreach (var opponent in closeOpponents)
			//{
			//	Player.Debug("#{0} has {1} steps: {2}", 
			//		opponent.opp.Id, 
			//		opponent.distance, 
			//		string.Join(", ", myPaths.Path[opponent.opp.Index].Select(index => Point.From(index, me.Width, me.Height))));
			//}



			int bestMove = me.Index;
			double bestScore = double.MaxValue;
			var possibleMoves = nodes[me.Index].Neighbours.Select(x => x.Node).Concat(new[] { nodes[me.Index] });
			foreach (var move in possibleMoves)
			{
				//Player.Debug("Testing move to {0}", Point.From(move.Node.Id, me.Width, me.Height));
				var movePaths = new Dijkstra(nodes, move.Id, move.Id);
				var score = 0.0;
				foreach (var opponent in opponents)
				{
					var length = nodes[opponent.Index].Distance;
					var opponentScore = Math.Pow(10.0 / length, 2);
					//Player.Debug("Scored length to player #{0} is {1} = {2} pts", opponent.Id, length, opponentScore);
					//Player.Debug("   {0}", string.Join(", ", nodes[opponent.Index].Path.Select(x => Point.From(x, me.Width, me.Height))));
					score += opponentScore;
				}
				//Player.Debug("Total score for {0} is {1}\n", move.Node, score);
				if (bestScore > score || (bestScore == score && random.Next(0, 2) == 0))
				{
					bestScore = score;
					bestMove = move.Id;
				}
			}
			//Player.Debug("Best move is to: {0}", Point.From(bestMove, me.Width, me.Height));

			return map.DirectionTo(me, bestMove);
		}




		Player.Debug("No opponents in close proximity. Exploring!");
		var directions = map.ExploreDirection(me);
		var validDirections = directions.Where(d => !hits(map.Move(me, Point.From(d)), opponents, map)).ToArray();

		char direction;
		if (!validDirections.Any())
		{
			Player.Debug("No valid path to explore, so we try to backtrack one step.");
			direction = reverse(lastDirection);
			//Just a sanity-check that possibleDirections contains the reverse of lastDirection
			if (!map.IsValidMove(me, direction))
			{
				throw new ApplicationException("Expected " + directions + " to be one of the possible directions.");
			}
		}
		else
		{
			direction = validDirections.FirstOrDefault();
		}
		return direction;
	}

	private Dijkstra.Node[] nodesFrom(Map map)
	{
		var nodes = map.Array.Select((x, index) => new Dijkstra.Node { Id = index }).ToArray();
		foreach (var node in nodes)
		{
			//var debugOutput = (node.Id == 6 + 22 * map.Width);

			if (map[node.Id] == Map.WALL)
			{
				//if(debugOutput) Player.Debug("Node {0} is a wall and has no neighbours");
				node.Neighbours = new Dijkstra.NodeCost[0];
			}
			else
			{
				var point = Point.From(node.Id, map.Width, map.Height);
				var exits = map.ValidExits(point);
				node.Neighbours = exits
					.Select(move => new Dijkstra.NodeCost { Node = nodes[move.Index], Cost = map[move.Index] == Map.UNKNOWN_SPACE ? 1.5 : 1.0 })
					.ToArray();
				//if (debugOutput) Player.Debug("Node {0} is at {1} and has exits to: {2}", node.Id, point, string.Join(", ", exits));
			}
		}
		return nodes;
	}



	private static bool hits(Point me, IEnumerable<Point> opponents, Map dfs)
	{
		var myRange = dfs.NeighboursOf(me).Concat(new[] { me.Index }).ToArray();

		var opponentPositions = opponents.Select(p => p.Index).ToArray();

		//Debug("Checking intersection of [{0}] and [{1}]", string.Join(", ", myRange), string.Join(", ", opponentPositions));
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

	private void positionPlayersOnMap(Point[] players)
	{
		Player.Debug("Players at: {0}", string.Join(", ", players.Select(p => p.ToString())));
		foreach (var player in players)
		{
			map.Set(player, Map.PATH_UNVISITED);
		}
	}

	private Point[] readPlayersFromConsole()
	{
		Point[] players;
		players = new Point[numberOfPlayers];
		for (int i = 0; i < numberOfPlayers; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int playerX = int.Parse(inputs[0]);
			int playerY = int.Parse(inputs[1]);
			players[i] = new Point(i, playerX, playerY, map.Width, map.Height);
		}
		return players;
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
			case Map.DIRECTION_WAIT: return new Point(0, 0);
			default:
				throw new NotSupportedException();
		}
	}

	public static Point From(int index, int width, int height)
	{
		return new Point(index % width, index / width, width, height);
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
			if (index < 0 || index >= this.Length)
				throw new ArgumentException("Invalid index");
			return _array[index];
		}
		set
		{
			if (index < 0 || index >= this.Length)
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

	public IEnumerable<Point> ValidExits(Point me)
	{
		return new[] { DIRECTION_EAST, DIRECTION_SOUTH, DIRECTION_WEST, DIRECTION_NORTH }
			.Select(direction => Move(me, Point.From(direction)))
			.Where(p => this[p.Index] != WALL)
			.ToArray();
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

	internal char DirectionTo(Point me, int targetIndex)
	{
		return new[] { DIRECTION_EAST, DIRECTION_SOUTH, DIRECTION_WEST, DIRECTION_NORTH, DIRECTION_WAIT }
			.Where(d => Move(me, Point.From(d)).Index == targetIndex)
			.First();
	}
}

#region Dijkstras algorithm for finding shortest path

public class Dijkstra
{
	public int PlayerPosition { get; set; }
	public int From { get; private set; }
	public IDictionary<int, int[]> Path { get; private set; }

	Queue<Node> unvisitedNodes = new Queue<Node>();

	public Dijkstra(Node[] nodes, int from, int playerPosition)
	{
		var nodeDictionary = nodes.ToDictionary(x => x.Id);
		Reset(nodeDictionary, from, playerPosition);

		Path = new Dictionary<int, int[]>();
		foreach (var to in nodeDictionary.Keys)
			Path.Add(to, PathTo(nodeDictionary, to));
	}

	private void Reset(IDictionary<int, Node> nodes, int from, int playerPosition)
	{
		foreach (var node in nodes.Values)
		{
			node.Path = null;
			node.Distance = int.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		this.PlayerPosition = playerPosition;

		nodes[from].Path = new int[] { from };
		nodes[from].Distance = 0;
		unvisitedNodes.Enqueue(nodes[from]);
	}

	private int[] PathTo(IDictionary<int, Node> nodes, int to)
	{
		if (!nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (nodes[to].Path != null)
			return nodes[to].Path;

		while (unvisitedNodes.Any())
		{
			var currentNode = unvisitedNodes.Dequeue();
			currentNode.Visited = true;
			if (currentNode.Path == null)
			{
				Player.Debug("Node {0} has NO Path!", currentNode.Id);
				continue;
			}
			foreach (var neighbour in currentNode.Neighbours.Where(node => !node.Node.Visited && node.Node.Id != PlayerPosition))
			{
				var tentativeDistance = currentNode.Distance + neighbour.Cost;
				bool isUnvisited = neighbour.Node.Path == null;
				if (isUnvisited || neighbour.Node.Distance > tentativeDistance)
				{
					neighbour.Node.Distance = tentativeDistance;
					neighbour.Node.Path = currentNode.Path.Concat(new[] { neighbour.Node.Id }).ToArray();
				}
				if (isUnvisited)
					unvisitedNodes.Enqueue(neighbour.Node);
			}
			if (currentNode.Id == to)
				break;
		}

		return nodes[to].Path;
	}


	public class Node
	{
		public int Id { get; set; }
		public NodeCost[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public double Distance { get; set; }
		public int[] Path { get; set; }

		public override string ToString()
		{
			return string.Format("{0}: d={1}, Neighbours: {2}", Id, Distance, Neighbours.Length);
		}
	}

	public class NodeCost
	{
		public Node Node { get; set; }
		public double Cost { get; set; }
	}
}

#endregion Dijkstras algorithm for finding shortest path

