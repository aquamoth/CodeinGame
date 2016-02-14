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
		var height = int.Parse(Console.ReadLine());
		var width = int.Parse(Console.ReadLine());
		var numberOfPlayers = int.Parse(Console.ReadLine());

		var game = new Game(width, height, numberOfPlayers);
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
	Stopwatch timer;
	int gameTurn = 0;

	public Game(int width, int height, int numberOfPlayers)
	{
		this.numberOfPlayers = numberOfPlayers;
		map = new Map(width, height);
		timer = new Stopwatch();
	}

	public void Execute()
	{
		try
		{
			gameTurn++;
			var debugMode = true;// gameTurn >= 250;

			string north = Console.ReadLine();
			string east = Console.ReadLine();
			string south = Console.ReadLine();
			string west = Console.ReadLine();

			int myPlayerIndex = numberOfPlayers - 1;//TODO: Is always last player?
			var players = readPlayersFromConsole();
			var me = players[myPlayerIndex];
			var opponents = players.Except(new[] { me });

			timer.Restart();

			positionPlayersOnMap(players);
			map.SetVisited(me, north, east, south, west);

			if (gameTurn % 10 == 0)
				map.PrintMap(players);

			var direction = selectNextDirection(players, myPlayerIndex, debugMode);

			//Player.Debug("{0} ms", timer.ElapsedMilliseconds);
			Console.WriteLine(direction);
		}
		catch (Exception ex)
		{
			Player.Debug("{0}", ex);
			Console.WriteLine(Map.DIRECTION_WAIT);
			//throw;
		}
	}

	private char selectNextDirection(Point[] players, int myPlayerIndex, bool debugMode)
	{
		//  2
		//  .
		// 0.....1
		//   .  .
		//   .  .


		//start timer
		var me = players[myPlayerIndex];
		var opponents = players.Except(new[] { me }).ToArray();
		if (debugMode) Player.Debug("I'm at {0}", Point.From(me.Index, map.Width, map.Height));

		MaxiMinState bestPath = null;
		var queue = new Queue<MaxiMinState>();
		var nodes = nodesFrom(map);

		// push all my exits to queue bundled with opponents current positions
		queue.Enqueue(new MaxiMinState { Steps = new[] { me.Index }, Me = me.Index, Opponents = opponents.Select(x => x.Index).ToArray(), Score = 0 });
		var counter = 0;

		// foreach position in queue
		while (queue.Any() && timer.ElapsedMilliseconds < MAX_AI_TIME_MS)
		{
			//	position me at new position
			counter++;
			var state = queue.Dequeue();

			var myPath = string.Join("-", state.Steps);

			//  push all new exits to queue (with path) bundled with opponents new positions
			foreach (var exit in nodes[state.Me].Neighbours.ToArray())//.Where(x => x.Cost == 1.0)
			{
				if (timer.ElapsedMilliseconds >= MAX_AI_TIME_MS)
				{
					Console.Error.WriteLine("Looping through exits when time is up.");
					break; //Time is up
				}

				//if (debugMode) Console.Error.Write("{0}: Eval {1}->{2} [{3}]: ", counter, myPath, exit.Node.Id, string.Join(", ", state.Opponents));
				if (state.Opponents.Any(x => x == exit.Node.Id))
				{
					//if (debugMode) Console.Error.WriteLine("Walked into opponent");
					continue;
				}

				//	move opponents towards my new position
				var opponentNewPositions = new int[state.Opponents.Length];
				for (int i = 0; i < state.Opponents.Length; i++)
				{
					if (timer.ElapsedMilliseconds >= MAX_AI_TIME_MS)
					{
						Console.Error.WriteLine("Looping through opponents when time is up.");
						break; //Time is up
					}

					var opponentPath = new Dijkstra(nodes, state.Opponents[i]);
					var newPosition = opponentPath.GetPathTo(exit.Node.Id).Skip(1).First();
					if (newPosition == exit.Node.Id)
					{
						//if (debugMode) Console.Error.WriteLine("Killed by opponent");
						break;
					}
					opponentNewPositions[i] = newPosition;
				}
				if (opponentNewPositions.Last() == 0)
					continue; //We were killed by opponent in inner loop or time is up


				var score = state.Score + valueOf(map[exit.Node.Id]);
				var newState = new MaxiMinState { Steps = state.Steps.Concat(new[] { exit.Node.Id }).ToArray(), Me = exit.Node.Id, Opponents = opponentNewPositions, Score = score };
				if (bestPath == null || bestPath.Score < newState.Score)
				{
					bestPath = newState;
					if (debugMode) Console.Error.Write("{0}: Eval {1}->{2} [{3}]: ", counter, myPath, exit.Node.Id, string.Join(", ", state.Opponents));
					if (debugMode) Console.Error.WriteLine("Score {0}", newState.Score);
				}
				//else
				//{
				//	if (debugMode) Console.Error.WriteLine("----- {0}", newState.Score);
				//}

				queue.Enqueue(newState);
			}

			//	if queue empty or time is up, break
		}

		//walk towards the best path
		Player.Debug("Calculated {0} iterations. Best path is {1} steps.", counter, bestPath == null ? 0 : bestPath.Steps.Length);
		var nextTile = bestPath == null || bestPath.Steps.Length < 2 ? me.Index : bestPath.Steps.Skip(1).First();
		//if (debugMode) Player.Debug("Moving to {0} through {1}", bestPath == null ? nextTile : bestPath.Me, nextTile);
		return map.DirectionTo(me, nextTile);
	}

	private double valueOf(char mapTile)
	{
		switch (mapTile)
		{
			case Map.UNKNOWN_SPACE: return -1;
			case Map.PATH_UNVISITED: return 5;
			case Map.PATH_VISITED: return 0;
			case Map.PATH_DEADEND: return -1;
			default:
				throw new NotSupportedException("Can't calculate value of map tile: " + mapTile);
		}
	}


	class MaxiMinState
	{
		public int[] Steps { get; set; }
		public int Me { get; set; }
		public int[] Opponents { get; set; }
		public double Score { get; set; }
	}

	private static int shortestDistanceFor(int[] playerIndexes, Dijkstra[] paths, Dijkstra.Node tile)
	{
		//Player.Debug("Indexes: {0}", string.Join(", ", playerIndexes));
		//Player.Debug("Paths: {0}", paths.Length);
		var length = playerIndexes
			.Where(index => paths[index].Path != null)
			.ToArray();
		length = length
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
		//Player.Debug("Players at: {0}", string.Join(", ", players.Select(p => p.ToString())));
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

	internal int StraightDistanceTo(int index)
	{
		var p = Point.From(index, this.Width, this.Height);
		return Math.Abs(p.X - this.X) + Math.Abs(p.Y - this.Y);
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
		//TODO: if we only have one exit that is not a dead end, this is a dead end
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

	//internal IEnumerable<char> ExploreDirection(Point me)
	//{
	//	var possibleDirections = directionsTo(me, PATH_UNVISITED);
	//	if (possibleDirections.Any())
	//	{
	//		return possibleDirections;
	//	}
	//	else
	//	{
	//		Set(me, PATH_DEADEND);
	//		possibleDirections = directionsTo(me, PATH_VISITED);
	//		return possibleDirections;
	//	}
	//}

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
		Player.Debug("Moving from {0} to {1}", me.Index, targetIndex);
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
	IDictionary<int, Node> nodes;

	public Dijkstra(Node[] nodes, int from, int? playerPosition = null, int? to = null)
	{
		this.nodes = nodes.ToDictionary(x => x.Id);
		Reset(from, playerPosition ?? from);

		Path = new Dictionary<int, int[]>();
		if (to.HasValue)
		{
			Path.Add(to.Value, PathTo(to.Value));
		}
		//else
		//{
		//	foreach (var toKey in nodeDictionary.Keys)
		//		Path.Add(toKey, PathTo(nodeDictionary, toKey));
		//}
	}

	private void Reset(int from, int playerPosition)
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

	public int[] GetPathTo(int to)
	{
		if (!Path.ContainsKey(to))
			Path.Add(to, PathTo(to));
		return Path[to];
	}

	private int[] PathTo(int to)
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

