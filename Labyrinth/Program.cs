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
	const char TELEPORTER = 'T';
	const char CONTROL_ROOM = 'C';
	const char PATH_UNVISITED = '.';
	const char PATH_VISITED = '1';
	const char DEAD_END = '#';
	const char BEST_PATH = '*';

	static void Main(string[] args)
	{
		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int R = int.Parse(inputs[0]); // number of rows.
		int C = int.Parse(inputs[1]); // number of columns.
		int A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.


		var travelMap = Enumerable.Repeat(string.Join("", Enumerable.Repeat(PATH_UNVISITED, C)), R).ToArray();
		//printMap(travelMap);

		bool controlRoomFound = false;

		// game loop
		Direction lastDirection = Direction.NONE;
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');

			var newMap = new List<string>();
			int KR = int.Parse(inputs[0]); // row where Kirk is located.
			int KC = int.Parse(inputs[1]); // column where Kirk is located.
			for (int i = 0; i < R; i++)
			{
				string ROW = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
				newMap.Add(ROW);
			}
			var map = newMap.ToArray();

			var position = new Point { X = KC, Y = KR };

			if (get(map, position) == CONTROL_ROOM)
			{
				Console.Error.WriteLine("Found the control room. Hurry back!");
				controlRoomFound = true;
			}

			Direction direction;
			if (controlRoomFound)
			{
				Console.Error.WriteLine("Going back from control room");
				toggleVisited(travelMap, position, BEST_PATH); //Ensure we don't walk back here
				direction = directionsTo(map, travelMap, position, PATH_VISITED).Single();
			}
			else
			{
				toggleVisited(travelMap, position, PATH_VISITED);
				var possibleDirections = directionsTo(map, travelMap, position, PATH_UNVISITED);
				if (possibleDirections.Any())
				{
					direction = possibleDirections.First();
				}
				else
				{
					toggleVisited(travelMap, position, DEAD_END);
					possibleDirections = directionsTo(map, travelMap, position, PATH_VISITED);
					if (possibleDirections.Count() == 1)
					{
						direction = possibleDirections.First();
					}
					else
					{
						direction = reverse(lastDirection);
						//Just a sanity-check that possibleDirections contains the reverse of lastDirection
						if (!possibleDirections.Contains(direction))
						{
							throw new ApplicationException("Expected " + direction + " to be one of the possible directions.");
						}
					}
				}
			}

			//printMap(travelMap);
			Console.WriteLine(direction.ToString()); // Kirk's next move (UP DOWN LEFT or RIGHT).
			lastDirection = direction;
		}
	}

	private static IEnumerable<Direction> directionsTo(string[] map, string[] travelMap, Point position, char searchedToken)
	{
		var tokens = directionTokensOf(map, travelMap, position);
		for (var direction = 0; direction < 4; direction++)
		{
			if (tokens[direction] == searchedToken)
				yield return (Direction)direction;
		}
	}

	private static Direction reverse(Direction lastDirection)
	{
		return (Direction)(((int)lastDirection + 2) % 4);
	}

	//private static Point step(Point position, Direction direction)
	//{
	//	switch (direction)
	//	{
	//		case Direction.UP: return new Point { X = position.X, Y = position.Y - 1 };
	//		case Direction.RIGHT: return new Point { X = position.X + 1, Y = position.Y };
	//		case Direction.DOWN: return new Point { X = position.X, Y = position.Y + 1 };
	//		case Direction.LEFT: return new Point { X = position.X - 1, Y = position.Y };
	//		default:
	//			throw new NotSupportedException();
	//	}
	//}

	private static List<char> directionTokensOf(string[] map, string[] travelMap, Point position)
	{
		var mapDirections = new List<char>(new[] { 
				merge(map, travelMap, position.X, position.Y- 1), 
				merge(map, travelMap, position.X+1, position.Y), 
				merge(map, travelMap, position.X, position.Y+ 1), 
				merge(map, travelMap, position.X-1, position.Y)
			});
		return mapDirections;
	}

	//static IEnumerable<char> tokensOf(string[] map, int x, int y)
	//{
	//	yield return map[y - 1][x];
	//	yield return map[y][x + 1];
	//	yield return map[y + 1][x];
	//	yield return map[y][x - 1];
	//}

	//private static Direction selectDirection(List<char> mapDirections, Point position)
	//{
	//	//TODO: IF we see the exit on the map, prefer direction towards it!

	//	//Console.Error.WriteLine("Still searching");
	//	foreach (var token in new[] { CONTROL_ROOM, PATH_UNVISITED, PATH_VISITED, TELEPORTER, DEAD_END })
	//	{
	//		Console.Error.WriteLine("Searching for token: " + token);
	//		var heading = (Direction)mapDirections.IndexOf(token);
	//		if (heading != Direction.NONE)
	//		{
	//			return heading;
	//		}
	//	}
	//	return Direction.NONE;
	//}

	private static char merge(string[] map, string[] travelMap, int x, int y)
	{
		var mapToken = map[y][x];
		if (mapToken != PATH_UNVISITED && mapToken != TELEPORTER && mapToken != CONTROL_ROOM)
			return mapToken;
		else
			return travelMap[y][x];
	}


	private static char get(string[] map, Point position)
	{
		return map[position.Y][position.X];
	}

	private static void toggleVisited(string[] travelMap, Point position, char token)
	{
		Console.Error.WriteLine("Toggle (" + position.X + ", " + position.Y + ") from " + get(travelMap, position) + " to " + token);

		var left = position.X == 0 ? "" : travelMap[position.Y].Substring(0, position.X);
		var right = position.X == travelMap[position.Y].Length - 1 ? "" : travelMap[position.Y].Substring(position.X + 1);
		travelMap[position.Y] = left + token.ToString() + right;
	}

	private static void printMap(string[] travelMap)
	{
		foreach (var row in travelMap)
		{
			Console.Error.WriteLine(row);
		}
	}

	public enum Direction
	{
		NONE = -1,
		UP = 0,
		RIGHT = 1,
		DOWN = 2,
		LEFT = 3
	}
}

public class Point
{
	public int X { get; set; }
	public int Y { get; set; }
}