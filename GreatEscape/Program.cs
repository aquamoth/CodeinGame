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
		int w = int.Parse(inputs[0]); // width of the board
		int h = int.Parse(inputs[1]); // height of the board
		int playerCount = int.Parse(inputs[2]); // number of players (2 or 3)
		int myId = int.Parse(inputs[3]); // id of my player (0 = 1st player, 1 = 2nd player, ...)

		// game loop
		while (true)
		{
			var players = new Point[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				int x = int.Parse(inputs[0]); // x-coordinate of the player
				int y = int.Parse(inputs[1]); // y-coordinate of the player
				int wallsLeft = int.Parse(inputs[2]); // number of walls available for the player

				players[i] = new Point { X = x, Y = y, WallsLeft = wallsLeft };
			}

			//Console.Error.WriteLine("My id = " + myId + ", Running horiz. = " + isRunningHorizontally + ", exit index = " + exitIndex);

			var walls = new List<Link>();
			int wallCount = int.Parse(Console.ReadLine()); // number of walls on the board
			for (int i = 0; i < wallCount; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				int wallX = int.Parse(inputs[0]); // x-coordinate of the wall
				int wallY = int.Parse(inputs[1]); // y-coordinate of the wall
				string wallOrientation = inputs[2]; // wall orientation ('H' or 'V')
				walls.AddRange(createWall(wallX, wallY, wallOrientation, w));
			}
			//Console.Error.WriteLine("Walls at:");
			//foreach (var link in walls)
			//{
			//	Console.Error.WriteLine(link);
			//}

			var map = emptyMap(w, h).Except(walls).ToList();


			var playerPaths = players.Select((player, index) =>
				{
					var exits = exitsForPlayer(index, w, h);
					var playerPath = pathFor(map, players[index], exits, w);
					return playerPath;
				}).ToArray();

			var currentLeaderboard = playerPaths
				.Select((path, index) => new { Id = index, Path = path, Score = path.Length - (index == myId ? 0.5 : 0.0) })
				.OrderBy(x => x.Score);

			Console.Error.WriteLine("Leaderboard: " + string.Join(", ", currentLeaderboard.Select(x => string.Format("{0}={1} pts", x.Id, x.Score)).ToArray()));

			var commandGiven = false;
			if (myId == currentLeaderboard.Skip(1).Select(x => x.Id).First())
			{
				Console.Error.WriteLine("I'm in second place! Try to stop the leader.");

				var locations = playerPaths[currentLeaderboard.Select(x => x.Id).First()].Take(2).ToArray();
				var movementToStop = new Link { A = locations[0], B = locations[1] };
				Console.Error.WriteLine("Try to stop movement: " + movementToStop);

				var heading = Link.DirectionOf(locations[0], locations[1]);
				var p = Point.From(locations[0], w);
				Link[] newWall = null;
				switch (heading)
				{
					case Direction.LEFT:
						newWall = createWall(p.X, p.Y, "V", w);
						break;
					case Direction.RIGHT:
						newWall = createWall(p.X + 1, p.Y, "V", w);
						break;
					case Direction.UP:
						newWall = createWall(p.X, p.Y, "H", w);
						break;
					case Direction.DOWN:
						newWall = createWall(p.X, p.Y + 1, "H", w);
						break;
					default:
						throw new NotSupportedException();
				}
				if (newWall != null)
				{
					var wallCommand = wallCommandOf(newWall.First(), w);
					Console.Error.WriteLine("Testing wall command: " + wallCommand);
					Console.Error.WriteLine("Want wall at First.A: " + newWall.First().A);
					Console.Error.WriteLine("Want wall at First.B: " + newWall.First().B);
					Console.Error.WriteLine("Want wall at Secnd.A: " + newWall.Skip(1).First().A);
					Console.Error.WriteLine("Want wall at Secnd.B: " + newWall.Skip(1).First().B);
					
					foreach (var wall in walls)
					{
						Console.Error.WriteLine("Existing wall: " + wall);
					}

					//TODO: Check we don't have overlap with existing walls
					//TODO: Check we don't trap opponents completely
					Console.WriteLine(wallCommand);
					commandGiven = true;
				}
			}

			if (!commandGiven)
			{
				Console.Error.WriteLine("I'm winning, or last of three, or CAN'T stop the leader right now. Just run and hope the others take each other out.");

				var shortestPath = playerPaths[myId];
				var direction = Link.DirectionOf(positionOf(players[myId].X, players[myId].Y, w), shortestPath[1]);
				Console.WriteLine(direction.ToString());
			}
		}
	}

	private static string wallCommandOf(Link link, int w)
	{
		var heading = Link.DirectionOf(link.A, link.B);
		var d = heading == Direction.RIGHT || heading == Direction.LEFT ? "V" : "H";
		var p = Point.From(link.A, w);
		return p + " " + d;
	}

	private static Link[] createWall(int wallX, int wallY, string wallOrientation, int w)
	{
		//TODO: This does not track where we can insert walls BETWEEN the existing parts
		Link[] wallSegments = null;
		if (wallOrientation == "V")
		{
			wallSegments = new[]
					{
						new Link { A = positionOf(wallX, wallY, w), B = positionOf(wallX - 1, wallY, w) },
						new Link { A = positionOf(wallX, wallY + 1, w), B = positionOf(wallX - 1, wallY + 1, w) }
					};
		}
		else if (wallOrientation == "H")
		{
			wallSegments = new[]{
						new Link { A = positionOf(wallX, wallY, w), B = positionOf(wallX, wallY - 1, w) },
						new Link { A = positionOf(wallX + 1, wallY, w), B = positionOf(wallX + 1, wallY - 1, w) }
					};
		}
		else
		{
			throw new NotSupportedException();
		}
		return wallSegments;
	}

	private static int[] pathFor(List<Link> map, Point player, Point[] myExits, int mapWidth)
	{
		var SI = positionOf(player.X, player.Y, mapWidth);
		var algorithm = new Dijkstra(map);
		int[] shortestPath = null;
		foreach (var exit in myExits)
		{
			var exitPosition = positionOf(exit.X, exit.Y, mapWidth);
			var path = algorithm.Path(SI, exitPosition);
			if (shortestPath == null || shortestPath.Length > path.Length)
				shortestPath = path;
		}

		if (shortestPath == null)
		{
			Console.Error.WriteLine("There is NO path to any exit!");
		}
		else
		{
			Console.Error.WriteLine("Shortest path to an exit is: " + string.Join(", ", shortestPath));
		}
		return shortestPath;
	}

	private static Point[] exitsForPlayer(int myId, int w, int h)
	{


		var isRunningHorizontally = myId != 2;
		var exitIndex = !isRunningHorizontally
			? h - 1
			: (myId == 0
				? w - 1
				: 0);

		var exits = isRunningHorizontally
			? Enumerable.Repeat(0, h).Select((x, index) => new Point { X = exitIndex, Y = index }).ToArray()
			: Enumerable.Repeat(0, w).Select((y, index) => new Point { X = index, Y = exitIndex }).ToArray();
		return exits;
	}

	private static List<Link> emptyMap(int w, int h)
	{
		var links = new List<Link>();
		for (int y = 0; y < h; y++)
			for (int x = 0; x < w; x++)
			{
				if (y > 0)
					links.Add(new Link { A = positionOf(x, y, w), B = positionOf(x, y - 1, w) });
				if (x > 0)
					links.Add(new Link { A = positionOf(x, y, w), B = positionOf(x - 1, y, w) });
			}
		return links;
	}

	static int positionOf(int x, int y, int width)
	{
		return (x + y * width);
	}
}




class Dijkstra
{
	public class Node
	{
		public Node(int position)
		{
			Position = position;
			Distance = int.MaxValue;
		}

		public int Position { get; private set; }
		public Node[] Neighbours { get; set; }
		public string ShortestPath { get; set; }
		public int Distance { get; set; }
		public bool Visited { get; set; }
	}

	IDictionary<int, Node> _nodes;

	public Dijkstra(IEnumerable<Link> links)
	{
		_nodes = links
			.SelectMany(link => link.Nodes)
			.Distinct()
			.Select(name => new Node(name))
			.ToDictionary(x => x.Position);

		foreach (var node in _nodes.Values)
		{
			node.Neighbours = links.Where(link => link.A == node.Position).Select(link => link.B)
					.Union(links.Where(link => link.B == node.Position).Select(link => link.A))
					.Select(name => _nodes[name])
					.ToArray();
		}
	}

	public int[] Path(int from, int to)
	{
		if (!_nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (!_nodes.ContainsKey(from))
			return null;//No paths from the source at all


		//Initialize the traversal
		var currentNode = _nodes[from];
		currentNode.Distance = 0;
		var unvisitedNodes = new List<Node>(_nodes.Values);

		do
		{
			var tentativeDistance = currentNode.Distance + 1;
			var unvisitedNeighbours = currentNode.Neighbours.Where(x => !x.Visited);

			foreach (var neighbour in unvisitedNeighbours)
			{
				if (neighbour.Distance > tentativeDistance)
				{
					neighbour.Distance = tentativeDistance;
					neighbour.ShortestPath = currentNode.ShortestPath + " " + currentNode.Position;
				}
			}

			currentNode.Visited = true;
			unvisitedNodes.Remove(currentNode);

			if (currentNode.Position == to)
				break;

			currentNode = unvisitedNodes.OrderBy(x => x.Distance).FirstOrDefault();
		}
		while (currentNode != null && currentNode.Distance != int.MaxValue);

		//Determine output
		var toNode = _nodes[to];
		if (toNode.Distance == int.MaxValue)
			return null; // No path to this gateway exists
		else
			return (currentNode.ShortestPath + " " + currentNode.Position).TrimStart().Split(' ').Select(x => int.Parse(x)).ToArray();
	}
}

public class Link
{
	public int A { get; set; }
	public int B { get; set; }

	public int[] Nodes { get { return new[] { A, B }; } }

	public override bool Equals(object obj)
	{
		var other = obj as Link;
		return obj != null 
			&& (
				(this.A.Equals(other.A) && this.B.Equals(other.B))
			|| (this.A.Equals(other.B) && this.B.Equals(other.A))
			);
	}

	public override int GetHashCode()
	{
		return A.GetHashCode() ^ B.GetHashCode();
	}

	public override string ToString()
	{
		return A + " " + B;
	}

	public static Direction DirectionOf(int from, int to)
	{
		if (to == from + 1)
			return Direction.RIGHT;
		else if (to == from - 1)
			return Direction.LEFT;
		else if (to < from)
			return Direction.UP;
		else if (to > from)
			return Direction.DOWN;
		else
			throw new NotSupportedException();
	}

}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }
	public int WallsLeft { get; set; }

	public static Point From(int position, int mapWidth)
	{
		var x = position % mapWidth;
		var y = position / mapWidth;
		return new Point { X = x, Y = y };
	}

	public override string ToString()
	{
		return X + " " + Y;
	}
}

public enum Direction
{
	UP,
	RIGHT,
	DOWN,
	LEFT
}