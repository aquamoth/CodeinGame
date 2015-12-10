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
		int N = int.Parse(Console.ReadLine());
		var works = new Work[N];
		for (int i = 0; i < N; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int J = int.Parse(inputs[0]);
			int D = int.Parse(inputs[1]);
			works[i] = new Work { Starts = J, Ends = J + D - 1 };
		}

		var sortedWorks = works.OrderBy(x => x.Starts).ThenBy(x => x.Ends).ToArray();

		var maxSolutions = solutions(0, sortedWorks);


		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine(maxSolutions);
	}

	private static int solutions(int lastOccupiedDay, Work[] works)
	{
		var max = 0;
		for (var i = 0; i < works.Count(); i++)
		{
			var work = works[i];
			if (work.Starts > lastOccupiedDay)
			{
				var count = solutions(work.Ends, works.Skip(i + 1).ToArray()) + 1;
				if (count > max)
					max = count;
			}
		}
		return max;
	}
}

class Work
{
	public int Starts { get; set; }
	public int Ends { get; set; }
}