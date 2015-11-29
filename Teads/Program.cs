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
		//Console.Error.WriteLine("{0}\tRead input with {1} links", sw.ElapsedMilliseconds, n);

		//Console.Error.WriteLine("{0}\tBuilt tree", sw.ElapsedMilliseconds);

		sw.Start();
		var dijkstra = new Dijkstra(tree);
		Console.Error.WriteLine("{0}\tCreated Dijkstra", sw.ElapsedMilliseconds);

		var persons = tree.Keys.ToArray();
		var longestPath = 0;
		var firstPerson = "";
		var tempStart = persons[0];

		var sw2 = new Stopwatch();
		Console.Error.WriteLine(string.Format("{0}\tScanning {1} persons", sw.ElapsedMilliseconds, persons.Length));
		for (int i = 1; i < persons.Length; i++)
		{
			var testNode = persons[i];
			var l = tree[testNode].Distance;
			if (l == int.MaxValue)
			{
				sw2.Start();
				l = dijkstra.Path(tempStart, testNode) ?? int.MaxValue;//.Length;
				sw2.Stop();
			}
			if (l > longestPath)
			{
				longestPath = l;
				firstPerson = persons[i];
			}
		}
		Console.Error.WriteLine("{0}\tFound first person: {1}. Path() used {2} ms.", sw.ElapsedMilliseconds, firstPerson, sw2.ElapsedMilliseconds);

		dijkstra.Reset();
		Console.Error.WriteLine("{0}\tReset Dijkstra", sw.ElapsedMilliseconds);

		longestPath = 0;
		var secondPerson = "";
		for (int i = 0; i < persons.Length; i++)
		{
			var testNode = persons[i];
			var l = tree[testNode].Distance;
			if (l == int.MaxValue) l = dijkstra.Path(firstPerson, testNode) ?? int.MaxValue;//.Length;
			if (l > longestPath)
			{
				longestPath = l;
				secondPerson = persons[i];
			}
		}
		Console.Error.WriteLine("{0}\tFound second person: {1}", sw.ElapsedMilliseconds, secondPerson);

		Console.WriteLine((int)Math.Floor((longestPath + 1) / 2.0)); // The minimal amount of steps required to completely propagate the advertisement
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
				node.Visited = false;
			}
		}

		public int? Path(string from, string to)
		{
			if (!_nodes.ContainsKey(to))
				return null;//No paths to the destination at all

			if (!_nodes.ContainsKey(from))
				return null;//No paths from the source at all

			//Initialize the traversal
			var currentNode = _nodes[from];
			currentNode.Distance = 0;
			var unvisitedNodes = new List<Node>(_nodes.Values);

			var sw = new Stopwatch();
			sw.Start();
			var sw2 = new Stopwatch();
			var sw3 = new Stopwatch();
			var sw4 = new Stopwatch();
			do
			{
				var tentativeDistance = currentNode.Distance + 1;

				sw2.Start();
				foreach (var neighbour in currentNode.Neighbours.Where(x => !x.Visited && x.Distance > tentativeDistance))
				{
					neighbour.Distance = tentativeDistance;
				}
				sw2.Stop();

				currentNode.Visited = true;

				sw3.Start();
				unvisitedNodes.Remove(currentNode);
				sw3.Stop();

				if (currentNode.Name == to)
					break;

				sw4.Start();
				currentNode = unvisitedNodes.OrderBy(x => x.Distance).FirstOrDefault();
				sw4.Stop();
			}
			while (currentNode != null && currentNode.Distance != int.MaxValue);
			sw.Stop();
			Console.Error.WriteLine("\t{0}\tTotal Dijkstra time", sw.ElapsedMilliseconds);
			Console.Error.WriteLine("\t{0}\tSetting distances", sw2.ElapsedMilliseconds);
			Console.Error.WriteLine("\t{0}\tRemoving visited node", sw3.ElapsedMilliseconds);
			Console.Error.WriteLine("\t{0}\tMoving node pointer", sw4.ElapsedMilliseconds);

			//Determine output
			var toNode = _nodes[to];
			if (toNode.Distance == int.MaxValue)
				return null; // No path to this gateway exists
			else
				return currentNode.Distance;
		}
	}

}