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
		//assertions();

		var nodes = new List<Node>();

		int width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
		Console.Error.WriteLine(width);
		int height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis
		Console.Error.WriteLine(height);
		for (int y = 0; y < height; y++)
		{
			var l = Console.ReadLine(); // width characters, each either a number or a '.'
			Console.Error.WriteLine(l);
			for (int x = 0; x < l.Length; x++)
			{
				if (l[x] != '.')
				{
					var connections = int.Parse(l[x].ToString());
					nodes.Add(new Node { X = x, Y = y, RequiredConnections = connections });
				}
			}
		}
		//Console.Error.WriteLine("Map:");
		//foreach (var  node in nodes)
		//{
		//	Console.Error.WriteLine(node);
		//}

		Console.Error.WriteLine("Solving");
		if (!solve(nodes))
		{
			throw new ApplicationException("No possible solutions");
		}

		


		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		//Console.WriteLine("0 0 2 0 1"); // Two coordinates and one integer: a node, one of its neighbors, the number of links connecting them.
	}

	private static void assertions()
	{
		var testnodes = new List<Node> { 
			new Node { X = 0, Y = 0 }, 
			new Node { X = 1, Y = 0 }, 
			new Node { X = 2, Y = 0 }, 

			new Node { X = 0, Y = 1 }, 
			new Node { X = 1, Y = 1 }, 
			new Node { X = 2, Y = 1 }, 

			new Node { X = 0, Y = 2 }, 
			new Node { X = 1, Y = 2 }, 
			new Node { X = 2, Y = 2 }, 
		};
		attach(testnodes[3], testnodes[5]);
		var isTrue = crossesExistingLinks(testnodes[1], testnodes[7], testnodes);
		if (!isTrue)
			throw new ApplicationException();
	}

	private static void print(Node node, Node link, int count = 1)
	{
		Console.WriteLine("{0} {1} {2} {3} {4}", node.X, node.Y, link.X, link.Y, count);
	}

	private static bool solve(List<Node> nodes)
	{
		solveEssentialLinks(nodes);
		solveSingleSolutions(nodes);

		Console.Error.WriteLine("Trying to solve complex solutions by brute force");
		nodes.Sort((x, y) => y.MissingLinks.CompareTo(x.MissingLinks));
		return solveBruteforce(nodes, 0);
	}

	private static void solveEssentialLinks(List<Node> nodes)
	{
		Console.Error.WriteLine("Solving all essential links");
		foreach (var node in nodes)
		{
			var targets = targetsFor(node, nodes);
			var availableLinks = targets.Sum(x => x.Item2);
			if (node.MissingLinks == availableLinks)
			{
				attachAndPrint(node, targets, false);
			}
			else if (node.MissingLinks == availableLinks - 1)
			{
				attachAndPrint(node, targets.Where(x => x.Item2 == 2), true);
			}
		}
	}

	private static void solveSingleSolutions(List<Node> nodes)
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
					attachAndPrint(node, targets, false);
					runAgain = true;
				}
			}
		} while (runAgain);
	}

	private static void attachAndPrint(Node node, IEnumerable<Tuple<Node, int>> targets, bool onlyOneLinkPerTarget)
	{
		foreach (var target in targets)
		{
			attach(node, target.Item1);
			if (target.Item2 > 1 && !onlyOneLinkPerTarget)
				attach(node, target.Item1);
			print(node, target.Item1, onlyOneLinkPerTarget ? 1 : target.Item2);
		}
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

	private static bool solveBruteforce(List<Node> nodes, int currentIndex)
	{
		if (currentIndex >= nodes.Count)
			return true;

		var node = nodes[currentIndex];
		Console.Error.WriteLine("Solving for #" + currentIndex + ": " + node);
		if (node.MissingLinks == 0)
		{
			return solveBruteforce(nodes, currentIndex + 1);
		}
		else
		{
			var targets = targetsFor(node, nodes)
				.OrderBy(x => Math.Abs(x.Item1.X - node.X) + Math.Abs(x.Item1.Y - node.Y))
				.ToArray();
			//Console.Error.WriteLine("Targets: " + string.Join(", ", targets.Select(x => x.ToString()).ToArray()));
			foreach (var target in targets)
			{
				attach(node, target.Item1);

				if (!detectClosedLoop(node, nodes))
				{
					var nextIndex = currentIndex + (node.MissingLinks == 0 ? 1 : 0);
					if (solveBruteforce(nodes, currentIndex))
					{
						Player.print(node, target.Item1);
						return true;
					}
				}
				else
				{
					Console.Error.WriteLine("Detected a closed loop.");
				}

				Console.Error.WriteLine("Cleaning up link between " + node + " and " + target);
				target.Item1.Links.Remove(node);
				node.Links.Remove(target.Item1);
			}

			return false;
		}
	}

	private static bool detectClosedLoop(Node node, List<Node> nodes)
	{
		var testedNodes = new HashSet<Node>();
		var pendingNodes = new Queue<Node>();

		testedNodes.Add(node);
		pendingNodes.Enqueue(node);

		while (pendingNodes.Any())
		{
			var walker = pendingNodes.Dequeue();
			if (walker.MissingLinks > 0)
				return false; //There is at least one free link left, so no closed loop yet

			var nodesToTest = walker.Links.Where(l => !testedNodes.Contains(l));
			foreach (var link in nodesToTest)
			{
				testedNodes.Add(link);
				pendingNodes.Enqueue(link);
			}

			if (nodes.Count == testedNodes.Count)
				return false; //All nodes are connected, so there are no closed loops
		}

		return true; //Detected a closed loop!
	}

	private static void attach(Node node, Node target)
	{
		Console.Error.WriteLine("Adding link between " + node + " and " + target);
		target.Links.Add(node);
		node.Links.Add(target);
	}


	private static IEnumerable<Tuple<Node, int>> targetsFor(Node node, IEnumerable<Node> allNodes)
	{
		return adjacentNodes(node, allNodes)
			.Select(target =>
			{
				var existingLinks = target.Links.Where(l => l == node).Count();
				var linksLeft = Math.Min(2 - existingLinks, target.MissingLinks);
				if (linksLeft > 0 && crossesExistingLinks(node, target, allNodes))
					linksLeft = 0;
				return new Tuple<Node, int>(target, linksLeft);
			})
			.Where(tuple => tuple.Item2 > 0);
	}

	private static bool crossesExistingLinks(Node node, Node target, IEnumerable<Node> allNodes)
	{
		if (target.X == node.X)
		{
			var y1 = Math.Min(target.Y, node.Y);
			var y2 = Math.Max(target.Y, node.Y);
			var isTrue = allNodes.Where(n => n.Y > y1 && n.Y < y2)
				.SelectMany(n => n.Links.Select(l => new { Node = n, Link = l }))
				.Select(x => new { x1 = Math.Min(x.Node.X, x.Link.X), x2 = Math.Max(x.Node.X, x.Link.X) })
				.Where(x => x.x1 < node.X && x.x2 > node.X)
				.Any();
			if (isTrue)
			{
				Console.Error.WriteLine("Not testing link {0}-{1} because it crosses a horizontal link", node, target);
			}
			return isTrue;
		}
		else
		{
			var x1 = Math.Min(target.X, node.X);
			var x2 = Math.Max(target.X, node.X);
			var isTrue = allNodes.Where(n => n.X > x1 && n.X < x2)
				.SelectMany(n => n.Links.Select(l => new { Node = n, Link = l }))
				.Select(x => new { y1 = Math.Min(x.Node.Y, x.Link.Y), y2 = Math.Max(x.Node.Y, x.Link.Y) })
				.Where(x => x.y1 < node.Y && x.y2 > node.Y)
				.Any();
			if (isTrue)
			{
				Console.Error.WriteLine("Not testing link {0}-{1} because it crosses a vertical link", node, target);
			}
			return isTrue;
		}
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