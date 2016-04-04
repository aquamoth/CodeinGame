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
		test();


		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int W = int.Parse(inputs[0]); // width of the building.
		int H = int.Parse(inputs[1]); // height of the building.
		int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
		inputs = Console.ReadLine().Split(' ');
		int X0 = int.Parse(inputs[0]);
		int Y0 = int.Parse(inputs[1]);

		var batman = new Point(X0, Y0);

		var polygon = new[] { new Point<double>(0, 0), new Point<double>(W - 1, 0), new Point<double>(W - 1, H - 1), new Point<double>(0, H - 1) };
		var round = 0;
		Tuple<Point<double>, Point<double>> line = null;

		// game loop
		var sw = new Stopwatch();
		while (true)
		{
			string BOMBDIST = Console.ReadLine(); // Current distance to the bomb compared to previous distance (COLDER, WARMER, SAME or UNKNOWN)
			sw.Restart();
			round++;

			switch (BOMBDIST)
			{
				case "UNKNOWN":
					break;
				case "COLDER":
					Debug("{0} ms: is colder", sw.ElapsedMilliseconds);
					polygon = findPolygon(batman, polygon, line, false);
					break;

				case "WARMER":
					Debug("{0} ms: is warmer", sw.ElapsedMilliseconds);
					polygon = findPolygon(batman, polygon, line, true);
					break;
	
				case "SAME":
					Debug("{0} ms: is same distance", sw.ElapsedMilliseconds);
					throw new NotImplementedException("Need to create a polygon that exactly fits all points on the cutting line!");
					//points = points.Where(x => x.Distance == x.OldDistance).ToArray();
					//deltaY = lastDirection;
					//Debug("{0} ms: Found valid points", sw.ElapsedMilliseconds);
					break;
				
				default:
					throw new NotSupportedException("Got Distance: " + BOMBDIST);
			}

			//Find centroid of polygon
			var centroid = findCentroidOf(polygon);
			Debug("Polygon is: ({0})", string.Join(") (", polygon.Select(p=>p.ToString()).ToArray()));
			Debug("Centroid is at ({0})", centroid);
			line = createSplittingLine(batman, centroid, W, H);
			Debug("Splitting line ({0})-({1})", line.Item1, line.Item2);
			var newBatman = mirror(batman, centroid);
			if (newBatman.X < 0 || newBatman.X >= W || newBatman.Y < 0 || newBatman.Y >= H)
			{
				Debug("Can't use ({0})", newBatman);
				newBatman = moveIntoBox(newBatman, centroid, W, H);
				centroid = new Point<double>((newBatman.X + batman.X) / 2, (newBatman.Y + batman.Y) / 2);
				Debug("New centroid is at ({0})", centroid);
				line = createSplittingLine(batman, centroid, W, H);
				Debug("New splitting line ({0})-({1})", line.Item1, line.Item2);
			}
			batman = newBatman;
			Debug("Next batman at ({0})", batman);
			Console.WriteLine(batman);
		}
	}

	private static Point<double>[] findPolygon(Point batman, Point<double>[] polygon, Tuple<Point<double>, Point<double>> line, bool isWarmer)
	{
		var lineP1 = line.Item1;
		var lineP2 = line.Item2;
		var lineK = (lineP2.Y - lineP1.Y) / (lineP2.X - lineP1.X);
		var lineYatBatman = lineP1.Y + batman.X * lineK;
		var batmanIsAboveSplittingLine = batman.Y <= lineYatBatman;
		var polygons = cut(polygon, new Tuple<double, double>(lineK, lineP1.Y)).ToArray();

		polygon = batmanIsAboveSplittingLine ^ isWarmer ? polygons.Last() : polygons.First();
		return polygon;
	}

	private static Point moveIntoBox(Point batman, Point<double> centroid, int W, int H)
	{
		var v = new Point<double>(batman.X - centroid.X, batman.Y - centroid.Y);
		if (batman.X < 0)
		{
			batman = new Point(0, (int)(batman.Y + (0 - batman.X) * v.Y / v.X));
		}
		else if (batman.X >= W)
		{
			batman = new Point(W - 1, (int)(batman.Y + (W - 1 - batman.X) * v.Y / v.X));
		}
		else if (batman.Y < 0)
		{
			batman = new Point((int)(batman.X + (0 - batman.Y) * v.X / v.Y), 0);
		}
		else if (batman.Y >= H)
		{
			batman = new Point((int)(batman.X + (H - 1 - batman.Y) * v.X / v.Y), H - 1);
		}
		return batman;
	}

	private static IEnumerable<Point<double>[]> cut(Point<double>[] polygon, Tuple<double, double> k_m)
	{
		if (polygon.Length < 2)
			throw new NotSupportedException();

		var polygon1 = new List<Point<double>>();
		var polygon2 = new List<Point<double>>();


		var lastPoint = polygon[polygon.Length - 1];
		var lastAbove = isAbove(lastPoint, k_m);
		for (var i = 0; i < polygon.Length; i++)
		{
			var thisPoint = polygon[i];
			var thisAbove = isAbove(thisPoint, k_m);
			if(thisAbove==0)
			{
				polygon1.Add(thisPoint);
				polygon2.Add(thisPoint);
			}
			else if (thisAbove * lastAbove < 0)
			{
				//Points on different sides of the line
				var intersection = intersectionOf(lastPoint, thisPoint, k_m);
				polygon1.Add(intersection);
				polygon2.Add(intersection);

				if (thisAbove < 0)
				{
					polygon1.Add(thisPoint);
				}
				else
				{
					polygon2.Add(thisPoint);
				}
			}
			else
			{
				if (thisAbove < 0)
				{
					polygon1.Add(thisPoint);
				}
				else
				{
					polygon2.Add(thisPoint);
				}
			}

			lastPoint = thisPoint;
			lastAbove = thisAbove;
		}

		if (polygon1.Count > 2)
			yield return polygon1.ToArray();
		if (polygon2.Count > 2)
			yield return polygon2.ToArray();
	}

	private static Point<double> intersectionOf(Point<double> lastPoint, Point<double> thisPoint, Tuple<double, double> k_m)
	{
		var k1 = k_m.Item1;
		var m1 = k_m.Item2;
		var k2 = (thisPoint.Y - lastPoint.Y) / (thisPoint.X - lastPoint.X);
		var m2 = double.IsInfinity(k2) ? thisPoint.X : thisPoint.Y - thisPoint.X * k2;

		if (double.IsInfinity(k1))
		{
			if (double.IsInfinity(k2))
				return null;
			var x = m1;
			var y = k2 * x;
			return new Point<double>(x, y);
		}
		else if(double.IsInfinity(k2))
		{
			var x = m2;
			var y = k1 * x + m1;
			return new Point<double>(x, y);
		}
		else
		{
			//Y = k2 * X + m2
			//Y = k1 * X + m1
			//=>
			// k1 * X + m1 = k2 * X + m2
			//=>
			var x = (m2 - m1) / (k1 - k2);
			var y = k2 * (m2 - m1) / (k1 - k2) + m2;
			return new Point<double>(x, y);
#warning what if lines are parallell, non-vertical?
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="lastPoint"></param>
	/// <param name="k_m"></param>
	/// <returns>-1 == above/left, 0 == on line, 1 == below/right</returns>
	private static int isAbove(Point<double> lastPoint, Tuple<double, double> k_m)
	{
		if (double.IsInfinity(k_m.Item1)) // The line is vertical
		{
			return lastPoint.X.CompareTo(k_m.Item2);
		}
		else
		{
			var y = k_m.Item1 * lastPoint.X + k_m.Item2;
			return lastPoint.Y.CompareTo(y);
		}
	}

	private static Point mirror(Point batman, Point<double> centroid)
	{
		var x = (int)Math.Round(2 * centroid.X - batman.X);
		var y = (int)Math.Round(2 * centroid.Y - batman.Y);
		return new Point(x, y);
	}

	private static Tuple<Point<double> ,Point<double>> createSplittingLine(Point batman, Point<double> centroid, int W, int H)
	{
		var v = new Point<double>(centroid.X - batman.X, centroid.Y - batman.Y);
		var normal = new Point<double>(v.Y, -v.X);
		var normalK = normal.Y / normal.X;

		var x0 = 0;
		var y0 = centroid.Y + normalK * (x0 - centroid.X);

		var x1 = W - 1;
		var y1 = centroid.Y + normalK * (x1 - centroid.X);


		////Find where line intersects top of bounding box
		////(x,0) = centroid - a*normal
		////=>
		////0 = centroid.y-a*normal.y		=> a*normal.y = centroid.y => a = centroid.y / normal.y
		////x = centroid.x-a*normal.x		=> x = centroid.x - normal.x /normal.y * centroid.y
		//double y0 = 0.0;
		//double x0 = centroid.X - normal.X * centroid.Y / normal.Y;
		//if (x0 < 0)
		//{
		//	//Intersection was outside the bounding box. Find where line intersects left of bounding box instead.
		//	x0 = 0;
		//	y0 = centroid.Y - normal.Y * centroid.X / normal.X;
		//}


		////Find where line intersects bottom of bounding box
		////(x,H) = centroid + a*normal
		////=>
		////H = centroid.y+a*normal.y		=> a*normal.y = H - centroid.y => a = (H - centroid.y) / normal.y
		////x = centroid.x+a*normal.x		=> x = centroid.x + normal.x / normal.y * (H - centroid.y)
		//double y1 = H;
		//double x1 = centroid.X + normal.X / normal.Y * (H - centroid.Y);
		//if (x1 > W)
		//{
		//	//Intersection was outside the bounding box. Find where line intersects left of bounding box instead.
		//	x1 = W;
		//	y1 = centroid.Y + normal.Y / normal.X * (W - centroid.X);
		//}

		return new Tuple<Point<double>, Point<double>>(new Point<double>(x0, y0), new Point<double>(x1, y1));
	}

	private static Point<double> findCentroidOf(Point<double>[] polygon)
	{
		var centroidX = 0.0;
		var centroidY = 0.0;
		var signedArea = 0.0;
		for (int i = 0; i < polygon.Length; i++)
		{
			var x0 = polygon[i].X;
			var y0 = polygon[i].Y;
			var x1 = polygon[(i + 1) % polygon.Length].X;
			var y1 = polygon[(i + 1) % polygon.Length].Y;
			var a = x0 * y1 - x1 * y0;
			signedArea += a;
			centroidX += (x0 + x1) * a;
			centroidY += (y0 + y1) * a;
		}

		signedArea /= 2;
		var centroid = new Point<double>(centroidX / (6 * signedArea), centroidY / (6 * signedArea));
		return centroid;
		/*
Point2D compute2DPolygonCentroid(const Point2D* vertices, int vertexCount)
{
    Point2D centroid = {0, 0};
    double signedArea = 0.0;
    double x0 = 0.0; // Current vertex X
    double y0 = 0.0; // Current vertex Y
    double x1 = 0.0; // Next vertex X
    double y1 = 0.0; // Next vertex Y
    double a = 0.0;  // Partial signed area

    // For all vertices
    int i=0;
    for (i=0; i<vertexCount-1; ++i)
    {
        x0 = vertices[i].x;
        y0 = vertices[i].y;
        x1 = vertices[(i+1) % vertexCount].x;
        y1 = vertices[(i+1) % vertexCount].y;
        a = x0*y1 - x1*y0;
        signedArea += a;
        centroid.x += (x0 + x1)*a;
        centroid.y += (y0 + y1)*a;
    }

    signedArea *= 0.5;
    centroid.x /= (6.0*signedArea);
    centroid.y /= (6.0*signedArea);

    return centroid;
}
		 */
	}


	private static void test()
	{

		//(0 9.67899942056785) (0 8.3) (4 6.7) (4 12.6856529001578)









		var polygon = new[] { new Point<double>(0, 9.67899942056785) , new Point<double>(0, 8.3), new Point<double>(4, 6.7), new Point<double>(4, 12.6856529001578)};
		//var polygon = new[] { new Point<double>(0, 0), new Point<double>(2, 0), new Point<double>(2, 2), new Point<double>(0, 2) };
		//var polygon = new[] { new Point<double>(1, 1), new Point<double>(4, 1), new Point<double>(4, 4), new Point<double>(1, 4) };
		//var polygon = new[] { new Point<double>(0, 0), new Point<double>(2, 0), new Point<double>(2, 1) };
		var centroid = findCentroidOf(polygon);
		System.Diagnostics.Debug.WriteLine(centroid);


		var polygons1 = cut(polygon, new Tuple<double, double>(0, 0)).ToArray(); //horizontal at y=0
		var polygons2 = cut(polygon, new Tuple<double, double>(0, 5)).ToArray(); //horizontal at y=5
		var polygons3 = cut(polygon, new Tuple<double, double>(double.PositiveInfinity, 0)).ToArray(); //vertical at y=0
		var polygons4 = cut(polygon, new Tuple<double, double>(double.PositiveInfinity, 5)).ToArray(); //vertical at y=5
		var polygons5 = cut(polygon, new Tuple<double, double>(1, 10)).ToArray();
		var polygons6 = cut(polygon, new Tuple<double, double>(1, 0)).ToArray();
		var polygons7 = cut(polygon, new Tuple<double, double>(1, 1)).ToArray(); 


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
	}

	//private static Point findMiddleOf(Point[] points)
	//{
	//	var midpoint = points.Length / 2;
	//	//var p1 = points.OrderBy(x => x.Distance).Skip(midpoint).First();
	//	var p2 = quickSelect.Get<Point>(points, midpoint);
	//	//if (p1 != p2)
	//	//{
	//	//	Debug("Trivial sort suggests point {0} (d={1}) and QuickSelects answers {2} (d={3})", p1, p1.Distance, p2, p2.Distance);
	//	//	if (p1.Distance != p2.Distance)
	//	//	{
	//	//		Debug("{0}", string.Join(", ", points.Select(x => x.Distance)));
	//	//	}
	//	//}
	//	return p2;
	//}

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

	//private static void setDistancesTo(Point from, Point[] points)
	//{
	//	for (var i = 0; i < points.Length; i++)
	//	{
	//		var to = points[i];
	//		to.Distance = euclides(from, to);
	//	}
	//}

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

class Point : Point<int>
{
	public Point(int x, int y) : base(x, y) { }
}

class Point<T> where T: struct//: IComparable where T : IComparable
{
	public T X { get; private set; }
	public T Y { get; private set; }
	//public T Distance { get; set; }
	//public T OldDistance { get; set; }
	//public T NewDistance { get; set; }

	public Point(T x, T y)
	{
		X = x;
		Y = y;
	}

	//public static Point<T> operator+(Point<T> p)
	//{
	//	dynamic d1 = p, d2 = p;
	//	return new Point<T>(d1.X + d2.X, d1.Y + d2.Y);
	//}

	//public static Point<T> operator -(Point<T> p)
	//{
	//	dynamic d1 = p, d2 = p;
	//	return new Point<T>(d1.X - d2.X, d1.Y - d2.Y);
	//}


	public override string ToString()
	{
		return X + " " + Y;
	}

	//int IComparable.CompareTo(object obj)
	//{
	//	return this.Distance.CompareTo(((Point)obj).Distance);
	//}
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