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
class Solution
{
	static void Main(string[] args)
	{
		var sw = new Stopwatch();
		sw.Start();

		var tree = new Dictionary<string, Node>();
		int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
		for (int i = 0; i < n; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			var xi = inputs[0]; // the ID of a person which is adjacent to yi
			var yi = inputs[1]; // the ID of a person which is adjacent to xi
			var nodex = ensureNodeInTree(tree, xi);
			var nodey = ensureNodeInTree(tree, yi);
			nodex.Neighbours.Add(nodey);
			nodey.Neighbours.Add(nodex);
		}
		Console.Error.WriteLine("{0}\tRead input with {1} links", sw.ElapsedMilliseconds, n);

		Console.Error.WriteLine("{0}\tBuilt tree", sw.ElapsedMilliseconds);

		var dijkstra = new Dijkstra(tree);
		Console.Error.WriteLine("{0}\tCreated Dijkstra", sw.ElapsedMilliseconds);

		var persons = tree.Keys.ToArray();
		var longestPath = 0;
		var firstPerson = "";
		var tempStart = persons[0];


		Console.Error.WriteLine(string.Format("Scanning {0} persons", persons.Length));
		for (int i = 1; i < persons.Length; i++)
		{
			var testNode = persons[i];
			var l = tree[testNode].Distance;
			if (l == int.MaxValue) l = dijkstra.Path(tempStart, testNode).Length;
			if (l > longestPath)
			{
				longestPath = l;
				firstPerson = persons[i];
			}
		}
		Console.Error.WriteLine("{0}\tFound first person", sw.ElapsedMilliseconds);

		dijkstra.Reset();
		Console.Error.WriteLine("{0}\tReset Dijkstra", sw.ElapsedMilliseconds);
		//var secondPerson = "";
		for (int i = 0; i < persons.Length; i++)
		{
			var testNode = persons[i];
			var l = tree[testNode].Distance;
			if (l == int.MaxValue) l = dijkstra.Path(firstPerson, testNode).Length;
			if (l > longestPath)
			{
				longestPath = l;
				//secondPerson = persons[i];
			}
		}
		Console.Error.WriteLine("{0}\tFound second person", sw.ElapsedMilliseconds);

		Console.WriteLine((longestPath / 2).ToString()); // The minimal amount of steps required to completely propagate the advertisement
		//Console.ReadLine();
	}

	private static Node ensureNodeInTree(Dictionary<string, Node> tree, string xi)
	{
		Node nodex;
		if (tree.ContainsKey(xi))
			nodex = tree[xi];
		else
		{
			nodex = new Node(xi);
			tree.Add(xi, nodex);
		}
		return nodex;
	}

	class Node
	{
		public Node(string name)
		{
			Name = name;
			Distance = int.MaxValue;
			Neighbours = new List<Node>();
		}

		public string Name { get; private set; }
		public List<Node> Neighbours { get; private set; }
		public string ShortestPath { get; set; }
		public int Distance { get; set; }
		public bool Visited { get; set; }

		public override string ToString()
		{
			return Name + ": " + string.Join(", ", Neighbours.Select(x => x.Name).ToArray());
		}
	}


	class Dijkstra
	{
		IDictionary<string, Node> _nodes;

		public Dijkstra(IDictionary<string, Node> nodes)
		{
			_nodes = nodes;
		}

		public void Reset()
		{
			foreach (var node in _nodes.Values)
			{
				node.Distance = int.MaxValue;
				node.ShortestPath = null;
				node.Visited = false;
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

}