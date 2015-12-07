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
		var queue = new Queue<CustomerGroup>();

		string[] inputs = Console.ReadLine().Split(' ');
		int numberOfSeatsOnRide = int.Parse(inputs[0]);
		int numberOfRidesPerDay = int.Parse(inputs[1]);
		int N = int.Parse(inputs[2]);
		for (int i = 0; i < N; i++)
		{
			int pi = int.Parse(Console.ReadLine());
			queue.Enqueue(new CustomerGroup { Size = pi });
		}

		var totalEarnings = 0L;

		for (int i = 0; i < numberOfRidesPerDay; i++)
		{
			var seatedGroups = new List<CustomerGroup>();
			var seatedPersons = 0;

			while(queue.Any() && seatedPersons+queue.Peek().Size <= numberOfSeatsOnRide)
			{
				var group = queue.Dequeue();
				seatedGroups.Add(group);
				seatedPersons += group.Size;
			}

			totalEarnings += 1 * seatedPersons;
			foreach (var group in seatedGroups)
				queue.Enqueue(group);
		}


		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine(totalEarnings);
	}
}


class CustomerGroup
{
	public int Size { get; set; }
}