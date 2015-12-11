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
		var sequences = new string[N]; 
		for (int i = 0; i < N; i++)
		{
			string subseq = Console.ReadLine();
			sequences[i] = subseq;
		}

		var bestSequence = combine(sequences);
		Console.WriteLine(bestSequence.Length);
	}

	private static string combine(IEnumerable<string> sequences)
	{
		var orderedSequences = sequences
			.OrderByDescending(x => x.Length)
			.Distinct()
			.ToArray();

		var sequence = orderedSequences.First();
		var partsToProcess = orderedSequences.Skip(1).ToList();

		while (partsToProcess.Any())
		{
			var nextResult = partsToProcess
				.Select(p => new { Part = p, Sequence = combine(sequence, p) })
				.OrderBy(x => x.Sequence.Length)
				.First();

			sequence = nextResult.Sequence;
			partsToProcess.Remove(nextResult.Part);
		}

		return sequence;
	}

	private static string combine(string s1, string s2)
	{
		int result1 = overlap(s2, s1);
		int result2 = overlap(s1, s2);


		if (result2 + s1.Length < result1 + s2.Length)
		{
			return s2.Substring(0, result2) + s1;
		}
		else
		{
			return s1.Substring(0, result1) + s2;
		}
	}

	//private static string lastStringOf(string s, int length)
	//{
	//	return s.Substring(s.Length - length);
	//}

	private static int overlap(string s1, string s2)
	{
		for (var i = 0; i < s2.Length; i++)
		{
			if (s1.StartsWith(s2.Substring(i)))
			{
				return i;
			}
		}

		return s2.Length;
	}
}