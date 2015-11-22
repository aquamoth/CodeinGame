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

				//TODO: This does not track where we can insert walls BETWEEN the existing parts

				if (wallOrientation == "V")
				{
					walls.Add(new Link { A = nameOf(wallX, wallY, w), B = nameOf(wallX - 1, wallY, w) });
					walls.Add(new Link { A = nameOf(wallX, wallY + 1, w), B = nameOf(wallX - 1, wallY + 1, w) });
				}
				else if (wallOrientation == "H")
				{
					walls.Add(new Link { A = nameOf(wallX, wallY, w), B = nameOf(wallX, wallY - 1, w) });
					walls.Add(new Link { A = nameOf(wallX + 1, wallY, w), B = nameOf(wallX + 1, wallY - 1, w) });
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			//Console.Error.WriteLine("Walls at:");
			//foreach (var link in walls)
			//{
			//	Console.Error.WriteLine(link);
			//}

			var map = emptyMap(w, h).Except(walls).ToList();


			var player = players[myId];

			var myExits = exitsForPlayer(myId, w, h);

			string[] shortestPath = pathFor(map, player, myExits, w);


			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");

			var direction = directionOf(nameOf(players[myId].X, players[myId].Y, w), shortestPath[1]);
			Console.WriteLine(direction);
		}
	}

	private static string[] pathFor(List<Link> map, Point player, Point[] myExits, int mapWidth)
	{
		var SI = nameOf(player.X, player.Y, mapWidth);
		var algorithm = new Dijkstra(map);
		string[] shortestPath = null;
		foreach (var exit in myExits)
		{
			var exitName = nameOf(exit.X, exit.Y, mapWidth);
			var path = algorithm.Path(SI, exitName);
			if (shortestPath == null || shortestPath.Length > path.Length)
				shortestPath = path;
		}

		Console.Error.WriteLine("Shortest path to an exit is: " + string.Join(", ", shortestPath));
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

	private static string directionOf(string fromName, string toName)
	{
		var from = int.Parse(fromName);
		var to = int.Parse(toName);
		if (to == from + 1)
			return "RIGHT";
		else if (to == from - 1)
			return "LEFT";
		else if (to < from)
			return "UP";
		else if (to > from)
			return "DOWN";
		else
			throw new NotSupportedException();
	}

	private static List<Link> emptyMap(int w, int h)
	{
		var links = new List<Link>();
		for (int y = 0; y < h; y++)
			for (int x = 0; x < w; x++)
			{
				if (y > 0)
					links.Add(new Link { A = nameOf(x, y, w), B = nameOf(x, y - 1, w) });
				if (x > 0)
					links.Add(new Link { A = nameOf(x, y, w), B = nameOf(x - 1, y, w) });
			}
		return links;
	}

	static string nameOf(int x, int y, int width)
	{
		return (x + y * width).ToString();
	}
}










class Dijkstra
{
	public class Node
	{
		public Node(string name)
		{
			Name = name;
			Distance = int.MaxValue;
		}

		public string Name { get; private set; }
		public Node[] Neighbours { get; set; }
		public string ShortestPath { get; set; }
		public int Distance { get; set; }
		public bool Visited { get; set; }
	}

	IDictionary<string, Node> _nodes;

	public Dijkstra(IEnumerable<Link> links)
	{
		_nodes = links
			.SelectMany(link => link.Nodes)
			.Distinct()
			.Select(name => new Node(name))
			.ToDictionary(x => x.Name);

		foreach (var node in _nodes.Values)
		{
			node.Neighbours = links.Where(link => link.A == node.Name).Select(link => link.B)
					.Union(links.Where(link => link.B == node.Name).Select(link => link.A))
					.Select(name => _nodes[name])
					.ToArray();
		}
	}

	public string[] Path(string from, string to)
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
					neighbour.ShortestPath = currentNode.ShortestPath + " " + currentNode.Name;
				}
			}

			currentNode.Visited = true;
			unvisitedNodes.Remove(currentNode);

			if (currentNode.Name == to)
				break;

			currentNode = unvisitedNodes.OrderBy(x => x.Distance).FirstOrDefault();
		}
		while (currentNode != null && currentNode.Distance != int.MaxValue);

		//Determine output
		var toNode = _nodes[to];
		if (toNode.Distance == int.MaxValue)
			return null; // No path to this gateway exists
		else
			return (currentNode.ShortestPath + " " + currentNode.Name).TrimStart().Split(' ');
	}
}

public class Link
{
	public string A { get; set; }
	public string B { get; set; }

	public string[] Nodes { get { return new[] { A, B }; } }

	public override string ToString()
	{
		return A + " " + B;
	}

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
}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }
	public int WallsLeft { get; set; }
}