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
		var links = new List<Tuple<string, string>>();
		int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
		for (int i = 0; i < n; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			var xi = inputs[0]; // the ID of a person which is adjacent to yi
			var yi = inputs[1]; // the ID of a person which is adjacent to xi
			links.Add(new Tuple<string, string>(xi, yi));
		}

		var tree = buildTree(links);
		var dijkstra = new Dijkstra(tree);

		//foreach (var node in tree.Values)
		//	Console.Error.WriteLine(node);

		//var persons = links.SelectMany(x => new[] { x.Item1, x.Item2 }).Distinct().ToArray();
		var persons = tree.Keys.ToArray();
		//Console.Error.WriteLine("All persons: " + string.Join(", ", persons));

		var longestPath = 0;
		var firstPerson = "";
		for (int i = 1; i < persons.Length; i++)
		{
			var l = dijkstra.Path(persons[0], persons[i]).Length;
			Console.Error.WriteLine(string.Format("{0}->{1} : {2} steps", persons[0], persons[i], l));
			if (l > longestPath)
			{
				longestPath = l;
				firstPerson = persons[i];
			}
		}


		var secondPerson = "";
		for (int i = 0; i < persons.Length; i++)
		{
			if (persons[i] == firstPerson)
				continue;

			var l = dijkstra.Path(firstPerson, persons[i]).Length;
			if (l > longestPath)
			{
				longestPath = l;
				secondPerson = persons[i];
			}
		}

		Console.WriteLine((longestPath/2).ToString()); // The minimal amount of steps required to completely propagate the advertisement
		Console.ReadLine();
	}

	private static Dictionary<string, Node> buildTree(IEnumerable<Tuple<string, string>> links)
	{
		var tree = links
			.SelectMany(x => new[] { x.Item1, x.Item2 })
			.Distinct()
			.Select(name => new Node(name))
			.ToDictionary(x => x.Name);

		foreach (var node in tree.Values)
		{
			node.Neighbours = links.Where(tuple => tuple.Item1 == node.Name).Select(tuple => tuple.Item2)
					.Union(links.Where(tuple => tuple.Item2 == node.Name).Select(tuple => tuple.Item1))
					.Select(name => tree[name])
					.ToArray();
		}

		return tree;
	}

	class Node
	{
		public Node(string name)
		{
			Name = name;
			Distance = int.MaxValue;
		}

		public string Name { get; private set; }
		public Node[] Neighbours { get; set; }
		public string ShortestPath { get; set; }
		public int Distance { get; set; }
		public bool Visited { get; set; }

		public override string ToString()
		{
			return Name + ": " + string.Join(", ", Neighbours.Select(x => x.Name).ToArray());
		}
	}



	//class BreadthFirstSearch
	//{

	//}


	class Dijkstra
	{
		IDictionary<string, Node> _nodes;

		public Dijkstra(IDictionary<string, Node> nodes)
		{
			_nodes = nodes;
		}

		void clearTree()
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

			clearTree();

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