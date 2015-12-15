using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
	static void Main(string[] args)
	{
		var sw0 = new Stopwatch();
		var sw1 = new Stopwatch();
		var sw2 = new Stopwatch();


		sw0.Start();
		var exitRoom = new Room { Id = -1 };
		int N = int.Parse(Console.ReadLine());
		var rooms = new Room[N];
		for (int i = 0; i < N; i++)
		{
			string room = Console.ReadLine();
			rooms[i] = new Room(room);
		}
		foreach (var room in rooms)
		{
			room.Exits = room.Neighbours
				.Select(id => id == "E" ? exitRoom : rooms[int.Parse(id)])
				.ToArray();
		}
		sw0.Stop();



		sw1.Start();

		var leafs = new Queue<Node>();
		var allNodes = new HashSet<Node>();

		var node = new Node { Room = rooms[0] };
		allNodes.Add(node);
		leafs.Enqueue(node);

		int bestPathMoney = 0;
		while (leafs.Any())
		{
			var walker = leafs.Dequeue();
			if (walker.IsDisposed)
				continue;

			//Find possible children
			bool foundValidChild = false;
			foreach (var childRoom in walker.Room.Exits)
			{
				if (childRoom.Id == -1)
				{
					//TODO: Store path for later summation
					foundValidChild = true;
					var money = 0;
					while (walker != null)
					{
						money += walker.Room.Money;
						walker = walker.Parent;
					}
					if (money > bestPathMoney)
					{
						bestPathMoney = money;
					}
				}
				else if (!isParent(walker, childRoom))
				{
					foundValidChild = true;
					var newLeaf = addChildNode(walker, childRoom);
					var existingBranch = findShorterBranch(newLeaf, allNodes);
					if (existingBranch == null)
					{
						allNodes.Add(newLeaf);
						leafs.Enqueue(newLeaf);
					}
					else
					{
						replace(newLeaf, existingBranch);
					}
				}
			}
			if (!foundValidChild)
			{
				//TODO: Unlink this branch since it has no valid exit
				while (walker.Parent != null && walker.Parent.Children.Count == 1)
					walker = walker.Parent;
				walker.Dispose();
			}
		}

		sw1.Stop();


		Console.Error.WriteLine("Timer0: {0}", sw0.ElapsedMilliseconds);
		Console.Error.WriteLine("Timer1: {0}", sw1.ElapsedMilliseconds);
		Console.Error.WriteLine("Timer2: {0}", sw2.ElapsedMilliseconds);

		Console.WriteLine(bestPathMoney);
	}

	private static void replace(Node newLeaf, Node existingBranch)
	{
		newLeaf.Parent.Children.Remove(newLeaf);
		existingBranch.Parent.Children.Remove(existingBranch);
		newLeaf.Parent.Children.Add(existingBranch);
		existingBranch.Parent = newLeaf.Parent;
		newLeaf.Parent = null;

		pruneInvalidChildren(existingBranch);
	}

	private static void pruneInvalidChildren(Node existingBranch)
	{
		var untestedChildren = new Queue<Node>();
		foreach(var node in existingBranch.Children){
			untestedChildren.Enqueue(node);
		}
		while (untestedChildren.Any())
		{
			var testBranch = untestedChildren.Dequeue();
			if (isParent(existingBranch, testBranch.Room))
			{
				testBranch.Dispose();
			}
		}
	}

	private static Node findShorterBranch(Node newBranch, HashSet<Node> allNodes)
	{
		var alternativeBranches = allNodes.Where(x => x.Room.Equals(newBranch.Room));
		foreach(var alternativeBranch in alternativeBranches)
		{
			var nodeOnNewBranch = newBranch;
			var nodeOnAlternativeBranch = alternativeBranch;
			while (nodeOnAlternativeBranch != null)
			{
				while (nodeOnNewBranch != null && !nodeOnNewBranch.Room.Equals(nodeOnAlternativeBranch.Room))
				{
					nodeOnNewBranch = nodeOnNewBranch.Parent;
				}
				if (nodeOnNewBranch == null)
					break;
				nodeOnAlternativeBranch = nodeOnAlternativeBranch.Parent;
				if (nodeOnAlternativeBranch == null)
					return alternativeBranch;
			}
		}
		return null;
	}

	private static Node addChildNode(Node parent, Room room)
	{
		var child = new Node { Room = room, Parent = parent };
		parent.Children.Add(child);
		return child;
	}

	private static bool isParent(Node walker, Room room)
	{
		while (walker != null)
		{
			if (walker.Room == room)
				return true;
			walker = walker.Parent;
		}
		return false;
	}
}


public class Room
{
	public int Id { get; set; }
	public string[] Neighbours { get; set; }
	public Room[] Exits { get; set; }
	public int Money { get; set; }

	public Room()
	{

	}
	public Room(string addressLine):this()
	{
		var parts = addressLine.Split(' ');
		this.Id = parts[0] == "E" ? -1 : int.Parse(parts[0]);
		this.Money = int.Parse(parts[1]);
		this.Neighbours = new[] { parts[2], parts[3] };
	}

	public override string ToString()
	{
		return Id == -1 ? "EXIT" : string.Format("#{0} ${1}. {2}", Id, Money, string.Join(", ", Neighbours));
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
	public override bool Equals(object obj)
	{
		return this.Id.Equals(((Room)obj).Id);
	}
}

class Node
{
	public Room Room { get; set; }

	public Node Parent { get; set; }
	public List<Node> Children { get; private set; }
	public bool IsDisposed { get; private set; }

	public Node()
	{
		Children = new List<Node>();
	}

	internal void Dispose()
	{
		if (this.Parent != null)
		{
			this.Parent.Children.Remove(this);
			this.Parent = null;
		}
		this.Children = null;
		IsDisposed = true;
	}
}