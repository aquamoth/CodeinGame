﻿using System;
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

		Queue<Command> bestPath = null;

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

				//, 
				var paths = pathsFrom(0, clonePos, direction == "RIGHT", exitFloor, exitPos, elevatorsLookup, nbAdditionalElevators).ToArray();

	
				//var pathsWithDistances = paths.Select(path =>
				//{
				//	var distance = Math.Abs(Math.Abs(path[0]) - clonePos);
				//	for (var i = 1; i < path.Length; i++)
				//	{
				//		distance += Math.Abs(Math.Abs(path[i]) - Math.Abs(path[i - 1]));
				//	}
				//	return new
				//	{
				//		Path = path,
				//		Distance = distance
				//	};
				//}).ToArray();
				Console.Error.WriteLine("Considering following paths:");
				//foreach (var path in pathsWithDistances.OrderBy(x => x.Distance))
				foreach (var path in paths)
				{
					Console.Error.WriteLine(string.Join(", ", path.Select(x => x.ToString()).ToArray()));
				}

				//bestPath = pathsWithDistances.OrderBy(x => x.Distance).First().Path;
				bestPath = new Queue<Command>(paths.First());
			}


			if (cloneFloor == -1)
			{
				Console.Error.WriteLine("No clone to control right now");
				Console.WriteLine("WAIT");
			}
			else if (!bestPath.Any())
			{
				Console.Error.WriteLine("No more commands are required");
				Console.WriteLine("WAIT");
			}
			else
			{
				var nextCommand = bestPath.Peek();
				if (cloneFloor != nextCommand.Floor)
				{
					Console.Error.WriteLine("Waiting to elevate to floor " + nextCommand.Floor);
					Console.WriteLine("WAIT");
				}
				else if (clonePos != nextCommand.Position)
				{
					Console.Error.WriteLine("Waiting to reach position " + nextCommand.Position);
					Console.WriteLine("WAIT");
				}
				else
				{
					Console.Error.WriteLine("Executing {0}", nextCommand);
					Console.WriteLine(nextCommand.Action);
					bestPath.Dequeue();
				}
				//var targetPos = bestPath[cloneFloor];
				//if (clonePos == -targetPos)
				//{
				//	Console.Error.WriteLine("Creating elevator at {0}", clonePos);
				//	Console.WriteLine("ELEVATOR");
				//	bestPath[cloneFloor] = -bestPath[cloneFloor];
				//}
				//else if (clonePos == targetPos)
				//{
				//	Console.Error.WriteLine("Reached elevator at {0}", clonePos);
				//	Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				//}
				//else if (hasCorrectDirection(clonePos, targetPos, direction == "RIGHT"))
				//{
				//	Console.Error.WriteLine("Traveling to {0}", targetPos);
					//Console.WriteLine("WAIT"); // action: WAIT or BLOCK
				//}
				//else
				//{
				//	Console.Error.WriteLine("Blocking at {0} so clones can reach {1}", clonePos, targetPos);
				//	Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
				//}
			}
		}
	}

	private static bool hasCorrectDirection(int clonePos, int targetPos, bool travellingRight)
	{
		return (travellingRight) ^ (clonePos > Math.Abs(targetPos));
	}

	private static IEnumerable<Command[]> pathsFrom(int currentFloor, int currentPosition, bool headingRight, int exitFloor, int exitPosition, ILookup<int, int> elevatorsPerFloor, int additionalElevators)
	{
		Console.Error.WriteLine("Evaluating paths from {0}/{1}->{2}", currentFloor, currentPosition, headingRight ? "Right" : "Left");
		if (currentFloor == exitFloor)
		{
			Console.Error.WriteLine("This is the exit floor");
			var commands = new List<Command>();
			if (exitPosition != currentPosition)
			{
				var exitIsToRight = exitPosition > currentPosition;
				if (exitIsToRight ^ headingRight)
					commands.Add(new Command(currentFloor, currentPosition, "BLOCK"));

				commands.Add(new Command(exitFloor, exitPosition, "WAIT"));
			}
			yield return commands.ToArray();
		}
		else
		{
			//Try paths for any existing elevator on the floor
			var elevatorPositions = elevatorsPerFloor[currentFloor];
			Console.Error.WriteLine("Found {0} elevators on floor {1}", elevatorPositions.Count(), currentFloor);
			foreach (var elevatorPosition in elevatorPositions)
			{
				Console.Error.WriteLine("Evaluating elevator at {0}/{1}", currentFloor, elevatorPosition);
				var leaveFloorHeadingRight = headingRight;
				var optionalBlockCommand = new Command[0];
				if (currentPosition != elevatorPosition)
				{
					var elevatorIsToRight = elevatorPosition > currentPosition;
					if (elevatorIsToRight ^ headingRight)
					{
						optionalBlockCommand = new[] { 
							new Command(currentFloor, currentPosition, "BLOCK"), 
						};
						leaveFloorHeadingRight = !leaveFloorHeadingRight;
					}
				}

				var levelCommands = optionalBlockCommand
					.Concat(new[]{ new Command(currentFloor, elevatorPosition, "WAIT") })
					.ToArray();

				var paths = pathsFrom(currentFloor + 1, elevatorPosition, leaveFloorHeadingRight, exitFloor, exitPosition, elevatorsPerFloor, additionalElevators).ToArray();
				foreach (var path in paths)
				{
					yield return levelCommands.Concat(path).ToArray();
				}
			}

			//If there are additional elevators available, try using one of them
			if (additionalElevators > 0)
			{
				Console.Error.WriteLine("Evaluating a new elevator (of {2}) at {0}/{1}", currentFloor, currentPosition, additionalElevators);
				var levelCommands = new[] { new Command(currentFloor, currentPosition, "ELEVATOR") };
				var paths = pathsFrom(currentFloor + 1, currentPosition, headingRight, exitFloor, exitPosition, elevatorsPerFloor, additionalElevators - 1).ToArray();
				foreach (var path in paths)
				{
					yield return levelCommands.Concat(path).ToArray();
				}
			}
		}
	}

}

class Command
{
	public int Floor { get; set; }
	public int Position { get; set; }
	public string Action { get; set; }

	public Command(int floor, int position, string action)
	{
		Floor = floor;
		Position = position;
		Action = action;
	}

	public override string ToString()
	{
		return string.Format("[{0}/{1} {2}]", Floor, Position, Action);
	}
}