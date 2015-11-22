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


			var gatewayLinks = links.Where(x=>gateways.Intersect(new[]{x.Item1, x.Item2}).Any()).ToArray();
			var nodesInFrontOfGateways = gatewayLinks.SelectMany(x => new[] { x.Item1, x.Item2 }).Except(gateways)
				.Distinct();
			//foreach (var node in nodesInFrontOfGateways)
			//{
			//	Console.Error.WriteLine(node + " is in front of one or more gateways");
			//}

			var nodesWithLinks = nodesInFrontOfGateways.Select(node => new
			{
				Node = node,
				Links = links.Where(link =>
						link.Item1 == node && gateways.Contains(link.Item2)
					|| link.Item2 == node && gateways.Contains(link.Item1)
					).ToArray()
			}).ToArray();



			var scoredNodes = new List<Tuple<string, int, Tuple<string, string>[], double>>();
			foreach (var node in nodesWithLinks)
			{
				var algorithm = new Dijkstra(links);
				var path = algorithm.Path(SI, node.Node);

				if (path != null)
				{
					var score = path.Length == 0 
						? double.MaxValue 
						: node.Links.Count() / ((double)path.Length - 1);
					if (node.Links.Count() > 1)
						score += 1000;
					scoredNodes.Add(new Tuple<string, int, Tuple<string, string>[], double>(node.Node, path.Length, node.Links, score));
				}
			}

			foreach (var node in scoredNodes.OrderByDescending(x => x.Item4))
			{
				Console.Error.WriteLine(node.Item1 + " is " + node.Item2 + " steps away and has " + node.Item3.Length + " gateway links => Score = " + node.Item4);
			}
			//Tuple<string, double, Tuple<string, string>> mostCriticalNode = null;
			var mostCriticalNode = scoredNodes.OrderByDescending(x => x.Item4).First();

			//if (mostCriticalNode == null)
			//{
			//	Console.Error.WriteLine("No path left. I won!");
			//	Console.ReadLine();
			//	break;
			//}


			var linkToSevere = mostCriticalNode.Item3.First();
			Console.WriteLine(linkToSevere.Item1 + " " + linkToSevere.Item2);
			links.Remove(linkToSevere);
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