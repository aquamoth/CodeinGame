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
		int nodesCounter = 0;

		int N = int.Parse(Console.ReadLine());
		var dictionary = new Node();
		for (int i = 0; i < N; i++)
		{
			string telephone = Console.ReadLine();
			//Console.Error.WriteLine(telephone);

			var walker = dictionary;
			for (int j = 0; j < telephone.Length; j++)
			{
				var digit = byte.Parse(telephone[j].ToString());

				Node temp;
				if (!walker.Next.TryGetValue(digit, out temp))
				{
					temp = new Node();
					walker.Next.Add(digit, temp);
					nodesCounter++;
				}
				walker = temp;
			}
		}




		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine(nodesCounter); // The number of elements (referencing a number) stored in the structure.
	}
}


class Node
{
	//public byte Digit { get; set; }
	public IDictionary<byte, Node> Next { get; set; }
	public Node(/*byte digit*/)
	{
		//Digit = digit;
		Next = new Dictionary<byte, Node>();
	}
}