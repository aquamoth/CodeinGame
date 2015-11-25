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
		//unitTests();

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
				//Console.Error.WriteLine("Player " + i + " at " + players[i]);
			}

			var walls = new List<Wall>();
			int wallCount = int.Parse(Console.ReadLine()); // number of walls on the board
			for (int i = 0; i < wallCount; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				int wallX = int.Parse(inputs[0]); // x-coordinate of the wall
				int wallY = int.Parse(inputs[1]); // y-coordinate of the wall
				string wallOrientation = inputs[2]; // wall orientation ('H' or 'V')
				walls.Add(createWall(new Point(wallX, wallY), wallOrientation));
			}
			//Console.Error.WriteLine("Walls at:");
			//foreach (var wall in walls)
			//{
			//	Console.Error.WriteLine(wall);
			//}

			var map = emptyMap(w, h).Where(link=> !walls.Any(wall=> isBlockedBy(link, wall, w))).ToList();
			//Console.Error.WriteLine("Map consists of " + map.Count + " links");

			var playerPaths = bestPathsFor(players, map, w, h).ToArray();
			//Console.Error.WriteLine("Determining leaderboard");
			var currentLeaderboard = playerPaths.Select((path, index) => new
				{
					Id = index,
					Path = path,
					Score = (path == null || !players[index].IsRunning ? int.MaxValue : path.Length) - (index == myId ? 0.5 : 0.0)
				})
				.OrderBy(x => x.Score)
				.ToArray();

			//Console.Error.WriteLine("Leaderboard:" + string.Join(", ", currentLeaderboard.Select(x => string.Format("{0}={1} pts", x.Id, x.Score)).ToArray()));

			var commandGiven = false;
			if (players[myId].WallsLeft > 0 && myId != currentLeaderboard[0].Id)
			{
				if (myId == currentLeaderboard[1].Id)
				{
					Console.Error.WriteLine("I'm in second place");
				}
				else
				{
					Console.Error.WriteLine("I'm in third place with " + playerPaths[myId].Length + " steps to go");
				}
				//I'm not winning, and I have walls left
				if (myId == currentLeaderboard[1].Id || (playerPaths[myId].Length <= 5))
				{
					Console.Error.WriteLine("Trying to stop the leader.");
					var leaderId = currentLeaderboard.Select(x => x.Id).First();
					var leaderPath = playerPaths[leaderId];

					Wall bestWall = null;
					int bestScore = 0;
					for (int i = 0; i < leaderPath.Length - 2; i++)
					{
						var movementToStop = new Link { A = leaderPath[i], B = leaderPath[i + 1] };
						//Console.Error.Write("Try to stop movement: " + movementToStop);
						var heading = Link.DirectionOf(movementToStop.A, movementToStop.B);
						//Console.Error.WriteLine(", to " + heading);

						//TODO: Consider using the wall that causes the longest new path for the opponent, yet the shortest path for self
						var newWallsToTest = wallsThatBlocks(movementToStop.A, heading, w)
							.Where(wall => wall.Inside(new Area { X1 = 0, X2 = w, Y1 = 0, Y2 = h }))
							.Where(w1 => !walls.Any(w2 => w2.Overlaps(w1)));

						foreach (var testWall in newWallsToTest)
						{
							//Console.Error.WriteLine("Testing wall at " + testWall);
							var testMap = map.Where(link => !isBlockedBy(link, testWall, w)).ToList();
							//Console.Error.WriteLine("Testmap consists of " + testMap.Count + " links");

							var resultingPlayerPaths = bestPathsFor(players, testMap, w, h).ToArray();
							//Console.Error.WriteLine("Resulting player paths:");
							//foreach (var path in resultingPlayerPaths)
							//{
							//	Console.Error.WriteLine("Path: " + string.Join(", ", path ?? new int[0]));
							//}
							if (resultingPlayerPaths.Any(p => p == null))
							{
								//Console.Error.WriteLine("The wall traps at least one player.");
								continue; //At least one player has NO paths to get out, so this is not a valid alternative
							}
							else
							{
								var testScore = resultingPlayerPaths.Select((path, index) =>
								{
									return !players[index].IsRunning ? 0 : (path.Length - playerPaths[index].Length) * (index == myId ? -1 : 1);
								}).Sum();
								//Console.Error.WriteLine("The wall gives a score of " + testScore);

								if (testScore > bestScore)
								{
									bestWall = testWall;
									bestScore = testScore;
								}
							}
						}
					}

					if (bestWall != null)
					{
						var wallCommand = commandFor(bestWall);
						Console.Error.WriteLine("Using wall command: " + wallCommand);

						Console.WriteLine(wallCommand);
						commandGiven = true;
					}
				}
			}

			if (!commandGiven)
			{
				Console.Error.WriteLine("I'm winning, or last of three, or CAN'T stop the leader right now. Just run and hope the others take each other out.");

				var shortestPath = playerPaths[myId];
				var currentPosition = positionOf(players[myId].X, players[myId].Y, w);
				if (currentPosition != shortestPath[0])
					throw new ApplicationException("Expected code optimization here");
				var direction = Link.DirectionOf(currentPosition, shortestPath[1]);
				Console.WriteLine(direction.ToString());
			}
		}
	}

	private static IEnumerable<int[]> bestPathsFor(Point[] players, List<Link> map, int mapWidth, int mapHeight)
	{
		return players.Select((player, index) =>
		{
			if (player.IsRunning)
			{
				var exits = exitsForPlayer(index, mapWidth, mapHeight);
				return pathFor(map, players[index], exits, mapWidth);
			}
			else
			{
				return new int[0];
			}
		});
	}

	private static void unitTests()
	{
		var verticalWall = new Wall { X1 = 2, X2 = 2, Y1 = 1, Y2 = 3 };

		if (verticalWall.Overlaps(createWall(new Point(1, 0), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(1, 1), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(1, 2), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(1, 3), "V"))) throw new ApplicationException();

		if (!verticalWall.Overlaps(createWall(new Point(2, 0), "V"))) throw new ApplicationException();
		if (!verticalWall.Overlaps(createWall(new Point(2, 1), "V"))) throw new ApplicationException();
		if (!verticalWall.Overlaps(createWall(new Point(2, 2), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(2, 3), "V"))) throw new ApplicationException();

		if (verticalWall.Overlaps(createWall(new Point(3, 0), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(3, 1), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(3, 2), "V"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(3, 3), "V"))) throw new ApplicationException();

		if (verticalWall.Overlaps(createWall(new Point(0, 1), "H"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(1, 1), "H"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(2, 1), "H"))) throw new ApplicationException();

		if (verticalWall.Overlaps(createWall(new Point(0, 2), "H"))) throw new ApplicationException();
		if (!verticalWall.Overlaps(createWall(new Point(1, 2), "H"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(2, 2), "H"))) throw new ApplicationException();

		if (verticalWall.Overlaps(createWall(new Point(0, 3), "H"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(1, 3), "H"))) throw new ApplicationException();
		if (verticalWall.Overlaps(createWall(new Point(2, 3), "H"))) throw new ApplicationException();




		if (isBlockedBy(new Link { A = 0, B = 1 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 1, B = 2 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 2, B = 3 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 1, B = 0 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 2, B = 1 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 3, B = 2 }, verticalWall, 10)) throw new ApplicationException();

		if (isBlockedBy(new Link { A = 10, B = 11 }, verticalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 11, B = 12 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 12, B = 13 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 11, B = 10 }, verticalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 12, B = 11 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 13, B = 12 }, verticalWall, 10)) throw new ApplicationException();

		if (isBlockedBy(new Link { A = 20, B = 21 }, verticalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 21, B = 22 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 22, B = 23 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 21, B = 20 }, verticalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 22, B = 21 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 23, B = 22 }, verticalWall, 10)) throw new ApplicationException();

		if (isBlockedBy(new Link { A = 30, B = 31 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 31, B = 32 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 32, B = 33 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 31, B = 30 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 32, B = 31 }, verticalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 33, B = 32 }, verticalWall, 10)) throw new ApplicationException();





		var horisontalWall = new Wall { X1 = 1, X2 = 3, Y1 = 2, Y2 = 2 };
		if (isBlockedBy(new Link { A = 0, B = 10 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 1, B = 11 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 2, B = 12 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 3, B = 13 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 10, B = 0 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 11, B = 1 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 12, B = 2 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 13, B = 3 }, horisontalWall, 10)) throw new ApplicationException();

		if (isBlockedBy(new Link { A = 10, B = 20 }, horisontalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 11, B = 21 }, horisontalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 12, B = 22 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 13, B = 23 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 20, B = 10 }, horisontalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 21, B = 11 }, horisontalWall, 10)) throw new ApplicationException();
		if (!isBlockedBy(new Link { A = 22, B = 12 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 23, B = 13 }, horisontalWall, 10)) throw new ApplicationException();

		if (isBlockedBy(new Link { A = 20, B = 30 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 21, B = 31 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 22, B = 32 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 23, B = 33 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 30, B = 20 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 31, B = 21 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 32, B = 22 }, horisontalWall, 10)) throw new ApplicationException();
		if (isBlockedBy(new Link { A = 33, B = 23 }, horisontalWall, 10)) throw new ApplicationException();


		var testMap1 = emptyMap(9, 9);
		if (testMap1.Count != 144) throw new ApplicationException();
		var testwalls = new List<Wall> { new Wall { X1 = 2, Y1 = 3, X2 = 2, Y2 = 5 } };
		var testMap2 = testMap1.Where(link => !testwalls.Any(wall => isBlockedBy(link, wall, 9))).ToList();
		if (testMap2.Count != 142) throw new ApplicationException();
	}

	private static IEnumerable<Wall> wallsThatBlocks(int position, Direction heading, int w)
	{
		var p = Point.From(position, w);
		switch (heading)
		{
			case Direction.LEFT:
				yield return createWall(p, "V");
				yield return createWall(p + Direction.UP, "V");
				break;
			case Direction.RIGHT:
				yield return createWall(p + Direction.RIGHT, "V");
				yield return createWall(p + Direction.RIGHT + Direction.UP, "V");
				break;
			case Direction.UP:
				yield return createWall(p, "H");
				yield return createWall(p + Direction.LEFT, "H");
				break;
			case Direction.DOWN:
				yield return createWall(p + Direction.DOWN, "H");
				yield return createWall(p + Direction.DOWN + Direction.LEFT, "H");
				break;
			default:
				throw new NotSupportedException();
		}
	}

	private static bool isBlockedBy(Link link, Wall wall, int w)
	{
		var from = Point.From(link.A, w);
		var to = Point.From(link.B, w);

		//Passing to right
		if (from.X < wall.X1 && to.X >= wall.X1 && from.Y >= wall.Y1 && to.Y < wall.Y2)
			return true;
		//Passing to left
		if (from.X >= wall.X1 && to.X < wall.X1 && from.Y >= wall.Y1 && to.Y < wall.Y2)
			return true;
		//Passing up
		if (from.X >= wall.X1 && to.X < wall.X2 && from.Y >= wall.Y1 && to.Y < wall.Y1)
			return true;
		//Passing down
		if (from.X >= wall.X1 && to.X < wall.X2 && from.Y < wall.Y1 && to.Y >= wall.Y1)
			return true;

		return false;
	}

	private static string commandFor(Wall wall)
	{
		return wall.Command();
	}

	private static Wall createWall(Point location, string wallOrientation)
	{
		var wall = new Wall { X1 = location.X, Y1 = location.Y };
		switch (wallOrientation)
		{
			case "V":
				wall.X2 = wall.X1;
				wall.Y2 = wall.Y1 + 2;
				break;
			case "H":
				wall.X2 = wall.X1 + 2;
				wall.Y2 = wall.Y1;
				break;
			default:
				throw new NotSupportedException();
		}
		return wall;
	}

	private static int[] pathFor(List<Link> map, Point player, Point[] exits, int mapWidth)
	{
		if (!player.IsRunning)
			return new int[0];

		var SI = positionOf(player.X, player.Y, mapWidth);
		var algorithm = new Dijkstra(map);
		int[] shortestPath = null;
		foreach (var exit in exits)
		{
			var exitPosition = positionOf(exit.X, exit.Y, mapWidth);
			var path = algorithm.Path(SI, exitPosition);
			if (shortestPath == null || shortestPath.Length > path.Length)
				shortestPath = path;
		}
		//if (shortestPath == null)
		//{
		//	Console.Error.WriteLine("There is NO path to any exit!");
		//}
		//else
		//{
		//	Console.Error.WriteLine("Shortest path to an exit is: " + string.Join(", ", shortestPath));
		//}
		return shortestPath;
	}

	private static Point[] exitsForPlayer(int playerId, int mapWidth, int mapHeight)
	{
		var isRunningHorizontally = playerId != 2;
		var exitIndex = !isRunningHorizontally
			? mapHeight - 1
			: (playerId == 0
				? mapWidth - 1
				: 0);

		var exits = isRunningHorizontally
			? Enumerable.Repeat(0, mapHeight).Select((x, index) => new Point { X = exitIndex, Y = index }).ToArray()
			: Enumerable.Repeat(0, mapWidth).Select((y, index) => new Point { X = index, Y = exitIndex }).ToArray();
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

public class Area
{
	public int X1 { get; set; }
	public int Y1 { get; set; }
	public int X2 { get; set; }
	public int Y2 { get; set; }
}
public class Wall : Area
{
	//public Direction Heading
	//{
	//	get
	//	{
	//		if (X1 < X2 && Y1 == Y2)
	//			return Direction.LEFT;
	//		else if (X1 == X2 && Y1 < Y2)
	//			return Direction.UP;
	//		else if (X1 > X2 && Y1 == Y2)
	//			return Direction.RIGHT;
	//		else if (X1 == X2 && Y1 > Y2)
	//			return Direction.DOWN;
	//		else
	//		{
	//			throw new NotSupportedException();
	//		}
	//	}
	//}

	bool IsHorizonal { get { return X1 != X2 && Y1 == Y2; } }
	bool IsVertical { get { return X1 == X2 && Y1 != Y2; } }

	public string Command()
	{
		return X1 + " " + Y1 + " " + (this.IsVertical ? "V" : "H");
	}

	public override string ToString()
	{
		return string.Format("({0}, {1}) - ({2}, {3})", X1, Y1, X2, Y2);
	}

	internal bool Overlaps(Wall wall)
	{
		if(this.IsHorizonal && wall.IsHorizonal)
		{
			//Both horizontal
			return (Y1 == wall.Y1 && X2 > wall.X1 && X1 < wall.X2);
		}
		else if (this.IsVertical && wall.IsVertical)
		{
			//Both vertical
			return (X1 == wall.X1 && Y2 > wall.Y1 && Y1 < wall.Y2);
		}
		else
		{
			//Crossing
			var overLapsInX = (X2 > wall.X1 && X1 < wall.X2);
			var overLapsInY = (Y2 > wall.Y1 && Y1 < wall.Y2);
			return overLapsInX && overLapsInY;
		}
	}

	internal bool Inside(Area area)
	{
		return X1 >= area.X1 && X2 < area.X2 && Y1 >= area.Y1 && Y2 <= area.Y2;
	}
}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }
	public int WallsLeft { get; set; }
	public bool IsRunning { get { return X >= 0 && Y >= 0; } }

	public Point() { }

	public Point(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	public static Point From(int position, int mapWidth)
	{
		var x = position % mapWidth;
		var y = position / mapWidth;
		return new Point { X = x, Y = y };
	}

	public static Point operator +(Point point, Direction direction)
	{
		switch (direction)
		{
			case Direction.UP: return new Point(point.X, point.Y - 1);
			case Direction.RIGHT: return new Point(point.X + 1, point.Y);
			case Direction.DOWN: return new Point(point.X, point.Y + 1);
			case Direction.LEFT: return new Point(point.X - 1, point.Y);
			default:
				throw new NotSupportedException();
		}
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