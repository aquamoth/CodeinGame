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
		var executionTimes = new SortedList<int, int>();
		for (int i = 0; i < N; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int num = int.Parse(inputs[0]);
			int t = int.Parse(inputs[1]);
			executionTimes.Add(num, t);
		}


		var errorO1 = leastError(executionTimes, (num) => 1);
		var errorOn = leastError(executionTimes, (num) => num);
		var errorOLogN = leastError(executionTimes, (num) => Math.Log(num));
		var errorONLogN = leastError(executionTimes, (num) => num * Math.Log(num));
		var errorON2 = leastError(executionTimes, (num) => num * num);
		var errorON2LogN = leastError(executionTimes, (num) => num * num * Math.Log(num));
		var errorON3 = leastError(executionTimes, (num) => num * num * num);
		var errorO2PowN = leastError(executionTimes, (num) => Math.Pow(2, num));

		var minError = min(errorO1, errorOn, errorOLogN, errorONLogN, errorON2, errorON2LogN, errorON3, errorO2PowN);
		if (errorO1 == minError)
			Console.WriteLine("O(1)");
		else if (errorOn == minError)
			Console.WriteLine("O(n)");
		else if (errorOLogN == minError)
			Console.WriteLine("O(log n)");
		else if (errorONLogN == minError)
			Console.WriteLine("O(n log n)");
		else if (errorON2 == minError)
			Console.WriteLine("O(n^2)");
		else if (errorON2LogN == minError)
			Console.WriteLine("O(n^2 log n)");
		else if (errorON3 == minError)
			Console.WriteLine("O(n^3)");
		else if (errorO2PowN == minError)
			Console.WriteLine("O(2^n)");
		else
		{
			throw new NotImplementedException();
		}
	}

	private static double min(double errorO1, params double[] args)
	{
		var value = errorO1;
		foreach (var arg in args)
		{
			if (double.IsNaN(value))
				value = arg;
			else if (!double.IsNaN(arg))
				value = Math.Min(value, arg);
		}
		return value;
	}

	private static double leastError(SortedList<int, int> executionTimes, Func<int,double> fn)
	{
		var kvp = executionTimes.Last();
		var m = kvp.Value / fn(kvp.Key);
		var result = leastSquare(executionTimes, (num) => m * fn(num));
		return result;
	}

	static double leastSquare(IEnumerable<KeyValuePair<int,int>> values, Func<int, double> fn)
	{
		return values.Select(kvp =>
		{
			var fnValue = fn(kvp.Key);
			return Math.Pow(kvp.Value - fnValue, 2);
		}).Sum();
	}
}