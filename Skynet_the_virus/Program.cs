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

			var shortestPaths = new List<string[]>();
			foreach (var gateway in gateways)
			{
				var algorithm = new Dijkstra(links);
				var path = algorithm.Path(SI, gateway);
				Console.Error.Write("Shortest path to gateway " + gateway + ": ");
				Console.Error.WriteLine(path == null ? "(none)" : string.Join(", ", path));
	
				if (path != null)
				{
					if (shortestPaths.Count == 0 || path.Length < shortestPaths.First().Length)
					{
						shortestPaths.Clear();
						shortestPaths.Add(path);
					}
					else if (path.Length == shortestPaths.First().Length)
					{
						shortestPaths.Add(path);
					}
				}
			}

			if (!shortestPaths.Any())
			{
				Console.Error.WriteLine("No path left. I won!");
				Console.ReadLine();
				break;
			}


			var linksToChooseFrom = shortestPaths.Select(path => path.Reverse().Take(2).Reverse()).ToArray();
			foreach (var link in linksToChooseFrom)
			{
				Console.Error.WriteLine("Possible link: " + string.Join(" - ", link));
			}
			var linkToSevere = linksToChooseFrom.GroupBy(link => link.First()).OrderByDescending(grp => grp.Count()).First().First();
			//var linkToSevere = linksToChooseFrom.First();


			Console.WriteLine(string.Join(" ", linkToSevere));
			links.Remove(new Tuple<string, string>(linkToSevere.First(), linkToSevere.Last()));
			links.Remove(new Tuple<string, string>(linkToSevere.Last(), linkToSevere.First()));
		}
	}



	class Dijkstra
	{
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
		}

		IDictionary<string, Node> _nodes;

		public Dijkstra(IEnumerable<Tuple<string, string>> links)
		{
			_nodes = links
				.SelectMany(x => new[] { x.Item1, x.Item2 })
				.Distinct()
				.Select(name => new Node(name))
				.ToDictionary(x => x.Name);

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