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
		int n = int.Parse(Console.ReadLine());
		string vs = Console.ReadLine();
		Console.Error.WriteLine(vs);


		var amounts = vs.Split(' ').Select(v => int.Parse(v)).ToArray();

		var maxLoss = 0;
		var localMax = amounts[0];
		for (int i = 1; i < n; i++)
		{
			var amount = amounts[i];
			if (amount > localMax)
			{
				localMax = amount;
				Console.Error.WriteLine("New local max: " + localMax.ToString());
			}
			else
			{
				var loss = amount - localMax;
				Console.Error.WriteLine("loss: " + loss.ToString());
				maxLoss = Math.Min(maxLoss, loss);
			}
		}

		Console.WriteLine(maxLoss.ToString());
	}
}