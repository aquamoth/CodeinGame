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
		//Console.Error.WriteLine("Number of Nodes, Links, Gateways");
		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
		int L = int.Parse(inputs[1]); // the number of links
		int E = int.Parse(inputs[2]); // the number of exit gateways

		//Console.Error.WriteLine("All links, one per line");
		var links = new List<Tuple<string, string>>();
		for (int i = 0; i < L; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			links.Add(new Tuple<string, string>(inputs[0], inputs[1]));
		}

		//Console.Error.WriteLine("All gateway node indexes, one per line");
		var gateways = new string[E];
		for (int i = 0; i < E; i++)
		{
			gateways[i] = Console.ReadLine(); // the index of a gateway node
		}

		// game loop
		while (true)
		{
			//Console.Error.WriteLine("Current SI position");
			var SI = Console.ReadLine(); // The index of the node on which the Skynet agent is positioned this turn

			string[] shortestPath = null;
			foreach (var gateway in gateways)
			{
				var algorithm = new Dijkstra(links);
				var path = algorithm.Path(SI, gateway);
				Console.Error.Write("Shortest path to gateway " + gateway + ": ");
				Console.Error.WriteLine(path == null ? "(none)" : string.Join(", ", path));
	
				if (path != null)
				{
					if (shortestPath == null || path.Length < shortestPath.Length)
						shortestPath = path;
				}
			}

			if (shortestPath == null)
			{
				Console.Error.WriteLine("No path left. I won!");
				break;
			}

			var linkToSevere = shortestPath.Reverse().Take(2).Reverse();
			Console.WriteLine(string.Join(" ", linkToSevere));

			links.Remove(new Tuple<string, string>(linkToSevere.First(), linkToSevere.Last()));
			links.Remove(new Tuple<string, string>(linkToSevere.Last(), linkToSevere.First()));
		}
	}



	class Dijkstra
	{
		class Node
		{
			public string Name { get; set; }
			public Node[] Neighbours { get; set; }
			public string ShortestPath { get; set; }
			public int? LowestCost { get; set; }
			public bool Visited { get; set; }
		}

		IDictionary<string, Node> _nodes;

		public Dijkstra(IEnumerable<Tuple<string, string>> links)
		{
			_nodes = links
				.SelectMany(x => new[] { x.Item1, x.Item2 })
				.Distinct()
				.Select(name => new Node { Name = name })
				.ToDictionary(x=>x.Name);

			foreach (var node in _nodes.Values)
			{
				node.Neighbours = links.Where(tuple => tuple.Item1 == node.Name).Select(tuple => tuple.Item2)
						.Union(links.Where(tuple => tuple.Item2 == node.Name).Select(tuple => tuple.Item1))
						.Select(name => _nodes[name])
						.ToArray();
			}

		}

		public string[] Path(string from, string to)
		{
			var currentNode = _nodes[from];
			currentNode.LowestCost = 0;

			bool isDebug = from == "7";

			while (currentNode.Name != to)
			{
				var nextCost = currentNode.LowestCost + 1;
				var neighbours = currentNode.Neighbours.Where(x => !x.Visited);

				if (isDebug)
				{
					Console.Error.WriteLine("Node: " + currentNode.Name);
					Console.Error.WriteLine("Neighbours: " + string.Join(", ", neighbours.Select(x => x.Name).ToArray()));
				}

				foreach (var neighbour in neighbours)
				{
					if (!neighbour.LowestCost.HasValue || neighbour.LowestCost.Value > nextCost)
					{
						neighbour.LowestCost = nextCost;
						neighbour.ShortestPath = currentNode.ShortestPath + " " + currentNode.Name;
					}
				}
				currentNode.Visited = true;

				currentNode = neighbours.OrderBy(x => x.LowestCost).FirstOrDefault();
				if (currentNode == null)
					return null;//No path to this gateway exists
			}

			return (currentNode.ShortestPath + " " + currentNode.Name).TrimStart().Split(' ');
		}
	}
}