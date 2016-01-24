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
		double[] distances = null, lastDistances = null;

		// game loop
		while (true)
		{
			string BOMBDIST = Console.ReadLine(); // Current distance to the bomb compared to previous distance (COLDER, WARMER, SAME or UNKNOWN)

			distances = createDistances(batman, points);
			switch (BOMBDIST)
			{
				case "UNKNOWN":
					break;
				case "COLDER":
					var validPoints = points
						.Select((point, index) => new { Point = point, Distance = distances[index], lastDistance = lastDistances[index] })
						.Where(x => x.Distance > x.lastDistance && x.Point!=batman)
						.ToArray();
					distances = validPoints.Select(x => x.Distance).ToArray();
					points = validPoints.Select(x => x.Point).ToArray();
					break;
				case "WARMER":
					var validPoints2 = points
						.Select((point, index) => new { Point = point, Distance = distances[index], lastDistance = lastDistances[index] })
						.Where(x => x.Distance < x.lastDistance && x.Point != batman)
						.ToArray();
					distances = validPoints2.Select(x => x.Distance).ToArray();
					points = validPoints2.Select(x => x.Point).ToArray();
					break;
				case "SAME":
					var validPoints3 = points
						.Select((point, index) => new { Point = point, Distance = distances[index], lastDistance = lastDistances[index] })
						.Where(x => x.Distance == x.lastDistance && x.Point != batman)
						.ToArray();
					distances = validPoints3.Select(x => x.Distance).ToArray();
					points = validPoints3.Select(x => x.Point).ToArray();
					break;
				default:
					throw new NotSupportedException("Got Distance: " + BOMBDIST);
			}

			//printMap(points, W, H, batman);

			var count = points.Length;
			var nextPoint = points.Select((Point, index) => new { Point, Distance = distances[index] })
				.OrderBy(x => x.Distance)
				.Skip(count / 2)
				.First()
				.Point;


			batman = nextPoint;
			lastDistances = distances;
			Console.WriteLine(batman);
		}
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

	private static double[] createDistances(Point from, Point[] points)
	{
		var distances = new double[points.Length];
		for(var i=0;i<points.Length;i++)
		{
			var to = points[i];
			distances[i] = euclides(from, to);
		}
		return distances;
	}

	private static double euclides(Point from, Point to)
	{
		return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
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

class Point
{
	public int X { get; private set; }
	public int Y { get; private set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return X + " " + Y;
	}
}