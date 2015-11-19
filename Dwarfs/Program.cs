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
		//var writers = new Dictionary<int, Dijkstra.Node>();
		var links = new List<Tuple<int, int>>();

		int n = int.Parse(Console.ReadLine()); // the number of relationships of influence
		for (int i = 0; i < n; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int writer = int.Parse(inputs[0]); // a relationship of influence between two people (x influences y)
			int influencer = int.Parse(inputs[1]);

			links.Add(new Tuple<int, int>(writer, influencer));
		}

		var people = links.SelectMany(x => new[] { x.Item1, x.Item2 }).ToArray();
		var allInfluencers = links.Select(x => x.Item2).Distinct();
		var firstPersons = people.Except(allInfluencers);

		var algorithm = new Dijkstra(links);
		var maxLength = firstPersons.Select(p => algorithm.Nodes[p].MaxLength()).Max();

		Console.WriteLine(maxLength); // The number of people involved in the longest succession of influences
	}
}

public class Node
{
	public Node(int id)
	{
		Id = id;
		Distance = int.MaxValue;
	}

	public int Id { get; private set; }
	public Node[] Neighbours { get; set; }
	public int[] ShortestPath { get; set; }
	public int Distance { get; set; }
	public bool Visited { get; set; }

	//public static Node Tree(IEnumerable<Tuple<int, int>> links)
}

public static class NodeExtensions
{
	public static int MaxLength(this Node node)
	{
		if (node.Neighbours == null || node.Neighbours.Length == 0)
			return 1;
		return node.Neighbours.Max(child => child.MaxLength()) + 1;
	}
}

public class Dijkstra
{

	public IDictionary<int, Node> Nodes { get; private set; }

	public Dijkstra(IEnumerable<Tuple<int, int>> links)
	{
		Nodes = links
			.SelectMany(x => new[] { x.Item1, x.Item2 })
			.Distinct()
			.Select(id => new Node(id))
			.ToDictionary(x => x.Id);

		foreach (var node in Nodes.Values)
		{
			node.Neighbours = links.Where(tuple => tuple.Item1 == node.Id).Select(tuple => tuple.Item2)
					.Select(id => Nodes[id])
					.ToArray();
		}
	}
}
