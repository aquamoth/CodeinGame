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
		string[] inputs = Console.ReadLine().Split(' ');
		int numberOfSeatsOnRide = int.Parse(inputs[0]);
		int numberOfRidesPerDay = int.Parse(inputs[1]);
		int numberOfCustomerGroups = int.Parse(inputs[2]);
		var customerGroup = new int[numberOfCustomerGroups];
		for (int i = 0; i < numberOfCustomerGroups; i++)
		{
			customerGroup[i] = int.Parse(Console.ReadLine());
		}

		var queuePointer = 0;
		var totalEarnings = 0L;

		//var sw = new Stopwatch();
		//sw.Start();
		//Console.Error.WriteLine("{0}: Starting simulation", sw.ElapsedMilliseconds);


		if (numberOfSeatsOnRide >= customerGroup.Sum())
		{
			totalEarnings = customerGroup.Sum() * numberOfRidesPerDay;
		}
		else
		{
			for (int i = 0; i < numberOfRidesPerDay; i++)
			{
				var emptySeats = numberOfSeatsOnRide;
				do
				{
					emptySeats -= customerGroup[queuePointer];
					queuePointer++;
					if (queuePointer == numberOfCustomerGroups)
						queuePointer = 0;
				}
				while (emptySeats >= customerGroup[queuePointer]);
				totalEarnings = totalEarnings + numberOfSeatsOnRide - emptySeats;

				//if (i % 100000 == 0)
				//	Console.Error.WriteLine("{0}: Processed {1} rides", sw.ElapsedMilliseconds, i);
			}
		}
		//sw.Stop();
		//Console.Error.WriteLine("{0}: Processed {1} rides = {2} rides/s", sw.ElapsedMilliseconds, numberOfRidesPerDay, (double)numberOfRidesPerDay / sw.ElapsedMilliseconds * 1000);

		Console.WriteLine(totalEarnings);
	}
}


class CustomerGroup
{
	public int Size { get; set; }
}