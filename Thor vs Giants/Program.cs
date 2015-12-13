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
		int TX = int.Parse(inputs[0]);
		int TY = int.Parse(inputs[1]);

		var thor = new Point { X = TX, Y = TY };

		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int H = int.Parse(inputs[0]); // the remaining number of hammer strikes.
			int N = int.Parse(inputs[1]); // the number of giants which are still present on the map.

			var giants = new Point[N];
			for (int i = 0; i < N; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				int X = int.Parse(inputs[0]);
				int Y = int.Parse(inputs[1]);
				giants[i] = new Point { X = X, Y = Y };
			}

			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");
			var xx = giants.Select(x => new { Giant = x, Direction = x.DirectionTo(thor), Distance = x.DistanceTo(thor) }).ToArray();

			foreach (var giant in xx)
			{
				Console.Error.WriteLine("Distance from {0} to {1} is {2} to {3}", thor, giant.Giant, giant.Distance, giant.Direction);
			}

			var closestGiant = xx.OrderBy(x => x.Distance).First();
			if (closestGiant.Distance > 2)
			{
				Console.WriteLine(closestGiant.Direction.ToString());
				switch (closestGiant.Direction)
				{
					case Direction.NW: thor.Y--; thor.X--; break;
					case Direction.N: thor.Y--; break;
					case Direction.NE: thor.Y--; thor.X++; break;
					case Direction.W: thor.X--; break;
					case Direction.E: thor.X++; break;
					case Direction.SW: thor.Y++; thor.X--; break;
					case Direction.S: thor.Y++; break;
					case Direction.SE: thor.Y++; thor.X++; break;
					default:
						throw new NotSupportedException();
				}
			}
			else
			{
				Console.WriteLine("STRIKE");
			}

			//Console.WriteLine("WAIT"); // The movement or action to be carried out: WAIT STRIKE N NE E SE S SW W or N
		}
	}
}

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}
	public int DistanceTo(Point p)
	{
		var dx = X - p.X;
		var dy = Y - p.Y;
		return Math.Max(Math.Abs(dx), Math.Abs(dy));
	}

	public Direction DirectionTo(Point p)
	{
		var dx = X - p.X;
		var dy = Y - p.Y;
		var d = 4 + 3 * Math.Sign(dy) + Math.Sign(dx);
		return (Direction)d;
	}
}

public enum Direction
{
	NW = 0,
	N = 1,
	NE = 2,

	W = 3,
	E = 5,
	
	SW = 6,
	S = 7,
	SE = 8,
}