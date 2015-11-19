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

			//if (writers.ContainsKey(writer))
			//{
			//	writers[writer].Add(influencer);
			//}
			//else
			//{

			//}
		}

		var people = links.SelectMany(x => new[] { x.Item1, x.Item2 }).ToArray();
		var allInfluencers = links.Select(x => x.Item2).Distinct();
		var firstPerson = people.Except(allInfluencers).Single();

		int maxLength = 0;
		var algorithm = new Dijkstra(links);
		foreach (var person in people)
		{
			var path = algorithm.Path(firstPerson, person);
			if (maxLength<path.Length)
			{
				maxLength = path.Length;
			}
		}

		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine(maxLength); // The number of people involved in the longest succession of influences
	}
}


public class Dijkstra
{
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
	}

	IDictionary<int, Node> _nodes;

	public Dijkstra(IEnumerable<Tuple<int, int>> links)
	{
		_nodes = links
			.SelectMany(x => new[] { x.Item1, x.Item2 })
			.Distinct()
			.Select(id => new Node(id))
			.ToDictionary(x => x.Id);

		foreach (var node in _nodes.Values)
		{
			node.Neighbours = links.Where(tuple => tuple.Item1 == node.Id).Select(tuple => tuple.Item2)
					.Union(links.Where(tuple => tuple.Item2 == node.Id).Select(tuple => tuple.Item1))
					.Select(id => _nodes[id])
					.ToArray();
		}
	}

	public int[] Path(int from, int to)
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
					neighbour.ShortestPath = (currentNode.ShortestPath ?? new int[0]).Union(new[] { currentNode.Id }).ToArray();
				}
			}

			currentNode.Visited = true;
			unvisitedNodes.Remove(currentNode);

			if (currentNode.Id == to)
				break;

			currentNode = unvisitedNodes.OrderBy(x => x.Distance).FirstOrDefault();
		}
		while (currentNode != null && currentNode.Distance != int.MaxValue);

		//Determine output
		var toNode = _nodes[to];
		if (toNode.Distance == int.MaxValue)
			return null; // No path to this gateway exists
		else
			return (currentNode.ShortestPath ?? new int[0]).Union(new[] { currentNode.Id }).ToArray();
	}
}
