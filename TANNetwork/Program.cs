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
		string startPoint = Console.ReadLine().Substring(9);
		string endPoint = Console.ReadLine().Substring(9);
		int N = int.Parse(Console.ReadLine());
		var addresses = new Address[N];
		for (int i = 0; i < N; i++)
		{
			string stopName = Console.ReadLine();
			addresses[i] = new Address(stopName);
		}
		int M = int.Parse(Console.ReadLine());
		var links = new Link[M];
		for (int i = 0; i < M; i++)
		{
			string route = Console.ReadLine();
			var nodes = route.Split(new[] { ' ' }, 2).Select(x => x.Substring(9)).ToArray();
			links[i] = new Link { A = nodes[0], B = nodes[1] };
		}

		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");
		var bfs = new Dijkstra(links);
		var path = bfs.Path(startPoint, endPoint);

		if (path == null)
		{
			Console.WriteLine("IMPOSSIBLE");
		}
		else
		{
			var addressDictionary = addresses.ToDictionary(x => x.StopArea);
			foreach (var stopArea in path)
			{
				var address = addressDictionary[stopArea];
				Console.WriteLine(address.Name);
			}
		}

	}
}

class Address
{
	public string StopArea { get; set; }
	public string Name { get; set; }

	public Address(string addressLine)
	{
		var parts = addressLine.Split(new[] { ',' });
		StopArea = parts[0].Substring(9);
		Name = parts[1].Substring(1, parts[1].Length - 2);
	}
}


#region Helpers


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

#endregion Helpers

