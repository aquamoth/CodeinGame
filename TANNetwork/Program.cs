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
		build(links, addresses);
		var bfs = new Dijkstra(addresses);
		var path = bfs.Path(startPoint, endPoint);

		if (path == null)
		{
			Console.WriteLine("IMPOSSIBLE");
		}
		else
		{
			var addressDictionary = addresses.ToDictionary(x => x.Name);
			foreach (var stopArea in path)
			{
				var address = addressDictionary[stopArea];
				Console.WriteLine(address.Fullname);
			}
		}

	}

	private static void build(Link[] links, Address[] addresses)
	{
		var nodes = addresses.ToDictionary(x => x.Name);

		foreach (var link in links.GroupBy(x=>x.A))
		{
			var node = nodes[link.Key];
			node.Neighbours = link.Select(x => nodes[x.B]).ToArray();
		}
	}
}

public class Address : Dijkstra.Node
{
	public string Fullname { get; set; }
	public double Longitude { get; set; }
	public double Latitude { get; set; }

	public Address(string addressLine)
	{
		var parts = addressLine.Split(new[] { ',' });
		this.Name = parts[0].Substring(9);
		Fullname = parts[1].Substring(1, parts[1].Length - 2);

		Latitude = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
		Longitude = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
	}

	public override double DistanceTo(Dijkstra.Node neighbour)
	{
		var node2 = neighbour as Address;



		var x = radians(node2.Longitude - this.Longitude) * Math.Cos(radians(this.Latitude + node2.Latitude) / 2);
		var y = radians(node2.Latitude - this.Latitude);
		var d = Math.Sqrt(x * x + y * y) * 6371;
		return d;
	}

	private double radians(double degrees)
	{
		return Math.PI / 180.0 * degrees;
	}
}


#region Helpers


public class Dijkstra
{
	public class Node
	{
		protected Node()
		{
			Distance = int.MaxValue;

		}

		public Node(string name)
			: base()
		{
			Name = name;
		}

		public string Name { get; protected set; }
		public Node[] Neighbours { get; set; }
		public string ShortestPath { get; set; }
		public double Distance { get; set; }
		public bool Visited { get; set; }

		public virtual double DistanceTo(Node neighbour)
		{
			return 1.0;
		}
	}

	IDictionary<string, Node> _nodes;

	public Dijkstra(IEnumerable<Node> nodes)
	{
		_nodes = nodes.ToDictionary(x => x.Name);
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
			var unvisitedNeighbours = currentNode.Neighbours.Where(x => !x.Visited);
			foreach (var neighbour in unvisitedNeighbours)
			{
				var tentativeDistance = currentNode.Distance + currentNode.DistanceTo(neighbour);
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

#endregion Helpers

