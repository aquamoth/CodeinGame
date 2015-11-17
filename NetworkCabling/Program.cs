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
		var buildings = new List<Point>();
		int N = int.Parse(Console.ReadLine());
		for (int i = 0; i < N; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			int X = int.Parse(inputs[0]);
			int Y = int.Parse(inputs[1]);
			buildings.Add(new Point(X, Y));
		}

		//foreach (var b in buildings)
		//	Console.Error.WriteLine(b);

		var leftX = buildings.Min(b => b.X);
		var rightX = buildings.Max(b => b.X);
		var mainCableLength = rightX - leftX;

		var verticalMedian = buildings.Select(b => b.Y).OrderBy(y => y).Skip(N / 2).First();
		var dedicatedCableLength = buildings.Sum(b => Math.Abs(b.Y - verticalMedian));

		var totalLength = mainCableLength + dedicatedCableLength;
		Console.WriteLine(totalLength);
	}
}

public class Point
{
	public long X { get; set; }
	public long Y { get; set; }

	public Point(long x, long y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", X, Y);
	}
}
