﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * It's the survival of the biggest!
 * Propel your chips across a frictionless table top to avoid getting eaten by bigger foes.
 * Aim for smaller oil droplets for an easy size boost.
 * Tip: merging your chips will give you a sizeable advantage.
 **/
class Player
{
	const float SCARE_DISTANCE = 50;
	const float SEARCH_AND_DESTROY_DISTANCE = 200;

	static void Main(string[] args)
	{
		int playerId = int.Parse(Console.ReadLine()); // your id (0 to 4)

		// game loop
		while (true)
		{
			int playerChipCount = int.Parse(Console.ReadLine()); // The number of chips under your control
			int entityCount = int.Parse(Console.ReadLine()); // The total number of entities on the table, including your chips

			var entities = new Entity[entityCount];
			for (int i = 0; i < entityCount; i++)
			{
				entities[i] = readEntityFromConsole();
			}

			Console.Error.WriteLine("Entities:");
			foreach(var e in entities)
			{
				Console.Error.WriteLine(e);
			}

			var expectedPositionsIn3Secs = entities.Select(x => new { Id = x.Id, Player = x.Player, Radius = x.Radius, P = x.P + x.V * 3 }).ToArray();
			Console.Error.WriteLine("Entities are in 3 sec at:");
			foreach (var ee in expectedPositionsIn3Secs)
			{
				Console.Error.WriteLine(ee.Id + ": " + ee.P);
			}


			var myChips = entities.Where(x => x.Player == playerId).ToArray();
			for (int i = 0; i < playerChipCount; i++)
			{
				var chip = myChips[i];
				Console.Error.WriteLine("Calculating for #" + chip.Id);

				var expectedChipPosition = expectedPositionsIn3Secs.Where(x => x.Id == chip.Id).Single();

				var closeLargerChipsIn3Sec = expectedPositionsIn3Secs
					.Where(x => x.Player != chip.Player)
					.Where(x => x.Radius > expectedChipPosition.Radius)
					.Select(x => new { Chip = x, Distance = x.P.DistanceTo(expectedChipPosition.P) })
					.Where(x => x.Distance < SCARE_DISTANCE)
					.OrderBy(x => x.Distance);
				if (closeLargerChipsIn3Sec.Any())
				{
					var singleClosestLargerChip = closeLargerChipsIn3Sec.First();
					var closestAggressor = entities.Where(x => x.Id == singleClosestLargerChip.Chip.Id).Single();
					Console.Error.WriteLine(string.Format("#{0} is larger and only {1}m away. Avoiding!", singleClosestLargerChip.Chip.Id, singleClosestLargerChip.Distance));
					var targetPosition = chip.P + chip.P - closestAggressor.P;
					Console.Error.WriteLine("Speeding towards " + targetPosition);
					Console.WriteLine(targetPosition.Print() + " Help!"); // One instruction per chip: 2 real numbers (x y) for a propulsion, or 'WAIT'.
				}
				else
				{
					var closestSmallerChipIn3Sec = expectedPositionsIn3Secs
						.Where(x => x.Radius < expectedChipPosition.Radius)
						.Select(x => new { Chip = x, Distance = x.P.DistanceTo(expectedChipPosition.P) })
						.Where(x => x.Distance < SEARCH_AND_DESTROY_DISTANCE)
						.OrderBy(x => x.Distance)
						.ToArray();

					foreach (var smallerChip in closestSmallerChipIn3Sec)
					{
						Console.Error.WriteLine(string.Format("#{0} has range {1}", smallerChip, smallerChip.Distance));
					}


					var nextTarget = closestSmallerChipIn3Sec.FirstOrDefault();
					if (nextTarget != null)
					{
						Console.Error.WriteLine("Closest smaller target is " + nextTarget.Chip.Id);
						var expectedDistance = (nextTarget.Chip.P - expectedChipPosition.P);
						Console.Error.WriteLine("Expecting distance of " + expectedDistance.Length);

						var targetPosition = chip.P + expectedDistance;
						Console.Error.WriteLine("Speeding towards " + targetPosition);
						Console.WriteLine(targetPosition.Print()); // One instruction per chip: 2 real numbers (x y) for a propulsion, or 'WAIT'.
					}
					else
					{
						Console.WriteLine("WAIT Zzzz...");
					}

				}

			}
		}
	}

	private static Entity readEntityFromConsole()
	{
		string[] inputs = Console.ReadLine().Split(' ');
		int id = int.Parse(inputs[0]); // Unique identifier for this entity
		int player = int.Parse(inputs[1]); // The owner of this entity (-1 for neutral droplets)
		float radius = float.Parse(inputs[2]); // the radius of this entity
		float x = float.Parse(inputs[3]); // the X coordinate (0 to 799)
		float y = float.Parse(inputs[4]); // the Y coordinate (0 to 514)
		float vx = float.Parse(inputs[5]); // the speed of this entity along the X axis
		float vy = float.Parse(inputs[6]); // the speed of this entity along the Y axis

		var entity = new Entity { 
			Id = id, 
			Player = player, 
			Radius = radius, 
			P = new Point { X = x, Y = y }, 
			V = new Point { X = vx, Y = vy } 
		};
		return entity;
	}
}

class Entity
{
	public int Id { get; set; }
	public int Player { get; set; }
	public float Radius { get; set; }
	public Point P { get; set; }
	public Point V { get; set; }

	public override string ToString()
	{
		return string.Format("#{0} ({1}) at {2} + {3} has size {4}", Id, Player, P, V, Radius);
	}
}

class Point
{
	public float X { get; set; }
	public float Y { get; set; }

	public static Point operator *(Point p, int value)
	{
		return new Point { X = p.X * value, Y = p.Y * value };
	}

	public static Point operator +(Point p1, Point p2)
	{
		return new Point { X = p1.X + p2.X, Y = p1.Y + p2.Y };
	}

	public static Point operator -(Point p1, Point p2)
	{
		return new Point { X = p1.X - p2.X, Y = p1.Y - p2.Y };
	}

	public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }

	public float DistanceTo(Point p)
	{
		var vector = this - p;
		return vector.Length;
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}

	internal string Print()
	{
		var x = Math.Round(this.X);
		var y = Math.Round(this.Y);
		return x + " " + y;
	}
}