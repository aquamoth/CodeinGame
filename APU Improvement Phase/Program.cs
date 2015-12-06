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

		nodes.Sort((x, y) => y.RequiredConnections.CompareTo(x.RequiredConnections));

		if (solve(nodes, 0))
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

	private static bool solve(List<Node> nodes, int currentIndex)
	{
		if (currentIndex >= nodes.Count)
			return true;

		var node = nodes[currentIndex];
		Console.Error.WriteLine("Solving for #" + currentIndex + ": " + node);
		if (node.RequiredConnections == node.Links.Count)
		{
			return solve(nodes, currentIndex + 1);
		}
		else
		{
			var targets = adjacentNodes(node, nodes.Skip(currentIndex)).ToArray();
			Console.Error.WriteLine("Adjacents: " + string.Join(", ", targets.Select(x => x.ToString()).ToArray()));
			foreach (var target in targets)
			{
				if (target.RequiredConnections > target.Links.Count)
				{
					Console.Error.WriteLine("Adding link between " + node + " and " + target);
					target.Links.Add(node);
					node.Links.Add(target);

					var nextIndex = currentIndex + (node.RequiredConnections == node.Links.Count ? 1 : 0);
					if (solve(nodes, currentIndex))
						return true;

					Console.Error.WriteLine("Cleaning up link between " + node + " and " + target);
					target.Links.Remove(node);
					node.Links.Remove(target);
				}
			}

			return false;
		}
	}

	private static IEnumerable<Node> adjacentNodes(Node node, IEnumerable<Node> enumerable)
	{
		var onX = enumerable.Where(e => e.X == node.X).OrderBy(e=>e.Y);
		var onY = enumerable.Where(e => e.Y == node.Y).OrderBy(e=>e.X);
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

	public Node()
	{
		Links = new List<Node>();
	}

	public override string ToString()
	{
		return string.Format("({0}, {1}) = {2}", X, Y, RequiredConnections);
	}
}