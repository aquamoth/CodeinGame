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
class Solution
{
	static void Main(string[] args)
	{
		int L = int.Parse(Console.ReadLine());
		int H = int.Parse(Console.ReadLine());

		var map = new List<Node[]>();
		for (int y = 0; y < H; y++)
		{
			var line = Console.ReadLine();
			var nodes = line.Select((c, x) => new Node(x, y, c == 'O')).ToArray();
			map.Add(nodes);
		}

		var coordinates = new List<Point>();
		int N = int.Parse(Console.ReadLine());
		for (int i = 0; i < N; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int X = int.Parse(inputs[0]);
			int Y = int.Parse(inputs[1]);
			coordinates.Add(new Point(X, Y));
		}
		for (int i = 0; i < N; i++)
		{
			var coordinate = coordinates[i];
			var node = map[coordinate.Y][coordinate.X];
			if (node.IsHandled)
				Console.WriteLine(node.Area);
			else if (!node.IsSea)
				Console.WriteLine(0);
			else
			{
				var untestedNodes = new Queue<Node>();
				var bodyOfWater = new List<Node>();

				untestedNodes.Enqueue(node);
				node.IsHandled = true;
				do
				{
					var walker = untestedNodes.Dequeue();
					bodyOfWater.Add(walker);

					var neighbours = neighboursOf(walker, map).Where(n => !n.IsHandled && n.IsSea);
					foreach (var n in neighbours)
					{
						untestedNodes.Enqueue(n);
						n.IsHandled = true;
					}
				}
				while (untestedNodes.Any());

				var area = bodyOfWater.Count;
				foreach (var n in bodyOfWater)
					n.Area = area;

				Console.WriteLine(node.Area);
			}
		}
	}

	private static IEnumerable<Node> neighboursOf(Node walker, List<Node[]> map)
	{
		if (walker.X > 0)
			yield return map[walker.Y][walker.X - 1];
		if (walker.Y > 0)
			yield return map[walker.Y - 1][walker.X];
		if (walker.X < map[0].Length - 1)
			yield return map[walker.Y][walker.X + 1];
		if (walker.Y < map.Count - 1)
			yield return map[walker.Y + 1][walker.X];
	}




	class Node : Point
	{
		public bool IsSea { get; protected set; }
		public bool IsHandled { get; set; }
		public int Area { get; set; }

		public Node(int x, int y, bool isSea)
			: base(x, y)
		{
			this.IsSea = isSea;
		}
	}

	class Point
	{
		public int X { get; protected set; }
		public int Y { get; protected set; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}
	}
}