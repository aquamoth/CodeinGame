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
		Console.Error.WriteLine(string.Join(" ", inputs));
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
			Console.Error.WriteLine(string.Join(" ", inputs));
			int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
			int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor
			elevators.Add(new Tuple<int,int>(elevatorFloor, elevatorPos));
		}

		var elevatorsLookup = elevators.ToLookup(x=>x.Item1, x=>x.Item2);

		int[] bestPath = null;

		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			Console.Error.WriteLine(string.Join(" ", inputs));
			int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
			int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
			string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT



			if (bestPath == null)
			{
				Console.Error.WriteLine("Calculating paths");

				var paths = pathsFrom(0, clonePos, exitFloor, exitPos, elevatorsLookup, nbAdditionalElevators).ToArray();
				var pathsWithDistances = paths.Select(path =>
				{
					var distance = Math.Abs(Math.Abs(path[0]) - clonePos);
					for (var i = 1; i < path.Length; i++)
					{
						distance += Math.Abs(Math.Abs(path[i]) - Math.Abs(path[i - 1]));
					}
					return new
					{
						Path = path,
						Distance = distance
					};
				}).ToArray();
				Console.Error.WriteLine("Considering following paths:");
				foreach (var path in pathsWithDistances.OrderBy(x => x.Distance))
				{
					Console.Error.WriteLine("Distance {0}:\t{1}", path.Distance, string.Join(", ", path.Path));
				}

				bestPath = pathsWithDistances.OrderBy(x => x.Distance).First().Path;
			}


			if (cloneFloor == -1)
			{
				Console.Error.WriteLine("No clone to control right now");
				Console.WriteLine("WAIT"); // action: WAIT or BLOCK
			}
			else
			{
				var targetPos = bestPath[cloneFloor];
				if (clonePos == -targetPos)
				{
					Console.Error.WriteLine("Creating elevator at {0}", clonePos);
					Console.WriteLine("ELEVATOR");
					bestPath[cloneFloor] = -bestPath[cloneFloor];
				}
				else if (clonePos == targetPos)
				{
					Console.Error.WriteLine("Reached elevator at {0}", clonePos);
					Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				}
				else if ((direction == "RIGHT") ^ (clonePos > Math.Abs(targetPos)))
				{
					Console.Error.WriteLine("Traveling to {0}", targetPos);
					Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				}
				else
				{
					Console.Error.WriteLine("Blocking at {0} so clones can reach {1}", clonePos, targetPos);
					Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
				}
			}
		}
	}

	private static IEnumerable<int[]> pathsFrom(int fromFloor, int fromPos, int toFloor, int toPos, ILookup<int, int> elevatorsLookup, int nbAdditionalElevators)
	{
		if (fromFloor == toFloor)
		{
			//On the right floor, so just go to exit
			yield return new[] { toPos };
		}
		else
		{
			//Try paths for any existing elevator on the floor
			var elevatorPositions = elevatorsLookup[fromFloor];
			foreach (var position in elevatorPositions)
			{
				var paths = pathsFrom(fromFloor + 1, position, toFloor, toPos, elevatorsLookup, nbAdditionalElevators).ToArray();
				foreach (var path in paths)
				{
					yield return new[] { position }.Concat(path).ToArray();
				}
			}

			//If there are additional elevators available, try using one of them
			if (nbAdditionalElevators > 0)
			{
				var paths = pathsFrom(fromFloor + 1, fromPos, toFloor, toPos, elevatorsLookup, nbAdditionalElevators - 1).ToArray();
				foreach (var path in paths)
				{
					yield return new[] { -fromPos }.Concat(path).ToArray();
				}
			}
		}
	}

}