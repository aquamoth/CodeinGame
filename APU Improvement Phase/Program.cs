using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * The machines are gaining ground. Time to show them what we're really made of...
 **/
class Player
{
	static void Main(string[] args)
	{
		var nodes = new List<Node>();

		int width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
		//Console.Error.WriteLine(width);
		int height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis
		//Console.Error.WriteLine(height);
		for (int y = 0; y < height; y++)
		{
			var l = Console.ReadLine(); // width characters, each either a number or a '.'
			//Console.Error.WriteLine(l);
			for (int x = 0; x < l.Length; x++)
			{
				if (l[x] != '.')
				{
					var connections = int.Parse(l[x].ToString());
					nodes.Add(new Node { X = x, Y = y, RequiredConnections = connections });
				}
			}
		}
		Console.Error.WriteLine("Solving");

		if (solve(nodes))
		{
			foreach (var node in nodes)
			{
				foreach(var link in node.Links.ToArray())
				{
					Console.WriteLine("{0} {1} {2} {3} {4}", node.X, node.Y, link.X, link.Y, 1);
					link.Links.Remove(node);
					node.Links.Remove(link);
				}
			}
		}
		else
		{
			throw new ApplicationException("No possible solutions");
		}

		


		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		//Console.WriteLine("0 0 2 0 1"); // Two coordinates and one integer: a node, one of its neighbors, the number of links connecting them.
	}

	private static bool solve(List<Node> nodes)
	{
		Console.Error.WriteLine("Solving all single solution parts");
		bool runAgain;
		do
		{
			runAgain = false;
			foreach (var node in nodes)
			{
				var targets = singleSolution(node, nodes);
				if (targets != null)
				{
					foreach (var target in targets)
					{
						attach(node, target.Item1);
						if (target.Item2 == 2)
							attach(node, target.Item1);
					}
					runAgain = true;
				}
			}
		} while (runAgain);

		Console.Error.WriteLine("Trying to solve complex solutions by brute force");
		nodes.Sort((x, y) => y.MissingLinks.CompareTo(x.MissingLinks));
		return solve(nodes, 0);
	}

	private static IEnumerable<Tuple<Node, int>> singleSolution(Node node, IEnumerable<Node> nodes)
	{
		if (node.MissingLinks == 0)
			return null;

		var availableLinks = targetsFor(node, nodes).ToArray();
		if (availableLinks.Count() == 1)
		{
			var target = availableLinks[0].Item1;
			var count = Math.Min(node.MissingLinks, availableLinks[0].Item2);
			return new[] { new Tuple<Node, int>(target, count) };
		}
		else if (availableLinks.Sum(x => x.Item2) == node.MissingLinks)
			return availableLinks;
		else
			return null;
	}

	private static bool solve(List<Node> nodes, int currentIndex)
	{
		if (currentIndex >= nodes.Count)
			return true;

		var node = nodes[currentIndex];
		Console.Error.WriteLine("Solving for #" + currentIndex + ": " + node);
		if (node.MissingLinks == 0)
		{
			return solve(nodes, currentIndex + 1);
		}
		else
		{
			var targets = adjacentNodes(node, nodes).ToArray();
			Console.Error.WriteLine("Adjacents: " + string.Join(", ", targets.Select(x => x.ToString()).ToArray()));
			foreach (var target in targets)
			{
				if (target.MissingLinks > 0)
				{
					if (target.Links.Where(l => l == node).Count() < 2)
					{
						attach(node, target);

						var nextIndex = currentIndex + (node.MissingLinks == 0 ? 1 : 0);
						if (solve(nodes, currentIndex))
							return true;

						Console.Error.WriteLine("Cleaning up link between " + node + " and " + target);
						target.Links.Remove(node);
						node.Links.Remove(target);
					}
				}
			}

			return false;
		}
	}

	private static void attach(Node node, Node target)
	{
		Console.Error.WriteLine("Adding link between " + node + " and " + target);
		target.Links.Add(node);
		node.Links.Add(target);
	}


	private static IEnumerable<Tuple<Node, int>> targetsFor(Node node, IEnumerable<Node> enumerable)
	{
		return adjacentNodes(node, enumerable)
			.Select(target => new Tuple<Node, int>(target, Math.Max(2, target.MissingLinks)))
			.Where(tuple => tuple.Item2 > 0);
	}

	private static IEnumerable<Node> adjacentNodes(Node node, IEnumerable<Node> enumerable)
	{
		var onX = enumerable.Where(e => e.X == node.X).OrderBy(e => e.Y);
		var onY = enumerable.Where(e => e.Y == node.Y).OrderBy(e => e.X);
		var above = onX.Where(e => e.Y < node.Y).LastOrDefault();
		var below = onX.Where(e => e.Y > node.Y).FirstOrDefault();
		var toLeft = onY.Where(e => e.X < node.X).LastOrDefault();
		var toRight = onY.Where(e => e.X > node.X).FirstOrDefault();
		return new[] { above, below, toLeft, toRight }.Where(e => e != null);
	}
}

public class Node
{
	public int X { get; set; }
	public int Y { get; set; }
	public int RequiredConnections { get; set; }
	public List<Node> Links { get; private set; }
	public int MissingLinks { get { return RequiredConnections - Links.Count; } }

	public Node()
	{
		Links = new List<Node>();
	}

	public override string ToString()
	{
		return string.Format("({0}, {1}) = {2}", X, Y, RequiredConnections);
	}
}