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
	//const int MAX_THREAT_DISTANCE = 7;
	const int MAX_AI_TIME_MS = 90;

	Random random = new Random();

	int numberOfPlayers;
	Map map;
	char? lastDirection = null;
	Stopwatch timer;
	int gameTurn = 0;
	int? targetTile = null;
	int lastTile = 0;

	public Game()
	{
		var height = int.Parse(Console.ReadLine());
		var width = int.Parse(Console.ReadLine());
		numberOfPlayers = int.Parse(Console.ReadLine());

		map = new Map(width, height);
		timer = new Stopwatch();
	}

	public void Execute()
	{
		try
		{
			gameTurn++;

			string north = Console.ReadLine();
			string east = Console.ReadLine();
			string south = Console.ReadLine();
			string west = Console.ReadLine();

			const int myPlayerIndex = 4;//TODO: Is always last player?
			var players = readPlayersFromConsole();
			var me = players[myPlayerIndex]; 
			var opponents = players.Except(new[] { me });

			timer.Restart();

			positionPlayersOnMap(players);
			map.SetVisited(me, north, east, south, west);

			if (gameTurn > 210)
				map.PrintMap(players);

			var direction = selectNextDirection(players, myPlayerIndex);
			lastDirection = direction;

			Player.Debug("{0} ms", timer.ElapsedMilliseconds);
			Console.WriteLine(direction);
		}
		catch (Exception ex)
		{
			Player.Debug("{0}", ex);
			Console.WriteLine(Map.DIRECTION_WAIT);
			throw;
		}
	}

	private char selectNextDirection(Point[] players, int myPlayerIndex)
	{
		//  2
		//  .
		// 0.....1
		//   .  .
		//   .  .


		//start timer
		// mark my position as processed so we don't try to backtrack to current position
		//push my tile exits to queue (BFS)
		//while queue.any() && timer < MAX_TIME (80ms?)
		//  take tile from queue
		//  mark the tile as processed
		//  if my.distanceTo(tile) >= opponents.min(o=>o.distanceTo(tile)) => lowest prioqueue
		//	else if tile is DEAD_END => low prioqueue
		//  else if tile is VISITED => normal prioqueue
		//	else (since tile is UNVISITED) => high prioqueue
		//	if normal or high prioqueue => push tile.exits to queue
		//for each prioqueue in order high to lowest
		//  calculate score = lowest distance to opponents when I reach the tile
		//  if tile with high score in high prioqueue has score >= MIN_SCORE (5?) (will probably automatically favor closest unvisited node)
		//		end algorithm and walk towards the tile
		//  else 
		//    score += my distance to the tile
		//    walk towards the tile with high score in prioqueue normal, low, lowest (including current tile)

		var me = players[myPlayerIndex];
		var opponents = players.Except(new[]{me}).ToArray();
		Player.Debug("I'm at {0}", me.Index);

		var processedNodes = new HashSet<int>(new[] { me.Index });
		var nodes = nodesFrom(map);
		var paths = players.Select(player => new Dijkstra(nodes, player.Index, player.Index)).ToArray();
		var queue = new Queue<Dijkstra.Node>(nodes[me.Index].Neighbours.Select(x => x.Node));
		var prioQueues = Enumerable.Range(1, 4).Select(x => new List<Tuple<Dijkstra.Node, int, int>>()).ToArray();
		while (queue.Any() && timer.ElapsedMilliseconds<MAX_AI_TIME_MS)
		{
			var tile = queue.Dequeue();
			if (processedNodes.Contains(tile.Id)) continue;//Guard clause
			processedNodes.Add(tile.Id);
			var myDistanceToTile = paths[myPlayerIndex].Path[tile.Id].Length;
			var opponentsDistanceToTile = shortestDistanceFor(opponents.Select(x => x.Id).ToArray(), paths, tile);
			var queueItem = new Tuple<Dijkstra.Node, int, int>(tile, opponentsDistanceToTile, myDistanceToTile);
			if (myDistanceToTile >= opponentsDistanceToTile)
				prioQueues[3].Add(queueItem); //Kill-paths => lowest prioqueue
			else if (map[tile.Id] == Map.PATH_UNVISITED)
				prioQueues[0].Add(queueItem); //Unvisited nodes => highest prioqueue
			else if (!tile.Neighbours.Any()) //Visited dead-ends => low prioqueue
				prioQueues[2].Add(queueItem);
			else //Visited with exits => normal prioqueue
			{
				prioQueues[1].Add(queueItem);
				foreach (var exit in tile.Neighbours)
					queue.Enqueue(exit.Node);//TODO: Only if not in processedNodes hashset, or is that redundant?
			}
		}

		if (queue.Any())
			Player.Debug("Had to stop ai before processing entire reachable map");

		Dijkstra.Node bestNode = null;

		if (targetTile.HasValue && targetTile != me.Index)
		{
			bestNode = prioQueues[0].Where(x => x.Item1.Id == targetTile.Value).Select(x => x.Item1).FirstOrDefault();
			if (bestNode != null)
			{
				Player.Debug("Staying on path to {0} ({1} steps left)", targetTile.Value, bestNode.Distance);
			}
			else
			{
				Player.Debug("Optimized destination at {0} no longer reachable.", targetTile);
				targetTile = null;
			}
		}

		//First try to reach a safe unvisited node
		if (bestNode == null)
		{
			const int MIN_UNVISITED_SCORE = 5;
			bestNode = prioQueues[0]
				.OrderByDescending(x => x.Item3)
				.Where(x => x.Item3 < MIN_UNVISITED_SCORE)
				.Where(x => !x.Item1.Path.Contains(lastTile))
				.Select(x => x.Item1)
				.FirstOrDefault();

			if (bestNode == null)
			{
				Player.Debug("None of {0} unvisited tiles are reachable within {1} moves without backtracking", prioQueues[0].Count, MIN_UNVISITED_SCORE);
				foreach (var x in prioQueues[0])
				{
					Player.Debug("{0}: {1} steps vs {2} steps", x.Item1, x.Item2, x.Item3);
				}
				//then try to reach any visited node
				bestNode = prioQueues[1]
					.OrderByDescending(x => x.Item2 + x.Item3)
					.Select(x => x.Item1)
					.FirstOrDefault();

				if (bestNode == null)
				{
					Player.Debug("No visited tiles are reachable");
					//then try to reach an unvisited node although dangerous
					bestNode = prioQueues[0]
						.OrderByDescending(x => x.Item2)
						.Select(x => x.Item1)
						.FirstOrDefault();

					if (bestNode == null)
					{
						Player.Debug("No unvisited tiles are reachable at all");
						//then fallback to walking into a dead end and hope for the best
						bestNode = prioQueues[2]
							.OrderByDescending(x => x.Item2 + x.Item3)
							.Select(x => x.Item1)
							.FirstOrDefault();

						if (bestNode == null)
						{
							Player.Debug("No dead-end tiles are reachable");
							//finally just walk towards near-certain death
							bestNode = prioQueues[3]
								.OrderByDescending(x => x.Item2 + x.Item3)
								.Select(x => x.Item1)
								.FirstOrDefault();

							if (bestNode == null)
							{
								Player.Debug("BUG! Failed to find ANY path from current position!");
								return Map.DIRECTION_WAIT;
							}
						}
					}
				}
			}
			//else
			//{
			//	Player.Debug("Found {0} unvisited nodes that are reachable within 5 steps")
			//}
			targetTile = null;//DISABLED bestNode.Id;
		}

		lastTile = me.Index;
		var nextTile = paths[myPlayerIndex].Path[bestNode.Id].Skip(1).First();
		Player.Debug("Moving to {0} through {1}", bestNode.Id, nextTile);
		return map.DirectionTo(me, nextTile);
	}

	private static int shortestDistanceFor(int[] playerIndexes, Dijkstra[] paths, Dijkstra.Node tile)
	{
		//Player.Debug("Indexes: {0}", string.Join(", ", playerIndexes));
		//Player.Debug("Paths: {0}", paths.Length);
		var length = playerIndexes
			.Where(index => paths[index].Path != null)
			.ToArray();
		length  = length
			.Where(index => paths[index].Path.ContainsKey(tile.Id))
			.ToArray();
		length = length
			.Select(index => paths[index].Path[tile.Id])
			.Where(path => path != null)
			.Select(path => path.Length)
			.ToArray();
		if (length == null || length.Length == 0)
			return int.MaxValue;

		return length
			.DefaultIfEmpty(int.MaxValue)
			.Min();
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
		Player.Debug("I'm at {0} and want to go to {1} with width={2}", me.Index, targetIndex, me.Width);
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

