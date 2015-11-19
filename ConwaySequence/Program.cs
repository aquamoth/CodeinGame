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
		int R = int.Parse(Console.ReadLine());
		int L = int.Parse(Console.ReadLine());

		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");
		var sequence = new[] { R }.AsEnumerable();
		for (int i = 0; i < L - 1; i++)
		{
			sequence = sequence.LookAndSay();
		}

		var response = string.Join(" ", sequence.Select(x => x.ToString()).ToArray());
		Console.WriteLine(response);
		Console.ReadLine();
	}
}

public static class Extensions
{
	public static IEnumerable<int> LookAndSay(this IEnumerable<int> sequence)
	{
		var counter = 0;
		int current = 0;
		foreach (var number in sequence)
		{
			if (counter == 0)
			{
				counter++;
				current = number;
			}
			else if (current == number)
			{
				counter++;
			}
			else
			{
				yield return counter;
				yield return current;
				counter = 1;
				current = number;
			}
		}

		if (counter > 0)
		{
			yield return counter;
			yield return current;
		}
	}
}