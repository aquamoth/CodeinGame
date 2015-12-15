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
	public static Stopwatch T1 = new Stopwatch();
	public static Stopwatch T2 = new Stopwatch();
	public static Stopwatch T3 = new Stopwatch();
	public static Stopwatch T4 = new Stopwatch();
	public static Stopwatch T5 = new Stopwatch();
	public static Stopwatch T6 = new Stopwatch();
	public static Stopwatch T7 = new Stopwatch();

	static void Main(string[] args)
	{
		var sw0 = new Stopwatch();
		var sw1 = new Stopwatch();


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

		var node = createNodeFor(rooms[0]);
		leafs.Enqueue(node);

		long lastPrintMs = sw1.ElapsedMilliseconds;
		int bestPathMoney = 0;
		while (leafs.Any())
		{

			if (lastPrintMs + 1000 < sw1.ElapsedMilliseconds)
			{
				Console.Error.WriteLine("Timer0: {0}", sw0.ElapsedMilliseconds);
				Console.Error.WriteLine("Timer1: {0}", sw1.ElapsedMilliseconds);
				Console.Error.WriteLine("HasParent: {0}", Solution.T1.ElapsedMilliseconds);
				Console.Error.WriteLine("FindShorterPath: {0}", Solution.T2.ElapsedMilliseconds);
				Console.Error.WriteLine("FindShorterPath (inner 1): {0}", Solution.T6.ElapsedMilliseconds);
				Console.Error.WriteLine("FindShorterPath (inner 2): {0}", Solution.T7.ElapsedMilliseconds);
				Console.Error.WriteLine("Calculating Money: {0}", Solution.T3.ElapsedMilliseconds);
				Console.Error.WriteLine("Move branch: {0}", Solution.T4.ElapsedMilliseconds);
				Console.Error.WriteLine("Unlink branch: {0}", Solution.T5.ElapsedMilliseconds);

				lastPrintMs = sw1.ElapsedMilliseconds;
			}


			var walker = leafs.Dequeue();
			if (walker.IsDisposed)
				continue;

			//Find possible children
			bool foundValidChild = false;
			foreach (var childRoom in walker.Room.Exits)
			{
				if (childRoom.Id == -1)
				{
					T3.Start();
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
					T3.Stop();
				}
				else if (!walker.HasParent(childRoom))
				{
					foundValidChild = true;
					var existingBranch = findShorterBranch(walker, childRoom);
					if (existingBranch == null)
					{
						var newLeaf = addChildNode(walker, childRoom);
						leafs.Enqueue(newLeaf);
					}
					else
					{
						move(walker, existingBranch);
					}
				}
			}
			if (!foundValidChild)
			{
				T5.Start();
				//TODO: Unlink this branch since it has no valid exit
				while (walker.Parent != null && walker.Parent.Children.Count == 1)
					walker = walker.Parent;
				walker.Dispose();
				T5.Stop();
			}
		}

		sw1.Stop();


		Console.Error.WriteLine("Timer0: {0}", sw0.ElapsedMilliseconds);
		Console.Error.WriteLine("Timer1: {0}", sw1.ElapsedMilliseconds);
		Console.Error.WriteLine("HasParent: {0}", Solution.T1.ElapsedMilliseconds);
		Console.Error.WriteLine("FindShorterPath: {0}", Solution.T2.ElapsedMilliseconds);
		Console.Error.WriteLine("FindShorterPath (inner 1): {0}", Solution.T6.ElapsedMilliseconds);
		Console.Error.WriteLine("FindShorterPath (inner 2): {0}", Solution.T7.ElapsedMilliseconds);
		Console.Error.WriteLine("Calculating Money: {0}", Solution.T3.ElapsedMilliseconds);
		Console.Error.WriteLine("Move: {0}", Solution.T4.ElapsedMilliseconds);
		Console.Error.WriteLine("Unlink branch: {0}", Solution.T5.ElapsedMilliseconds);

		Console.WriteLine(bestPathMoney);
	}

	private static Node createNodeFor(Room room)
	{
		var node = new Node { Room = room };
		room.ReferencedBy.Add(node);
		return node;
	}

	private static void move(Node newParent, Node existingBranch)
	{
		Solution.T4.Start();
		try
		{
			existingBranch.Parent.Children.Remove(existingBranch);
			newParent.Children.Add(existingBranch);
			existingBranch.Parent = newParent;

			pruneInvalidChildren(existingBranch);
		}
		finally
		{
			Solution.T4.Stop();
		}
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
			if (existingBranch.HasParent(testBranch.Room))
			{
				testBranch.Dispose();
			}
		}
	}

	private static Node findShorterBranch(Node parentOfNewNode, Room room)
	{
		Solution.T2.Start();
		try
		{
			foreach (var alternativeBranch in room.ReferencedBy)
			{
				var newNodeWalker = parentOfNewNode;
				var branchWalker = alternativeBranch.Parent;
				while (branchWalker != null)
				{
					Solution.T6.Start();
					try
					{
						Solution.T7.Start();
						while (newNodeWalker != null && !newNodeWalker.Room.Equals(branchWalker.Room))
							newNodeWalker = newNodeWalker.Parent;
						Solution.T7.Stop();

						if (newNodeWalker == null)
							break;
						branchWalker = branchWalker.Parent;
						if (branchWalker == null)
							return alternativeBranch;
					}
					finally
					{
						Solution.T6.Stop();
					}
				}
			}
			return null;
		}
		finally
		{
			Solution.T2.Stop();
		}
	}

	private static Node addChildNode(Node parent, Room room)
	{
		var node = createNodeFor(room);
		node.Parent = parent;
		parent.Children.Add(node);
		return node;
	}
}


public class Room
{
	public int Id { get; set; }
	public string[] Neighbours { get; set; }
	public Room[] Exits { get; set; }
	public int Money { get; set; }

	public List<Node> ReferencedBy { get; private set; }

	public Room()
	{
		ReferencedBy = new List<Node>();
	}
	public Room(string addressLine)
		:this()
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

public class Node
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
		if (this.Room != null)
		{
			this.Room.ReferencedBy.Remove(this);
			this.Room = null;
		}

		if (this.Parent != null)
		{
			this.Parent.Children.Remove(this);
			this.Parent = null;
		}

		if (this.Children != null)
		{
			foreach (var child in Children)
				child.Dispose();
			this.Children = null;
		}

		IsDisposed = true;
	}

	internal bool HasParent(Room room)
	{
		Solution.T1.Start();
		try
		{
			var walker = this;
			while (walker != null)
			{
				if (walker.Room == room)
					return true;
				walker = walker.Parent;
			}
			return false;
		}
		finally
		{
			Solution.T1.Stop();
		}
	}
}