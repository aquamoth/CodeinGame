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

			//foreach (var giant in xx)
			//{
			//	Console.Error.WriteLine("Distance from {0} to {1} is {2} to {3}", thor, giant.Giant, giant.Distance, giant.Direction);
			//}


			if (giantsInfo.All(giant=>giant.Distance<=4))
			{
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

				var veryCloseGiants = giantsInfo.Where(x => x.Distance < 2);
				foreach(var giant in veryCloseGiants){
					possibleDirections = exclude(possibleDirections, giant.Direction);
				}



				if (possibleDirections.Length > 0)
				{
					moveThorTo(thor, possibleDirections.First());
				}
				else
				{
					thorStrike();
				}

				//if (giantsInfo[0].Distance > 2)
				//{
				//	moveThorTo(thor, giantsInfo[0].Direction);
				//}
				//else if (giantsInfo.Length > 1
				//	&& isOpposite(giantsInfo[0].Direction, giantsInfo[1].Direction)
				//	&& giantsInfo[1].Distance > 2)
				//{
				//	moveThorTo(thor, giantsInfo[1].Direction);
				//}
				//else
				//{
				//	Console.WriteLine("STRIKE");
				//}

				//Console.WriteLine("WAIT"); // The movement or action to be carried out: WAIT STRIKE N NE E SE S SW W or N








			}



		}
	}

	private static Direction[] exclude(Direction[] possibleDirections, Direction direction)
	{
		IEnumerable<Direction> result = possibleDirections.AsEnumerable();
		if (direction.HasFlag(Direction.N))
			result = result.Where(d => !d.HasFlag(Direction.N));
		if (direction.HasFlag(Direction.E))
			result = result.Where(d => !d.HasFlag(Direction.E));
		if (direction.HasFlag(Direction.S))
			result = result.Where(d => !d.HasFlag(Direction.S));
		if (direction.HasFlag(Direction.W))
			result = result.Where(d => !d.HasFlag(Direction.W));
		return result.ToArray();
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