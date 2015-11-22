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
		//Console.Error.WriteLine(string.Join(" ", inputs));
		int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
		int L = int.Parse(inputs[1]); // the number of links
		int E = int.Parse(inputs[2]); // the number of exit gateways

		//Console.Error.WriteLine("All links, one per line");
		var links = new List<Link>();
		for (int i = 0; i < L; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			//Console.Error.WriteLine(string.Join(" ", inputs));
			links.Add(new Link { A = inputs[0], B = inputs[1] });
		}

		//Console.Error.WriteLine("All gateway node indexes, one per line");
		var gateways = new string[E];
		for (int i = 0; i < E; i++)
		{
			gateways[i] = Console.ReadLine(); // the index of a gateway node
			//Console.Error.WriteLine(gateways[i]);
		}

		// game loop
		while (true)
		{
			//Console.Error.WriteLine("Current SI position");
			var SI = Console.ReadLine(); // The index of the node on which the Skynet agent is positioned this turn
			//Console.Error.WriteLine(SI);
			//Console.Error.WriteLine("\nINPUT READ.");

			var gatewayLinks = links.Where(link => gateways.Intersect(link.Nodes).Any()).ToArray();
			var namesOfNodesToGateways = gatewayLinks.SelectMany(link => link.Nodes).Except(gateways).Distinct().ToArray();
			//foreach (var node in nodesInFrontOfGateways)
			//{
			//	Console.Error.WriteLine(node + " is in front of one or more gateways");
			//}

			var nodeToGateways = namesOfNodesToGateways.Select(node => new Node
			{
				Name = node,
				Links = links.Where(link =>
						link.A == node && gateways.Contains(link.B)
					|| link.B == node && gateways.Contains(link.A)
					).ToArray(),
				Score = double.MaxValue
			}).ToArray();


			foreach (var node in nodeToGateways)
			{
				var algorithm = new Dijkstra(links);
				node.Path = algorithm.Path(SI, node.Name);
				var pathNodesWithoutLinksToGateways = node.Path == null 
					? 0 
					: node.Path.Where(nodeName => !namesOfNodesToGateways.Contains(nodeName)).Count();

				if (node.Path == null)
				{
					Console.Error.WriteLine("Node " + node + " has no path");
					node.Score = 0;
				}
				else if (node.Links.Count() == 1)
				{
					//Console.Error.WriteLine("Node " + node.Name + " links to only one gateway");
					node.Score = node.Path.Length == 1 ? double.MaxValue : 0;// 1.0 / node.Path.Length;
				}
				else
				{
					node.Score = pathNodesWithoutLinksToGateways == 0
						? double.MaxValue
						: (node.Links.Count() - 1) / (double)pathNodesWithoutLinksToGateways;
				}
				//Console.Error.WriteLine("Node " + node + " links to " + node.Links.Count() + " gateways, with " + pathNodesWithoutLinksToGateways + " chances to severe its links");
			}

			foreach (var node in nodeToGateways.Where(node => node.Path != null).OrderByDescending(x => x.Score))
			{
				var pathNodesWithoutLinksToGateways = node.Path == null
					? 0
					: node.Path.Where(nodeName => !namesOfNodesToGateways.Contains(nodeName)).Count();
				Console.Error.WriteLine("Node " + node + " links to " + node.Links.Count() + " gateways, with " + pathNodesWithoutLinksToGateways + " chances to severe its links");
				//Console.Error.WriteLine(node.Name + " is " + node.Path.Length + " steps away and has " + node.Links.Length + " gateway links => Score = " + node.Score);
			}

			if (nodeToGateways.Length == 0)
			{
				Console.Error.WriteLine("No gateways are reachable. I won!");
				return;
			}


			var mostCriticalNode = nodeToGateways
				.Where(node => node.Path != null)
				.OrderByDescending(x => x.Score)
				.First();
			var linkToSevere = mostCriticalNode.Links.First();

			Console.WriteLine(linkToSevere);
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

		public Dijkstra(IEnumerable<Link> links)
		{
			_nodes = links
				.SelectMany(link => link.Nodes)
				.Distinct()
				.Select(name => new Node(name))
				.ToDictionary(x => x.Name);

			foreach (var node in _nodes.Values)
			{
				node.Neighbours = links.Where(link => link.A == node.Name).Select(link => link.B)
						.Union(links.Where(link => link.B == node.Name).Select(link => link.A))
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

public class Link
{
	public string A { get; set; }
	public string B { get; set; }

	public string[] Nodes { get { return new[] { A, B }; } }

	public override string ToString()
	{
		return A + " " + B;
	}
}

public class Node
{
	public string Name { get; set; }
	public Link[] Links { get; set; }
	public string[] Path { get; set; }
	public double Score { get; set; }

	public override string ToString()
	{
		return string.Format("{0} ({1} pts)", Name, Score);
	}
}