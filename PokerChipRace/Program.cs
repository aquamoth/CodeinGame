using System;
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
	const float SCARE_DISTANCE = 65;
	const float ATTACK_RANGE = 50;

	static void Main(string[] args)
	{
		int playerId = int.Parse(Console.ReadLine()); // your id (0 to 4)

		// game loop
		while (true)
		{
			int playerChipCount = int.Parse(Console.ReadLine()); // The number of chips under your control
			int entityCount = int.Parse(Console.ReadLine()); // The total number of entities on the table, including your chips
			var entities = readEntitiesFromConsole(entityCount);

			var myChips = entities.Where(x => x.Player == playerId).ToArray();

			//Loop 1: Evalute reachability of current targets
			foreach (var currentChip in myChips.Where(chip => chip.State == ChipState.Attacking))
			{
				abortAttackIfRequired(currentChip, entities);
			}

			//Loop 2: Determine action for each chip
			while (myChips.Any(chip => chip.NextAction == null))
			{
				var currentChip = myChips.Where(chip => chip.NextAction == null).First();
				determineActionFor(currentChip, myChips, entities);
			}

			//Loop 3: Print actions
			foreach (var currentChip in myChips)
			{
				Console.WriteLine(currentChip.NextAction);
				currentChip.NextAction = null;
			}





		}
	}

	private static void abortAttackIfRequired(Entity currentChip, Entity[] entities)
	{
		if (isPathThreat(currentChip, currentChip.Target, entities) != null)
		{
			Console.Error.WriteLine("Chip #{0} aborts the attack on #{1}", currentChip.Id, currentChip.Target.Id);
			currentChip.Target.State = ChipState.Waiting;
			currentChip.Target.Target = null;
			currentChip.State = ChipState.Waiting;
			currentChip.Target = null;
		}
	}

	private static void determineActionFor(Entity currentChip, Entity[] myChips, Entity[] entities)
	{
		if (currentChip.State == ChipState.Waiting)
		{
			var targets = findTarget(currentChip, myChips, entities);
			currentChip.Target = targets
				.Where(entity => isPathThreat(currentChip, entity, entities) == null)
				.FirstOrDefault();
			if (currentChip.Target != null)
			{
				currentChip.NextAction = string.Format("{0} I'm attacking #{1}", currentChip.Target.P.Print(), currentChip.Target.Id);
				currentChip.State = ChipState.Attacking;
				currentChip.Target.Target = currentChip;
				currentChip.Target.State = ChipState.Food;
			}
		}

		if (currentChip.State == ChipState.Attacking)
		{
			if (currentChip.NextAction == null)
			{
				var command = currentChip.V.Length == 0 ? currentChip.Target.P.Print() : "WAIT";
				currentChip.NextAction = string.Format("{0} I'm attacking #{1}", command, currentChip.Target.Id);
			}
		}
		else
		{
			var adversary = isPathThreat(currentChip, currentChip, entities);
			if (adversary == null)
			{
				var message = currentChip.Target == null ? "Zzzz..." : "I'm getting eaten";
				currentChip.NextAction = "WAIT " + message;
			}
			else
			{
				if (currentChip.Target != null)
				{
					currentChip.Target.NextAction = null;
					currentChip.Target.State = ChipState.Recalculate;
					currentChip.Target.Target = null;
				}
				currentChip.State = ChipState.Waiting;
				currentChip.Target = null;
				var distance = (currentChip.P - adversary.P);
				var fleeVector = distance.Rotate(15);
				var targetPosition = fleeVector + currentChip.P;
				currentChip.NextAction = targetPosition.Print("Scared of #" + adversary.Id);
				Console.Error.WriteLine("{0} vs {1}=> d={2}. Flee at: {3} => Target={4}", currentChip.P, adversary.P, distance, fleeVector, targetPosition);
			}
		}
	}

	private static Entity isPathThreat(Entity currentChip, Entity target, Entity[] entities)
	{
		//TODO: Implement this
		return entities
			.Where(chip => chip.Player != currentChip.Player)
			.Where(chip => chip.Radius > currentChip.Radius)
			.Where(chip => { var d = chip.DistanceTo(currentChip); Console.Error.WriteLine("#{0} -> #{1} = {2}", currentChip.Id, chip.Id, d); return d < SCARE_DISTANCE; })
			.FirstOrDefault();
	}

	private static IEnumerable<Entity> findTarget(Entity currentChip, Entity[] myChips, Entity[] entities)
	{
		var targets = edible(currentChip, entities)
						   .Select(entity => new { Entity = entity, Score = scoreEating(currentChip, entity, myChips) })
						   .OrderByDescending(x => x.Score)
						   .Select(x => x.Entity)
						   .ToArray();
		return targets;
	}

	private static double scoreEating(Entity currentChip, Entity entity, Entity[] myChips)
	{
		var score = Math.Pow(currentChip.Radius + entity.Radius, 2) - Math.Pow(currentChip.Radius, 2);
		if (entity.Player == currentChip.Player)
			score -= Math.Pow(entity.Radius, 2);
		return score / entity.DistanceTo(currentChip);
	}

	private static IEnumerable<Entity> edible(Entity currentChip, Entity[] entities)
	{
		return entities.Except(new[] { currentChip })
			.Where(chip => chip.Radius < currentChip.Radius || chip.Player == currentChip.Player)
			.Where(chip => chip.DistanceTo(currentChip) < ATTACK_RANGE)
			.Where(chip => (chip.V - currentChip.V).Length < 50) //Don't hunt down propulsion droplets
			.ToArray();
	}

	private static Tuple<Entity, float> closestSiblingToEat(Entity currentChip, Entity[] myChips)
	{
		var siblings = myChips.Except(new[] { currentChip });
		var closestSibling = siblings
			.Where(chip => chip.State == ChipState.Waiting)
			.Select(sibling => new Tuple<Entity, float>(sibling, sibling.DistanceTo(currentChip)))
			.OrderBy(x => x.Item2)
			.FirstOrDefault();
		return closestSibling;
	}

	#region Helper Methods

	private static Entity[] readEntitiesFromConsole(int entityCount)
	{
		return Enumerable
				.Repeat(0, entityCount)
				.Select(x => readEntityFromConsole())
				.ToArray();
		//var entities = new Entity[entityCount];
		//for (int i = 0; i < entityCount; i++)
		//{
		//	entities[i] = readEntityFromConsole();
		//	//Console.Error.WriteLine(entities[i]);
		//}
		//return entities;
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

		var entity = new Entity
		{
			Id = id,
			Player = player,
			Radius = radius,
			P = new Point { X = x, Y = y },
			V = new Point { X = vx, Y = vy },
			State = ChipState.Waiting
		};
		return entity;
	}

	#endregion Helper Methods
}

#region Classes

enum ChipState
{
	Waiting,
	Attacking,
	Food,
	Recalculate
}

class Entity
{
	public int Id { get; set; }
	public int Player { get; set; }
	public float Radius { get; set; }
	public Point P { get; set; }
	public Point V { get; set; }

	public ChipState State { get; set; }
	public Entity Target { get; set; }
	public string NextAction { get; set; }

	public float DistanceTo(Entity e)
	{
		var vector = this.P - e.P;
		return vector.Length - this.Radius - e.Radius;
	}

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

	public Point Rotate(double degrees)
	{
		var radians = degrees / 180 * Math.PI;
		return new Point
		{
			X = (float)(this.X * Math.Cos(radians) + this.Y * Math.Sin(radians)),
			Y = (float)(this.X * Math.Sin(radians) + this.Y * Math.Cos(radians))
		};
	}

	public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}

	internal string Print(string comment = null)
	{
		var x = Math.Round(this.X);
		var y = Math.Round(this.Y);
		return x + " " + y + (comment == null ? "" : " " + comment);
	}
}

#endregion Classes
