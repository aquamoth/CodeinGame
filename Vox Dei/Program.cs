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
	public const char ACTIVE_NODE = '@';
	public const char PASSIVE_NODE = '#';
	public const char EMPTY_POS = '.';

	public const char EMPTY_BLAST_3 = 'A';
	public const char EMPTY_BLAST_2 = 'B';
	public const char EMPTY_BLAST_1 = 'C';

	public const char ACTIVE_BLAST_3 = '3';
	public const char ACTIVE_BLAST_2 = '2';
	public const char ACTIVE_BLAST_1 = '1';

	public const int BOMB_TIMEOUT = 3;
	public const int BLAST_RANGE = 3;


	static void Main(string[] args)
	{
		var map = new List<string>();

		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int width = int.Parse(inputs[0]); // width of the firewall grid
		int height = int.Parse(inputs[1]); // height of the firewall grid
		for (int y = 0; y < height; y++)
		{
			var line = Console.ReadLine();
			map.Add(line);
		}

		var voxdei = new VoxDei(width, height, map.ToArray());

		Queue<Point> solution = null;
		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int rounds = int.Parse(inputs[0]); // number of rounds left before the end of the game
			int bombs = int.Parse(inputs[1]); // number of bombs left

			if (solution == null)
				solution = new Queue<Point>(voxdei.Solve(rounds, bombs));

			if (!solution.Any())
				Console.WriteLine("WAIT");
			else
			{
				var bomb = solution.Dequeue();
				if (bomb == null)
					Console.WriteLine("WAIT");
				else
					Console.WriteLine(bomb);
			}

			//currentRound++;
		}

	}
}

class VoxDei
{
	int _width, _height;
	char[] _map;

	public VoxDei(int width, int height, string[] map)
	{
		_width = width;
		_height = height;
		_map = map.SelectMany(x => x.ToCharArray()).ToArray();
	}

	public Point[] Solve(int rounds, int bombs)
	{
		var solution = solve(rounds, bombs, (char[])_map.Clone(), null);
		if (solution == null)
		{
			throw new ApplicationException("Unable to find a kill solution!");
		}
		return solution;
	}

	Point[] solve(int rounds, int bombs, char[] map, Point[] pointsNotToTest)
	{
		var nodesLeft = activeNodesOf(map, _width);

		if (nodesLeft.Length == 0)
			return new Point[0];

		if (rounds < Player.BOMB_TIMEOUT || bombs == 0)
			return null;

		//Decrease bomb timers
		decreaseBombTimers(map);

		//Try to kill any of the remaining nodes on this round
		var testedBombPoints = new List<Point>(pointsNotToTest ?? new Point[0]);
		foreach (var nextNode in nodesLeft)
		{
			//Console.Error.WriteLine("Testing round {0}/{1} by killing: {2}", rounds, bombs, nextNode);

			//Try to kill it from any available kill position
			var killPointsToTest = kills(map, nextNode).Where(point => !testedBombPoints.Contains(point)).ToArray();
			var killEffects = killPointsToTest.Select(p =>
			{
				var killedNodes = nodesLeft.Where(node => {
					var killedPoints = kills(map, node);
					return killedPoints.Contains(p);
				}).ToList();
				return new Tuple<Point, List<Point>>(p, killedNodes);
			}).ToArray();

			var tuplesToTest = unique(killEffects);
			
			var InferiorkillPoints = killPointsToTest.Where(x => !tuplesToTest.Any(t => t.Item1 == x));
			testedBombPoints.AddRange(InferiorkillPoints);

			foreach (var tuple in tuplesToTest)
			{
				Console.Error.WriteLine("Testing round {0}/{1} at: {2}", rounds, bombs, tuple.Item1);
				var killedNodes = tuple.Item2;// nodesLeft.Where(node => kills(map, node).Contains(point)).ToList();
				//Console.Error.WriteLine("  Killing: {0}", string.Join(", ", killedNodes.Select(n => n.ToString()).ToArray()));
				var unaffectedNodes = nodesLeft.Except(killedNodes);
				//Console.Error.WriteLine("  Survivors: {0}", string.Join(", ", unaffectedNodes.Select(n => n.ToString()).ToArray()));

				var bombTimer = Player.ACTIVE_BLAST_3;//TODO
				var newMap = (char[])map.Clone();
				foreach (var node in killedNodes)
					newMap[node.Y * _width + node.X] = bombTimer;



				var points = solve(rounds - 1, bombs - 1, (char[])newMap.Clone(), testedBombPoints.ToArray());
				if (points != null)
				{
					var bombsUsed = points.Where(x => x != null).Count();
					if (bombsUsed >= bombs)
					{
						Console.Error.WriteLine("TOO MANY BOMBS WERE USED! Skipping solution!");
						return null;
					}

					var commands = new[] { tuple.Item1 }.Concat(points).ToArray();
					return commands;
				}
				else
				{
					testedBombPoints.Add(tuple.Item1);
				}
			}
		}

		//Finally try to just WAIT to next round (if there are bombs counting down)
		Console.Error.WriteLine("Trying to solve by WAITing on round {0}", rounds);
		var partialSolution = solve(rounds - 1, bombs, (char[])map.Clone(), testedBombPoints.ToArray());
		if (partialSolution != null)
		{
			var commands = new Point[] { null }.Concat(partialSolution).ToArray();
			return commands;
		}


		return null;
	}

	private Tuple<Point, List<Point>>[] unique(Tuple<Point, List<Point>>[] killPoints)
	{
		var uniqueEffects = new List<Tuple<Point, List<Point>>>();
		foreach (var killPoint in killPoints.OrderByDescending(t=>t.Item2.Count))
		{
			var isCoveredByExistingPoint = 
				uniqueEffects.Any(existingKillPoint =>
					killPoint.Item2.All(p=> existingKillPoint.Item2.Contains(p))
				);

			if (!isCoveredByExistingPoint)
			{
				//Console.Error.WriteLine("    + Adding test of {0}", killPoint.Item1);
				uniqueEffects.Add(killPoint);
			}
			else
			{
				//Console.Error.WriteLine("    - Skipping test of {0}", killPoint.Item1);
			}
		}
		return uniqueEffects.ToArray();
	}

	private Point[] activeNodesOf(char[] map, int width)
	{
		return map
			.Select((ch, index) => ch == Player.ACTIVE_NODE ? index : -1)
			.Where(index => index >= 0)
			.Select(index => new Point { X = index % width, Y = index / width })
			.ToArray();
	}

	private static void decreaseBombTimers(char[] map)
	{
		for (var i = 0; i < map.Length; i++)
		{
			if (map[i] == '3') map[i] = '2';
			else if (map[i] == '2') map[i] = '1';
			else if (map[i] == '1') map[i] = '0';
			else if (map[i] == '0') map[i] = '.';
		}
	}

	private IEnumerable<Point> kills(char[] map, Point point)
	{
		var minX = Math.Max(0, point.X - Player.BLAST_RANGE);
		for (int x = point.X - 1; x >= minX; x--)
		{
			var token = map[point.Y * _width + x];
			if (token == Player.PASSIVE_NODE)
				break;
			else if (token == Player.EMPTY_POS) //TODO: TRACK IF EMPTY POS IS IN A DECREASING BOMB BLAST
				yield return new Point { X = x, Y = point.Y };
		}

		var maxX = Math.Min(_width - 1, point.X + Player.BLAST_RANGE);
		for (int x = point.X+1; x <= maxX; x++)
		{
			var token = map[point.Y * _width + x];
			if (token == Player.PASSIVE_NODE)
				break;
			else if (token == Player.EMPTY_POS) //TODO: TRACK IF EMPTY POS IS IN A DECREASING BOMB BLAST
				yield return new Point { X = x, Y = point.Y };
		}

		var minY = Math.Max(0, point.Y - Player.BLAST_RANGE);
		for (int y = point.Y - 1; y >= minY; y--)
		{
			var token = map[y * _width + point.X];
			if (token == Player.PASSIVE_NODE)
				break;
			else if (token == Player.EMPTY_POS) //TODO: TRACK IF EMPTY POS IS IN A DECREASING BOMB BLAST
				yield return new Point { X = point.X, Y = y };
		}

		var maxY = Math.Min(_height - 1, point.Y + Player.BLAST_RANGE);
		for (int y = point.Y + 1; y <= maxY; y++)
		{
			var token = map[y * _width + point.X];
			if (token == Player.PASSIVE_NODE)
				break;
			else if (token == Player.EMPTY_POS) //TODO: TRACK IF EMPTY POS IS IN A DECREASING BOMB BLAST
				yield return new Point { X = point.X, Y = y };
		}
	}

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

class Node : Point
{
	public bool IsPassive { get; set; }

	public Node(int x, int y, bool isPassive)
	{
		X = x;
		Y = y;
		IsPassive = isPassive;
	}
}