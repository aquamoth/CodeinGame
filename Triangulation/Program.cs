﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
	static void Main(string[] args)
	{
		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int W = int.Parse(inputs[0]); // width of the building.
		int H = int.Parse(inputs[1]); // height of the building.
		int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
		inputs = Console.ReadLine().Split(' ');
		int X0 = int.Parse(inputs[0]);
		int Y0 = int.Parse(inputs[1]);

		var batman = new Point(X0, Y0);

		var points = createPoints(W, H);

		// game loop
		var sw = new Stopwatch();
		while (true)
		{
			string BOMBDIST = Console.ReadLine(); // Current distance to the bomb compared to previous distance (COLDER, WARMER, SAME or UNKNOWN)
			sw.Restart();

			setDistancesTo(batman, points);
			//Debug("{0} ms: Calculated new distances", sw.ElapsedMilliseconds);
			switch (BOMBDIST)
			{
				case "UNKNOWN":
					break;
				case "COLDER":
					Debug("{0} ms: is colder", sw.ElapsedMilliseconds);
					points = points.Where(x => x.Distance > x.OldDistance).ToArray();
					//Debug("{0} ms: Found valid points", sw.ElapsedMilliseconds);
					break;
				case "WARMER":
					Debug("{0} ms: is warmer", sw.ElapsedMilliseconds);
					points = points.Where(x => x.Distance < x.OldDistance && x != batman).ToArray();
					//Debug("{0} ms: Found valid points", sw.ElapsedMilliseconds);
					break;
				case "SAME":
					Debug("{0} ms: is same distance", sw.ElapsedMilliseconds);
					points = points.Where(x => x.Distance == x.OldDistance).ToArray();
					//Debug("{0} ms: Found valid points", sw.ElapsedMilliseconds);
					break;
				default:
					throw new NotSupportedException("Got Distance: " + BOMBDIST);
			}
			//Debug("{0} ms: Determined {1} st valid points", sw.ElapsedMilliseconds, points.Length);

			//printMap(points, W, H, batman);
			batman = findMiddleOf(points);

			//Debug("{0} ms: Selected next point", sw.ElapsedMilliseconds);

			foreach (var point in points) point.OldDistance = point.Distance;
			//Debug("{0} ms: Updated old distances", sw.ElapsedMilliseconds);

			Console.WriteLine(batman);
		}
	}

	private static Point findMiddleOf(Point[] points)
	{
		var p1 = points.OrderBy(x => x.Distance).Skip(points.Length / 2).First();
		var p2 = new QuickSelect().Get<Point>(points, points.Length / 2);
		if (p1 != p2)
		{
			Debug("Trivial sort suggests point {0} (d={1}) and QuickSelects answers {2} (d={3})", p1, p1.Distance, p2, p2.Distance);
		}
		return p1;
	}

	private static void printMap(Point[] points, int width, int height, Point batman)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var token = batman.X == x && batman.Y == y ? "!" : points.Any(p => p.X == x && p.Y == y) ? "?" : "#";
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("");
		}
	}

	private static void setDistancesTo(Point from, Point[] points)
	{
		for (var i = 0; i < points.Length; i++)
		{
			var to = points[i];
			to.Distance = euclides(from, to);
		}
	}

	private static int euclides(Point from, Point to)
	{
		return (int)Math.Pow(to.X - from.X, 2) + (int)Math.Pow(to.Y - from.Y, 2);
	}

	private static Point[] createPoints(int width, int height)
	{
		var nodes = new Point[width * height];
		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
		{
			nodes[x + y * width] = new Point(x, y);
		}
		return nodes;
	}

	public static void Debug(string format, params object[] args)
	{
		Console.Error.WriteLine(string.Format(format, args));
	}
}

class Point : IComparable
{
	public int X { get; private set; }
	public int Y { get; private set; }
	public int Distance { get; set; }
	public int OldDistance { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return X + " " + Y;
	}

	int IComparable.CompareTo(object obj)
	{
		return this.Distance.CompareTo(((Point)obj).Distance);
	}
}

class QuickSelect
{
	Random random = new Random();

	public T Get<T>(T[] list, int index) where T : IComparable
	{
		return select(list, 0, list.Length - 1, index);
	}

	private T select<T>(T[] list, int left, int right, int index) where T : IComparable
	{
		if (left == right)
			return list[left];
		var pivotIndex = random.Next(left, right + 1);
		pivotIndex = partition(list, left, right, pivotIndex);
		if (index == pivotIndex)
			return list[index];
		else if (index < pivotIndex)
			return select(list, left, pivotIndex - 1, index);
		else
			return select(list, pivotIndex + 1, right, index);
	}

	private int partition<T>(T[] list, int left, int right, int pivotIndex) where T : IComparable
	{
		T temp;
		var pivotValue = list[pivotIndex];
		list[pivotIndex] = list[right];
		list[right] = pivotValue;
		var storeIndex = left;
		for (var i = left; i < right - 1; i++)
		{
			if (list[i].CompareTo(pivotValue) < 0)
			{
				temp = list[i];
				list[i] = list[storeIndex];
				list[storeIndex] = temp;
				storeIndex++;
			}
		}
		temp = list[right];
		list[right] = list[storeIndex];
		list[storeIndex] = temp;
		return storeIndex;
	}
}