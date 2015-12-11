﻿using System;
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
		var bfs = new Dijkstra(addresses, startPoint);

		var timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		var path = bfs.Path(endPoint);
		timer.Stop();

		Console.Error.WriteLine("Total time: " + timer.ElapsedMilliseconds);
		Console.Error.WriteLine("Remove item: " + bfs.Timer1.ElapsedMilliseconds);
		Console.Error.WriteLine("Insert item: " + bfs.Timer2.ElapsedMilliseconds);
		Console.Error.WriteLine("Find Neighbours: " + bfs.Timer3.ElapsedMilliseconds);
		Console.Error.WriteLine("Main loop: " + bfs.Timer4.ElapsedMilliseconds);
		Console.Error.WriteLine("Finding first: " + bfs.Timer5.ElapsedMilliseconds);
		Console.Error.WriteLine("Removing first: " + bfs.Timer6.ElapsedMilliseconds);

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


#region Helpers

public class Dijkstra
{
	public System.Diagnostics.Stopwatch Timer1 { get; set; }
	public System.Diagnostics.Stopwatch Timer2 { get; set; }
	public System.Diagnostics.Stopwatch Timer3 { get; set; }
	public System.Diagnostics.Stopwatch Timer4 { get; set; }
	public System.Diagnostics.Stopwatch Timer5 { get; set; }
	public System.Diagnostics.Stopwatch Timer6 { get; set; }

	public class Node
	{
		public string Id { get; set; }
		public string[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public string[] Path { get; set; }
		public double Distance { get; set; }

		public virtual double DistanceTo(Node node) { return 1.0; }
	}

	Dictionary<string, Node> _nodes;
	public string From { get; private set; }
	List<Node> unvisitedNodes = new List<Node>();
	int unvisitedNodesIndex;

	public Dijkstra(Node[] zones, string from)
	{
		_nodes = zones.ToDictionary(x => x.Id);// zones.Select(x => new Node { Id = x.Id, Neighbours = x.Neighbours.ToArray() }).ToArray();
		Reset(from);
		Timer1 = new System.Diagnostics.Stopwatch();
		Timer2 = new System.Diagnostics.Stopwatch();
		Timer3 = new System.Diagnostics.Stopwatch();
		Timer4 = new System.Diagnostics.Stopwatch();
		Timer5 = new System.Diagnostics.Stopwatch();
		Timer6 = new System.Diagnostics.Stopwatch();
	}

	public void Reset(string from)
	{
		foreach (var node in _nodes.Values)
		{
			node.Path = null;
			node.Distance = double.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		_nodes[from].Path = new string[] { from };
		_nodes[from].Distance = 0;
		unvisitedNodes.Add(_nodes[from]);
		unvisitedNodesIndex = 0;
	}

	public string[] Path(string to)
	{
		if (!_nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (_nodes[to].Path != null)
			return _nodes[to].Path;

		while (unvisitedNodesIndex < unvisitedNodes.Count)
		{
			Timer5.Start();
			var currentNode = unvisitedNodes[unvisitedNodesIndex];
			Timer5.Stop();

			if (currentNode.Id == to)
				break;

			Timer6.Start();
			unvisitedNodesIndex++;
			currentNode.Visited = true;
			Timer6.Stop();

			Timer3.Start();
			var unvisitedNeighbours = currentNode.Neighbours.Select(id => _nodes[id]).Where(node => !node.Visited);
			Timer3.Stop();
			Timer4.Start();
			foreach (var neighbour in unvisitedNeighbours)
			{
				var tentativeDistance = currentNode.Distance + currentNode.DistanceTo(neighbour);
				if (neighbour.Distance > tentativeDistance)
				{
					if (neighbour.Path != null)
					{
						Timer1.Start();
						unvisitedNodes.Remove(neighbour);
						Timer1.Stop();
					}
					neighbour.Path = currentNode.Path.Concat(new[] { neighbour.Id }).ToArray();
					neighbour.Distance = tentativeDistance;
				}
				Timer2.Start();
				insertUnvisited(neighbour, unvisitedNodesIndex);
				Timer2.Stop();
			}
			Timer4.Stop();
		}

		return _nodes[to].Path;
	}

	private void insertUnvisited(Node neighbour, int minIndex = 0)
	{
		var maxIndex = unvisitedNodes.Count;
		while (minIndex < maxIndex)
		{
			var index = (minIndex + maxIndex) / 2;
			var distance = unvisitedNodes[index].Distance;
			if (neighbour.Distance < distance)
			{
				maxIndex = index;
			}
			else
			{
				minIndex = index + 1;
			}
		}

		unvisitedNodes.Insert(minIndex, neighbour);
	}
}


/// <summary>
/// Comparer for comparing two keys, handling equality as beeing greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey> : IComparer<TKey> 
	where TKey : IComparable
{
	#region IComparer<TKey> Members

	public int Compare(TKey x, TKey y)
	{
		int result = x.CompareTo(y);

		if (result == 0)
			return 1;   // Handle equality as beeing greater
		else
			return result;
	}

	#endregion
}

#endregion Helpers






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

