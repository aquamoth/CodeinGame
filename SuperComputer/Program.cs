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

		var sortedWorks = works.OrderBy(x => x.Ends).ThenBy(x => x.Starts).ToArray();

		var lastEnd = 0;
		var count = 0;
		for (int i = 0; i < sortedWorks.Length; i++)
		{
			if (sortedWorks[i].Starts > lastEnd)
			{
				count++;
				lastEnd = sortedWorks[i].Ends;
			}
		}

		Console.WriteLine(count);
	}
}

class Work
{
	public int Starts { get; set; }
	public int Ends { get; set; }
}