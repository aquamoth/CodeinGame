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
		int nbFloors = int.Parse(inputs[0]); // number of floors
		int width = int.Parse(inputs[1]); // width of the area
		int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
		int exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
		int exitPos = int.Parse(inputs[4]); // position of the exit on its floor
		int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
		int nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)


		var elevators = new List<Tuple<int, int>>();
		int nbElevators = int.Parse(inputs[7]); // number of elevators
		for (int i = 0; i < nbElevators; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
			int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor
			elevators.Add(new Tuple<int,int>(elevatorFloor, elevatorPos));
		}

		var elevatorsLookup = elevators.ToLookup(x=>x.Item1, x=>x.Item2);

		Console.Error.WriteLine("Additional elevators = {0}", nbAdditionalElevators);



		Console.Error.WriteLine("Floors with elevators: {0}", string.Join(", ", elevatorsLookup.Select(x => x.Key).ToArray()));

		var floorsWithoutElevators = Enumerable.Range(0, exitFloor)
			.Except(elevatorsLookup.Select(x => x.Key))
			.ToArray();
		Console.Error.WriteLine("Floors without elevators = {0}", string.Join(", ", floorsWithoutElevators));

		var extraElevators = nbAdditionalElevators - floorsWithoutElevators.Count();
		Console.Error.WriteLine("Extra elevators = {0}", extraElevators);


		bool firstLoop = true;
		var positions = new int[exitFloor];
		var distances = new int[exitFloor];

		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
			int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
			string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT



			if (firstLoop)
			{
				firstLoop = false;
				Console.Error.WriteLine("Calculating paths");

				var pos = clonePos;
				for (int i = 0; i < exitFloor; i++)
				{
					if (elevatorsLookup.Any(x => x.Key == i))
					{
						var targetPos = closestElevator(elevatorsLookup, i, pos);
						Console.Error.WriteLine("Closest elevator on floor {0} is at {1}", i, targetPos);
						positions[i] = targetPos;
						distances[i] = Math.Abs(targetPos - pos);
						pos = targetPos;
					}
					else
					{
						Console.Error.WriteLine("No elevators on floor {0}. Create at {1}", i, pos);
						positions[i] = -pos;
						distances[i] = 0;
					}
				}

				var floorsToShortcut = distances
					.Select((distance, floor) => new { Distance = distance, Floor = floor })
					.OrderByDescending(x => x.Distance)
					.Take(extraElevators)
					.Select(x => x.Floor)
					.ToArray();

				foreach(var floor in floorsToShortcut)
				{
					positions[floor] = floor == 0 ? -clonePos : -Math.Abs(positions[floor - 1]);
					Console.Error.WriteLine("Shortcut elevator on floor {0} at {1}", floor, -positions[floor]);
					distances[floor] = 0;
				}

				Console.Error.WriteLine("Pos: {0}", string.Join(", ", positions));
				Console.Error.WriteLine("Dist: {0}", string.Join(", ", distances));
			}


			if (cloneFloor == -1)
			{
				Console.Error.WriteLine("No clone to control right now");
				Console.WriteLine("WAIT"); // action: WAIT or BLOCK
			}
			else
			{
				int targetPos;
				if (exitFloor == cloneFloor)
				{
					Console.Error.WriteLine("On correct floor");
					targetPos = exitPos;
				}
				else
				{
					targetPos = positions[cloneFloor];
					if (targetPos < 0)
					{
						Console.Error.WriteLine("Create elevator at: {0}", targetPos);
						Console.WriteLine("ELEVATOR");
						positions[cloneFloor] = clonePos;
						continue;
					}
				}

				if (clonePos == targetPos)
				{
					Console.Error.WriteLine("At elevator: {0}", targetPos);
					Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				}
				else if ((direction == "RIGHT") ^ (clonePos > targetPos))
				{
					Console.Error.WriteLine("Traveling to {0}", targetPos);
					Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				}
				else
				{
					Console.Error.WriteLine("Blocking at {0}", targetPos);
					Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
				}
			}
		}
	}

	private static int closestElevator(ILookup<int, int> elevatorsLookup, int cloneFloor, int clonePos)
	{
		int targetPos;
		targetPos = elevatorsLookup[cloneFloor]
			.Select(x => new { Pos = x, Distance = Math.Abs(clonePos - x) })
			.OrderBy(x => x.Distance)
			.First()
			.Pos;
		return targetPos;
	}
}