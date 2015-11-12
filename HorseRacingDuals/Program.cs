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
		var strengths = new List<int>();
		for (int i = 0; i < N; i++)
		{
			int pi = int.Parse(Console.ReadLine());
			strengths.Add(pi);
		}

		strengths.Sort();
		var minDiff = strengths.Skip(1)
			.Select((x, index) => new { a = x, b = strengths[index] })
			//.Select(item => new { a = item.a, b = item.b, diff = Math.Abs(item.a - item.b) })
			.Select(item => Math.Abs(item.a - item.b))
			.Min();

		Console.WriteLine(minDiff.ToString());
	}
}