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
		var oods = new List<Ood>();

		int N = int.Parse(Console.ReadLine());
		int C = int.Parse(Console.ReadLine());
		for (int i = 0; i < N; i++)
		{
			int B = int.Parse(Console.ReadLine());
			oods.Add(new Ood { Budget = B });
		}

		var amountLeft = C;

		var sortedOods = oods.OrderBy(x => x.Budget).ToList();
		for (int i = 0; i < sortedOods.Count; i++)
		{
			var currentOod = sortedOods[i];

			var amountPerOod = amountLeft / (double)(sortedOods.Count - i);
			if (amountPerOod > currentOod.Budget)
			{
				currentOod.Amount = currentOod.Budget;
			}
			else
			{
				currentOod.Amount = (int)Math.Floor(amountPerOod);
			}

			amountLeft -= currentOod.Amount;
		}

		if (amountLeft > 0)
		{
			Console.WriteLine("IMPOSSIBLE");
		}
		else
		{
			for (int i = 0; i < sortedOods.Count; i++)
			{
				Console.WriteLine(sortedOods[i].Amount);
			}
		}
	}
}

public class Ood
{
	public int Budget { get; set; }
	public int Amount { get; set; }
}