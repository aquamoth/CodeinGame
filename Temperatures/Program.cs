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
		int n = int.Parse(Console.ReadLine()); // the number of temperatures to analyse
		string temps = Console.ReadLine(); // the n temperatures expressed as integers ranging from -273 to 5526

		if (n == 0)
		{
			Console.WriteLine("0");
		}
		else
		{
			var temperatures = temps.Split(' ').Select(x => int.Parse(x));

			var closestT = temperatures
				.Select(x => new { t = x, absT = Math.Abs(x) })
				.OrderBy(x => x.absT)
				.ThenByDescending(x => x.t)
				.First()
				.t;

			Console.WriteLine(closestT.ToString());
		}
	}
}