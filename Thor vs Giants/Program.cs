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
		tdd();

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
				.Select(x => new { 
					Giant = x, 
					Direction = x.DirectionTo(thor),
					Distance = x.DistanceTo(thor) 
				})
				.ToArray();

			if (giantsInfo.All(giant => giant.Giant.InKillZoneOf(thor)))
			{
				Console.Error.WriteLine("Thor strikes to kill all giants left");
				thorStrike();
			}
			else
			{
				var possibleDirections = excludeWalkingOffMap(thor, allDirections());

				var criticalDirections = giantsInfo.SelectMany(info => critical(thor, info.Giant)).ToArray();

				foreach (var giant in giantsInfo)
				{
					Console.Error.WriteLine("{0} = {1}*{2}: {3}",
						new Point { X = thor.X - giant.Giant.X, Y = thor.Y - giant.Giant.Y },
						giant.Distance,
						giant.Direction,
						string.Join(", ", critical(thor, giant.Giant).ToArray())
						);
				}
				possibleDirections = possibleDirections.Except(criticalDirections).ToArray();

				Console.Error.WriteLine("Thor's non-critical directions: {0}", string.Join(", ", possibleDirections.Select(d => d.ToString()).ToArray()));

				if (!possibleDirections.Any())
				{
					Console.Error.WriteLine("Thor strikes because no other moves are left");
					thorStrike();
				}
				else
				{
					var target = calculateGiantsMedianPoint(giants);
					var targetDirection = target.DirectionTo(thor);
					Console.Error.WriteLine("Thor's target at {0} is to: {1}", target, targetDirection);

					var scoredDirections = possibleDirections
						.Select(d => new { Direction = d, Score = scoreDirection(targetDirection, d) })
						.OrderBy(x => x.Score)
						.ToArray();

					var nextDirection = scoredDirections.Select(x => x.Direction).First();
					Console.Error.WriteLine("Thor's best move is to: {0}", nextDirection);
					moveThorTo(thor, nextDirection);
				}
			}
		}
	}

	public static IEnumerable<Direction> critical(Point thor, Point point)
	{
		var dx = thor.X - point.X;
		var dy = thor.Y - point.Y;
		if (dx == 2)
		{
			#region dx 2
			if (dy == -2)
			{
				yield return Direction.SW;
			}
			else if (dy == -1)
			{
				yield return Direction.SW;
				yield return Direction.W;
			}
			else if (dy == 0)
			{
				yield return Direction.SW;
				yield return Direction.W;
				yield return Direction.NW;
			}
			else if (dy == 1)
			{
				yield return Direction.W;
				yield return Direction.NW;
			}
			else if (dy == 2)
			{
				yield return Direction.NW;
			}
			#endregion dx -2
		}
		else if (dx == 1)
		{
			#region dx 1
			if (dy == -2)
			{
				yield return Direction.S;
				yield return Direction.SW;
			}
			else if (dy == -1)
			{
				yield return Direction.S;
				yield return Direction.SW;
				yield return Direction.W;
			}
			else if (dy == 0)
			{
				yield return Direction.S;
				yield return Direction.SW;
				yield return Direction.W;
				yield return Direction.NW;
				yield return Direction.N;
			}
			else if (dy == 1)
			{
				yield return Direction.W;
				yield return Direction.NW;
				yield return Direction.N;
			}
			else if (dy == 2)
			{
				yield return Direction.NW;
				yield return Direction.N;
			}
			#endregion dx -1
		}
		else if (dx == 0)
		{
			#region dx 0
			if (dy == -2)
			{
				yield return Direction.SW;
				yield return Direction.S;
				yield return Direction.SE;
			}
			else if (dy == -1)
			{
				yield return Direction.W;
				yield return Direction.SW;
				yield return Direction.S;
				yield return Direction.SE;
				yield return Direction.E;
			}
			else if (dy == 0)
			{
				yield break;//!
			}
			else if (dy == 1)
			{
				yield return Direction.W;
				yield return Direction.NW;
				yield return Direction.N;
				yield return Direction.NE;
				yield return Direction.E;
			}
			else if (dy == 2)
			{
				yield return Direction.NW;
				yield return Direction.N;
				yield return Direction.NE;
			}
			#endregion dx 0
		}
		else if (dx == -1)
		{
			#region dx -1
			if (dy == -2)
			{
				yield return Direction.S;
				yield return Direction.SE;
			}
			else if (dy == -1)
			{
				yield return Direction.S;
				yield return Direction.SE;
				yield return Direction.E;
			}
			else if (dy == 0)
			{
				yield return Direction.S;
				yield return Direction.SE;
				yield return Direction.E;
				yield return Direction.NE;
				yield return Direction.N;
			}
			else if (dy == 1)
			{
				yield return Direction.E;
				yield return Direction.NE;
				yield return Direction.N;
			}
			else if (dy == 2)
			{
				yield return Direction.NE;
				yield return Direction.N;
			}
			#endregion dx 1
		}
		else if (dx == -2)
		{
			#region dx -2
			if (dy == -2)
			{
				yield return Direction.SE;
			}
			else if (dy == -1)
			{
				yield return Direction.SE;
				yield return Direction.E;
			}
			else if (dy == 0)
			{
				yield return Direction.SE;
				yield return Direction.E;
				yield return Direction.NE;
			}
			else if (dy == 1)
			{
				yield return Direction.E;
				yield return Direction.NE;
			}
			else if (dy == 2)
			{
				yield return Direction.NE;
			}
			#endregion dx 2
		}
	}

	private static int scoreDirection(Direction targetDirection, Direction evaluatedDirection)
	{
		var value1 = (8 + evaluatedDirection - targetDirection) % 8;
		var value2 = (8 + targetDirection - evaluatedDirection) % 8;
		return Math.Min(value1, value2);
	}

	private static Direction[] allDirections()
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
		return possibleDirections;
	}

	private static Point calculateGiantsMedianPoint(Point[] giants)
	{
		var giantXs = giants.Select(g => g.X).OrderBy(x => x);
		var medianX = (giantXs.Count() % 2 == 0)
			? giantXs.Skip(giantXs.Count() / 2 - 1).Take(2).Sum() / 2
			: giantXs.Skip((giantXs.Count() - 1) / 2).First();
		var giantYs = giants.Select(g => g.Y).OrderBy(y => y);
		var medianY = (giantYs.Count() % 2 == 0)
			? giantYs.Skip(giantYs.Count() / 2 - 1).Take(2).Sum() / 2
			: giantYs.Skip((giantYs.Count() - 1) / 2).First();

		var target = new Point { X = medianX, Y = medianY };
		return target;
	}

	private static Direction[] exclude(Direction[] possibleDirections, Direction giant, bool onlyKeepOppositeDirections)
	{
		var directionsToExclude = new[] { giant };
		if (onlyKeepOppositeDirections)
		{
			directionsToExclude = directionsToExclude.Concat(new[] { rotate(giant, 1), rotate(giant, -1) }).ToArray();
		}
		return possibleDirections.Except(directionsToExclude).ToArray();
	}

	private static Direction rotate(Direction d, int steps) {
		if (d == Direction.WAIT)
			return d;
		else
			return (Direction)((d - 1) + steps % 8) + 1;
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
			case Direction.WAIT: break;
			default:
				throw new NotSupportedException();
		}
	}

	private static void tdd()
	{
		tdd1(Direction.NW, Direction.NW, 0);
		tdd1(Direction.NW, Direction.N, 1);
		tdd1(Direction.NW, Direction.W, 1);
		tdd1(Direction.NW, Direction.NE, 2);
		tdd1(Direction.NW, Direction.SW, 2);
		tdd1(Direction.NW, Direction.E, 3);
		tdd1(Direction.NW, Direction.S, 3);
		tdd1(Direction.NW, Direction.SE, 4);

		var thor = new Point { X = 4, Y = 7 };


		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 4 }), new Direction[] { });
		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 5 }), new[] { Direction.NW }, "nw*2");
		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 6 }), new[] { Direction.W, Direction.NW }, "w-nw");
		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 7 }), new[] { Direction.SW, Direction.W, Direction.NW }, "w*2");
		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 8 }), new[] { Direction.SW, Direction.W }, "w-sw");
		collectionAssert<Direction>(critical(thor, new Point { X = 2, Y = 9 }), new[] { Direction.SW }, "sw*2");

		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 4 }), new Direction[] { });
		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 5 }), new[] { Direction.NW, Direction.N }, "n-nw");
		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 6 }), new[] { Direction.W, Direction.NW, Direction.N }, "nw");
		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 7 }), new[] { Direction.S, Direction.SW, Direction.W, Direction.NW, Direction.N }, "w");
		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 8 }), new[] { Direction.S, Direction.SW, Direction.W }, "sw");
		collectionAssert<Direction>(critical(thor, new Point { X = 3, Y = 9 }), new[] { Direction.S, Direction.SW }, "s-sw");

		collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 4 }), new Direction[] { });
		collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 5 }), new[] { Direction.NW, Direction.N, Direction.NE }, "n*2");
		collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 6 }), new[] { Direction.W, Direction.NW, Direction.N, Direction.NE, Direction.E }, "n");
		//collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 7 }), new[] { }, "wait");
		collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 8 }), new[] { Direction.W, Direction.SW, Direction.S, Direction.SE, Direction.E }, "s");
		collectionAssert<Direction>(critical(thor, new Point { X = 4, Y = 9 }), new[] { Direction.SW, Direction.S, Direction.SE }, "s*2");

		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 4 }), new Direction[] { });
		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 5 }), new[] { Direction.NE, Direction.N }, "n-ne");
		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 6 }), new[] { Direction.E, Direction.NE, Direction.N }, "ne");
		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 7 }), new[] { Direction.S, Direction.SE, Direction.E, Direction.NE, Direction.N }, "e");
		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 8 }), new[] { Direction.S, Direction.SE, Direction.E }, "se");
		collectionAssert<Direction>(critical(thor, new Point { X = 5, Y = 9 }), new[] { Direction.S, Direction.SE }, "s-se");

		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 4 }), new Direction[] { });
		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 5 }), new[] { Direction.NE }, "ne*2");
		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 6 }), new[] { Direction.E, Direction.NE }, "e-ne");
		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 7 }), new[] { Direction.SE, Direction.E, Direction.NE }, "e*2");
		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 8 }), new[] { Direction.SE, Direction.E }, "e-se");
		collectionAssert<Direction>(critical(thor, new Point { X = 6, Y = 9 }), new[] { Direction.SE }, "se*2");
	}

	private static void collectionAssert<T>(IEnumerable<T> actual, IEnumerable<T> expected, string message = null)
		where T : IComparable
	{
		if (actual.Count() == expected.Count())
		{
			if (actual.Select((x, index) => expected.ToArray()[index].Equals(x)).All(x => x == true))
				return;
		}

		if (message != null)
		{
			Console.Error.WriteLine(message);
		}
		Console.Error.WriteLine("Expected: " + string.Join(", ", expected.Select(x => x.ToString()).ToArray()));
		Console.Error.WriteLine("Actual: " + string.Join(", ", actual.Select(x => x.ToString()).ToArray()));
		throw new ApplicationException("Assertion failed");
	}

	static void tdd1(Direction d1, Direction d2, int expected)
	{
		var actual = scoreDirection(d1, d2);
		if (expected != actual)
		{
			Console.Error.WriteLine("{0}<->{1}, expecting {2}, got {3}.", d1, d2, expected, actual);
			throw new ArgumentException(string.Format("{0}<->{1}, expecting {2}, got {3}.", d1, d2, expected, actual));
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
		if (dx < 0)
		{
			if (dy < 0) return Direction.NW;
			else if (dy > 0) return Direction.SW;
			else return Direction.W;
		}
		else if (dx > 0)
		{
			if (dy < 0) return Direction.NE;
			else if (dy > 0) return Direction.SE;
			else return Direction.E;
		}
		else
		{
			if (dy < 0) return Direction.N;
			else if (dy > 0) return Direction.S;
			else return Direction.WAIT;
		}
	}

	public bool InKillZoneOf(Point p)
	{
		return this.X >= p.X - 4 
			&& this.X <= p.X + 4 
			&& this.Y >= p.Y - 4 
			&& this.Y <= p.Y + 4;
	}
}

//[Flags]
public enum Direction
{
	WAIT = 0,

	N = 1,
	NE = 2,
	E = 3,
	SE = 4,
	S = 5,
	SW = 6,
	W = 7,
	NW = 8,
}