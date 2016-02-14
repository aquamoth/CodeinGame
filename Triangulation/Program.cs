using System;
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
	static QuickSelect quickSelect = new QuickSelect();

	static void Main(string[] args)
	{
		//test();


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

	//private static void test()
	//{
	//	var pointsString = "2, 41, 32, 25, 5, 17, 13, 25";
	//	//var pointsString = "1, 1, 1, 1, 2, 41, 32, 25, 5, 17, 13, 25, 20, 25, 10, 41, 32, 25";
	//	//var pointsString = "1, 1, 1, 1, 2, 41, 32, 25, 5, 17, 13, 25, 20, 25, 10, 41, 32, 25, 18, 13, 16, 26, 8, 49, 36, 9, 40, 29, 45, 34, 25, 18, 5, 41, 17, 13, 8, 45, 5, 41, 34, 29, 10, 4, 40, 29, 20, 34, 20, 26, 2, 20, 17, 10, 34, 37, 10, 5, 45, 29, 4, 26, 9, 16, 17, 25, 40, 4, 26, 8, 49, 17, 2, 25, 37, 5, 20, 10, 9, 8, 13, 5, 26, 2, 10, 13, 16, 13, 4, 13, 29, 17, 5, 36, 37, 26, 5, 13, 20, 29, 40, 25, 34, 10, 34, 10, 17, 26, 37, 32, 45, 41, 17, 26, 37, 20, 18, 29, 40, 18, 25, 34, 45, 25, 20, 25, 32, 41, 9, 16, 25, 36, 49, 41, 29, 34, 41, 37, 40, 45, 50, 50, 50, 50, 50, 50, 50, 50, 52, 58, 52, 52, 64, 61, 65, 61, 58, 61, 53, 65, 65, 52, 65, 58, 65, 61, 52, 61, 53, 53, 53, 52, 58, 58, 64, 65, 65, 53, 65, 65, 68, 85, 85, 85, 80, 73, 90, 74, 90, 82, 81, 68, 68, 73, 80, 85, 73, 80, 68, 74, 81, 82, 80, 85, 85, 73, 90, 85, 82, 89, 85, 74, 72, 90, 74, 89, 82, 89, 97, 97, 97, 72, 97, 98, 122, 218, 128, 193, 194, 125, 185, 178, 173, 170, 169, 170, 109, 157, 101, 100, 205, 101, 149, 170, 104, 160, 153, 148, 145, 144, 145, 100, 136, 245, 232, 221, 212, 205, 200, 197, 104, 109, 164, 233, 277, 148, 160, 208, 180, 226, 137, 146, 113, 173, 178, 113, 113, 130, 130, 169, 98, 117, 106, 130, 125, 116, 125, 145, 136, 146, 306, 289, 274, 100, 261, 157, 164, 250, 226, 225, 241, 106, 117, 196, 197, 234, 169, 200, 229, 137, 260, 185, 185, 116, 104, 100, 130, 113, 101, 234, 125, 122, 205, 109, 106, 229, 153, 100, 181, 200, 221, 244, 269, 149, 128, 145, 164, 185, 208, 104, 125, 148, 173, 233, 170, 185, 202, 221, 242, 265, 290, 170, 193, 121, 144, 169, 218, 157, 122, 145, 145, 109, 130, 153, 178, 162, 180, 193, 208, 225, 244, 265, 288, 313, 181, 202, 225, 250, 170, 116, 137, 160, 185, 394, 194, 205, 218, 233, 250, 269, 290, 313, 338, 180, 116, 137, 160, 185, 205, 101, 122, 145, 170, 125, 148, 369, 173, 212, 221, 232, 245, 260, 277, 296, 317, 340, 365, 117, 130, 153, 178, 136, 125, 146, 169, 194, 130, 149, 241, 250, 261, 274, 289, 306, 325, 346, 121";
	//	var points = pointsString.Split(',').Select(x => int.Parse(x)).ToArray();
	//	var middlePoint = points.Length / 2;
	//	var p1 = points.OrderBy(x => x).Skip(middlePoint).First();
	//	var p2 = new QuickSelect().Get<int>(points, middlePoint);
	//	//var result = string.Join(", ", points);
	//	var unsortedArray = pointsString.Split(',').Select(x => int.Parse(x)).ToArray();
	//	var sortedArray = pointsString.Split(',').Select(x => int.Parse(x)).OrderBy(x => x).ToArray();
	//	for (int i = 0; i < middlePoint; i++)
	//	{
	//		if (sortedArray[i] != points[i])
	//		{
	//			System.Diagnostics.Debug.WriteLine("[{0}]: {2} != {1}. Unsorted {3}", i, points[i], sortedArray[i], unsortedArray[i]);
	//		}
	//	}
	//}

	private static Point findMiddleOf(Point[] points)
	{
		//var p1 = points.OrderBy(x => x.Distance).Skip(points.Length / 2).First();
		var p2 = quickSelect.Get<Point>(points, points.Length / 2);
		//if (p1 != p2)
		//{
		//	Debug("Trivial sort suggests point {0} (d={1}) and QuickSelects answers {2} (d={3})", p1, p1.Distance, p2, p2.Distance);
		//	if (p1.Distance != p2.Distance)
		//	{
		//		Debug("{0}", string.Join(", ", points.Select(x => x.Distance)));
		//	}
		//}
		return p2;
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
		for (var i = left; i < right; i++)
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