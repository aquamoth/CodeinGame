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

			var giantsInfo = giants
				.Select(x => new { Giant = x, Direction = x.DirectionTo(thor), Distance = x.DistanceTo(thor) })
				.OrderBy(x => x.Distance)
				.ToArray();

			//foreach (var giant in giantsInfo)
			//{
			//	Console.Error.WriteLine("Distance from {0} to {1} is {2} to {3}", thor, giant.Giant, giant.Distance, giant.Direction);
			//}


			if (giantsInfo.All(giant => giant.Distance <= 4))
			{
				Console.Error.WriteLine("Thor strikes to kill all giants left");
				thorStrike();
			}
			else
			{

				var possibleDirections = new[] 
					{ 
						Direction.N,
						Direction.NE,
						Direction.E,
						Direction.SE,
						Direction.S,
						Direction.SW,
						Direction.W,
						Direction.NW,
					};

				possibleDirections = excludeWalkingOffMap(thor, possibleDirections);

				var criticalDirections = giantsInfo.Where(giant => giant.Distance <= 2).Select(giant => giant.Direction).ToArray();
				foreach (var direction in criticalDirections)
					possibleDirections = exclude(possibleDirections, direction);

				Console.Error.WriteLine("Thor's non-critical directions: {0}", string.Join(", ", possibleDirections.Select(d => d.ToString()).ToArray()));

				var nextDirections = possibleDirections;
				foreach (var giant in giantsInfo)
				{
					possibleDirections = exclude(possibleDirections, giant.Direction);
					if (!possibleDirections.Any())
						break;
					nextDirections = possibleDirections;
				}

				if (nextDirections.Any())
				{
					Console.Error.WriteLine("Thor can move to: {0}", string.Join(", ", nextDirections.Select(d => d.ToString()).ToArray()));
					moveThorTo(thor, nextDirections.First());
				}
				else if (giantsInfo[0].Distance > 1)
					thorWaits();
				else
					thorStrike();
			}



		}
	}

	private static Direction[] exclude(Direction[] possibleDirections, Direction giant)
	{
		IEnumerable<Direction> result = possibleDirections.AsEnumerable();
		result = tryExclude(giant, result, Direction.N);
		result = tryExclude(giant, result, Direction.E);
		result = tryExclude(giant, result, Direction.S);
		result = tryExclude(giant, result, Direction.W);
		return result.ToArray();
	}

	private static IEnumerable<Direction> tryExclude(Direction giant, IEnumerable<Direction> result, Direction directionToTry)
	{
		if (giant.HasFlag(directionToTry))
			result = result.Where(d => !d.HasFlag(directionToTry));
		return result;
	}

	private static Direction[] excludeWalkingOffMap(Point thor, Direction[] possibleDirections)
	{
		if (thor.X == 0)
			possibleDirections = possibleDirections.Except(new[] { Direction.W, Direction.NW, Direction.SW }).ToArray();
		else if (thor.X == 39)
			possibleDirections = possibleDirections.Except(new[] { Direction.E, Direction.NE, Direction.SE }).ToArray();
		if (thor.Y == 0)
			possibleDirections = possibleDirections.Except(new[] { Direction.NW, Direction.N, Direction.NE }).ToArray();
		else if (thor.Y == 17)
			possibleDirections = possibleDirections.Except(new[] { Direction.SW, Direction.S, Direction.SE }).ToArray();
		return possibleDirections;
	}

	private static void thorStrike()
	{
		Console.WriteLine("STRIKE");
	}

	private static void thorWaits()
	{
		Console.WriteLine("WAIT");
	}

	private static bool isOpposite(Direction d1, Direction d2)
	{
		if (d1.HasFlag(Direction.N) && d2.HasFlag(Direction.S))
			return true;
		if (d1.HasFlag(Direction.S) && d2.HasFlag(Direction.N))
			return true;

		if (d1.HasFlag(Direction.W) && d2.HasFlag(Direction.E))
			return true;
		if (d1.HasFlag(Direction.E) && d2.HasFlag(Direction.W))
			return true;

		return false;
	}

	private static void moveThorTo(Point thor, Direction moveTo)
	{
		Console.WriteLine(moveTo.ToString());
		switch (moveTo)
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

		var d = (int)(dx == 0 ? Direction.NONE : dx < 0 ? Direction.W : Direction.E)
			+ (int)(dy == 0 ? Direction.NONE : dy < 0 ? Direction.N : Direction.S);

		return (Direction)d;
	}
}

[Flags]
public enum Direction
{
	NONE= 0,

	N = 1,
	S = 2,
	E = 4,
	W = 8,

	NE = 5,
	SE = 6,
	NW = 9,
	SW = 10,
}