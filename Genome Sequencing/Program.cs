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
		foreach (var subseq in orderedSequences.Skip(1))
		{
			sequence = combine(sequence, subseq);
		}

		return sequence;
	}

	private static string combine(string s1, string s2)
	{
		var s2AfterS1Length = 1;
		while (s2AfterS1Length < s2.Length && s1.EndsWith(s2.Substring(0, s2AfterS1Length)))
		{
			s2AfterS1Length++;
		}

		var s1AfterS2Length = 1;
		while (s1AfterS2Length < s1.Length && s2.EndsWith(s1.Substring(0, s1AfterS2Length)))
		{
			s1AfterS2Length++;
		}

		if (s1AfterS2Length > s2AfterS1Length)
		{
			return s2 + s1.Substring(s1AfterS2Length - 1);
		}
		else
		{
			return s1 + s2.Substring(s2AfterS1Length - 1);
		}
	}
}