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
		var nodes = new List<Node>();

		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int width = int.Parse(inputs[0]); // width of the firewall grid
		int height = int.Parse(inputs[1]); // height of the firewall grid
		for (int y = 0; y < height; y++)
		{
			var line = Console.ReadLine();
			for (int x = 0; x < width; x++)
			{
				if (line[x] == '@')
					nodes.Add(new Node(x, y, false));
				else if (line[x] == '#')
					nodes.Add(new Node(x, y, true));
			}
		}



		var map = new List<MapPoint>();
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var nodesInRange = nodes.Any(node => node.X == x && node.Y == y)
					? new Node[0]
					: nodes
						.Where(node => !node.IsPassive)
						.Where(node => node.InRangeOf(x, y))
						.ToArray();

				map.Add(new MapPoint(x, y, nodesInRange));
			}
		}

		map.Sort((a, b) => b.NodesInRange.Length.CompareTo(a.NodesInRange.Length));

		// game loop
		var currentRound = 0;
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int rounds = int.Parse(inputs[0]); // number of rounds left before the end of the game
			int bombs = int.Parse(inputs[1]); // number of bombs left

			if (bombs == 0)
				Console.WriteLine("WAIT");
			else
				Console.WriteLine("{0} {1}", map[currentRound].X, map[currentRound].Y);

			currentRound++;
		}
	}
}

class MapPoint
{
	public int X { get; set; }
	public int Y { get; set; }
	public Node[] NodesInRange { get; set; }

	public MapPoint(int x, int y, Node[] nodesInRange)
	{
		X = x;
		Y = y;
		NodesInRange = nodesInRange;
	}
}

class Node
{
	public int X { get; set; }
	public int Y { get; set; }
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
}