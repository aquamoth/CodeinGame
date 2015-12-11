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


		build(links, addresses);
		var bfs = new Dijkstra(addresses, startPoint);
		var path = bfs.Path(endPoint);


		if (path == null)
		{
			Console.WriteLine("IMPOSSIBLE");
		}
		else
		{
			var addressDictionary = addresses.ToDictionary(x => x.Id);
			foreach (var stopArea in path)
			{
				var address = addressDictionary[stopArea];
				Console.WriteLine(address.Fullname);
			}
		}

	}

	private static void build(Link[] links, Address[] addresses)
	{
		var nodes = addresses.ToDictionary(x => x.Id);

		foreach (var link in links.GroupBy(x=>x.A))
		{
			var node = nodes[link.Key];
			node.Neighbours = link.Select(x => x.B).ToArray();
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
		this.Id = parts[0].Substring(9);
		Fullname = parts[1].Substring(1, parts[1].Length - 2);

		Latitude = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
		Longitude = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return string.Format("{0} ({1} from {2})", this.Id, this.Distance, this.From);
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
		public string Id { get; set; }
		public string[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public string From { get; set; }
		public double Distance { get; set; }

		public virtual double DistanceTo(Node node) { return 1.0; }
	}

	Dictionary<string, Node> _nodes;
	public string From { get; private set; }
	LinkedList<Node> unvisitedNodes = new LinkedList<Node>();

	public Dijkstra(Node[] zones, string from)
	{
		_nodes = zones.ToDictionary(x => x.Id);
		Reset(from);
	}

	public void Reset(string from)
	{
		foreach (var node in _nodes.Values)
		{
			node.From = null;
			node.Distance = double.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		_nodes[from].From = "";
		_nodes[from].Distance = 0;

		unvisitedNodes.AddFirst(_nodes[from]);
	}

	public string[] Path(string to)
	{
		if (!_nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (_nodes[to].From != null)
			return pathTo(to);

		while (unvisitedNodes.Count > 0)
		{
			var currentNode = unvisitedNodes.First();
			if (currentNode.Id == to)
				break;

			//Console.Error.WriteLine("Processing {0} with {1} unvisited nodes", currentNode, unvisitedNodes.Count);
			currentNode.Visited = true;
			unvisitedNodes.Remove(currentNode);

			var unvisitedNeighbours = currentNode.Neighbours
				.Select(id => _nodes[id]).Where(node => !node.Visited)
				.Select(node => new
				{
					Node = node,
					RelativeDistance = currentNode.DistanceTo(node)
				}).OrderBy(x => x.RelativeDistance)
				.ToArray();

			foreach (var neighbour in unvisitedNeighbours)
			{
				var tentativeDistance = currentNode.Distance + neighbour.RelativeDistance;
				var addToUnvistedQueue = neighbour.Node.From == null;
				if (neighbour.Node.Distance > tentativeDistance)
				{
					if (neighbour.Node.From != null)
					{
						unvisitedNodes.Remove(neighbour.Node);
						addToUnvistedQueue = true;
					}
					neighbour.Node.From = currentNode.Id;
					neighbour.Node.Distance = tentativeDistance;
				}

				if (addToUnvistedQueue)
				{
					var beforeNode = unvisitedNodes.Where(node => node.Distance > neighbour.Node.Distance).FirstOrDefault();
					if (beforeNode == null)
						unvisitedNodes.AddLast(neighbour.Node);
					else
					{
						var lln = unvisitedNodes.Find(beforeNode);
						unvisitedNodes.AddBefore(lln, neighbour.Node);
					}
				}
			}
		}

		return pathTo(to);
	}

	private string[] pathTo(string to)
	{
		if (_nodes[to].From == null)
			return null;

		var path = new Stack<string>();
		var walker = to;
		while (walker != From)
		{
			path.Push(walker);
			walker = _nodes[walker].From;
		}
		path.Push(From);
		return path.ToArray();
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


