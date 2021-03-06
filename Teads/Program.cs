﻿using System;
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
		//var sw = new Stopwatch();

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

		var persons = tree.Keys.ToArray();
		var longestPath = 0;
		var firstPerson = "";
		var tempStart = persons[0];
		var bfs = new Dijkstra(tree, tempStart);
		//Console.Error.WriteLine("{0}\tCreated Dijkstra", sw.ElapsedMilliseconds);

		//Console.Error.WriteLine(string.Format("{0}\tScanning {1} persons", sw.ElapsedMilliseconds, persons.Length));
		for (int i = 1; i < persons.Length; i++)
		{
			var l = bfs.Path(persons[i]);
			if (!l.HasValue)
			{
				Console.Error.WriteLine(string.Format("No path between {0} and {1}", bfs.From, persons[i]));
			}
			else if (l > longestPath)
			{
				longestPath = l.Value;
				firstPerson = persons[i];
			}
		}
		//Console.Error.WriteLine("{0}\tFound first person: {1}. Path() used {2} ms.", sw.ElapsedMilliseconds, firstPerson, sw2.ElapsedMilliseconds);

		longestPath = 0;
		var secondPerson = "";
		bfs.Reset(firstPerson);
		//Console.Error.WriteLine("{0}\tReset Dijkstra", sw.ElapsedMilliseconds);
		for (int i = 0; i < persons.Length; i++)
		{
			var l = bfs.Path(persons[i]);
			if (!l.HasValue)
			{
				Console.Error.WriteLine(string.Format("No path between {0} and {1}", bfs.From, persons[i]));
			}
			else if (l > longestPath)
			{
				longestPath = l.Value;
				secondPerson = persons[i];
			}
		}
		//Console.Error.WriteLine("{0}\tFound second person: {1}", sw.ElapsedMilliseconds, secondPerson);

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
		public string From { get; private set;}
		Queue<Node> unvisitedNodes = new Queue<Node>();

		public Dijkstra(IDictionary<string, Node> nodes, string from)
		{
			_nodes = nodes;
			Reset(from);
		}

		public void Reset(string from)
		{
			foreach (var node in _nodes.Values)
			{
				node.Distance = int.MaxValue;
				node.Visited = false;
			}
			unvisitedNodes.Clear();

			this.From = from;
			_nodes[from].Distance = 0;
			unvisitedNodes.Enqueue(_nodes[from]);
		}

		public int? Path(string to)
		{
			if (!_nodes.ContainsKey(to))
				return null;//No paths to the destination at all

			if (_nodes[to].Distance != int.MaxValue)
				return _nodes[to].Distance;

			while (unvisitedNodes.Any())
			{
				var currentNode = unvisitedNodes.Dequeue();
				currentNode.Visited = true;
				var tentativeDistance = currentNode.Distance + 1;

				foreach (var neighbour in currentNode.Neighbours.Where(x => !x.Visited))
				{
					if (neighbour.Distance > tentativeDistance)
						neighbour.Distance = tentativeDistance;
					unvisitedNodes.Enqueue(neighbour);
				}
				if (currentNode.Name == to)
					break;
			}

			//Determine output
			var toNode = _nodes[to];
			if (toNode.Distance == int.MaxValue)
				return null; // No path to this gateway exists
			else
				return toNode.Distance;
		}
	}

}