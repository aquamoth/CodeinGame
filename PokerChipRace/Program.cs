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

			var myChips = entities.Where(x=>x.Player==playerId).ToArray();
			for (int i = 0; i < playerChipCount; i++)
			{
				var chip = myChips[i];

				var nextTarget = entities.Where(x => x.Radius < chip.Radius).OrderByDescending(x => x.Radius).FirstOrDefault();
				if (nextTarget!=null)
				{
					var nextTargetIn1Sec = nextTarget.P + nextTarget.V * 1;
					var x = Math.Round(nextTargetIn1Sec.X);
					var y = Math.Round(nextTargetIn1Sec.Y);
					Console.WriteLine(x + " " + y); // One instruction per chip: 2 real numbers (x y) for a propulsion, or 'WAIT'.
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
}