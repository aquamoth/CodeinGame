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
	const char PATH_VISITED_ONCE = '1';
	const char PATH_VISITED_TWICE = '2';
	const char DEAD_END = '#';

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

			if (map[KR][KC] == CONTROL_ROOM)
			{
				Console.Error.WriteLine("Found the control room. Hurry back!");
				//toggleVisited(travelMap, KC, KR, DEAD_END); //Ensure we don't walk back here
				controlRoomFound = true;
			}

			toggleVisited(travelMap, KC, KR);
			//printMap(travelMap);

			var mapDirections = getDirectionTokens(travelMap, KR, KC, map);
			var direction = selectDirection(mapDirections, travelMap, KC, KR, controlRoomFound);

			if (mapDirections[(int)direction] == PATH_VISITED_ONCE)
			{
				Console.Error.WriteLine("Back-tracing. Double-marking current location.");
				toggleVisited(travelMap, KC, KR, PATH_VISITED_TWICE);
			}

			Console.WriteLine(direction.ToString()); // Kirk's next move (UP DOWN LEFT or RIGHT).
		}
	}

	private static List<char> getDirectionTokens(string[] travelMap, int KR, int KC, string[] map)
	{
		var mapDirections = new List<char>(new[] { 
				merge(map, travelMap, KC, KR - 1), 
				merge(map, travelMap, KC+1, KR), 
				merge(map, travelMap, KC, KR + 1), 
				merge(map, travelMap, KC-1, KR)
			});
		return mapDirections;
	}

	static IEnumerable<char> tokensOf(string[] map, int x, int y)
	{
		yield return map[y - 1][x];
		yield return map[y][x + 1];
		yield return map[y + 1][x];
		yield return map[y][x - 1];
	}

	private static Direction selectDirection(List<char> mapDirections, string[] travelMap, int x, int y, bool traceBackToStart)
	{
		//TODO: IF we see the exit on the map, prefer direction towards it!

		if (traceBackToStart)
		{
			//Console.Error.WriteLine("Back-tracking");
			return (Direction)(tokensOf(travelMap, x, y).ToList().IndexOf(PATH_VISITED_ONCE));
		}
		else
		{
			//Console.Error.WriteLine("Still searching");
			foreach (var token in new[] { CONTROL_ROOM, PATH_UNVISITED, PATH_VISITED_ONCE, PATH_VISITED_TWICE })
			{
				var heading = (Direction)mapDirections.IndexOf(token);
				if (heading != Direction.NONE)
				{
					//if (token == PATH_VISITED_TWICE)
					//{
					//	Console.Error.WriteLine("We're in a dead-end. Ensure the current pos is marked as double-visited.");
					//	toggleVisited(travelMap, x, y, DEAD_END);
					//}
					return heading;
				}
			}
			return Direction.NONE;
		}
	}

	private static char merge(string[] map, string[] travelMap, int x, int y)
	{
		if (map[y][x] != PATH_UNVISITED)
			return map[y][x];
		else
			return travelMap[y][x];
	}


	private static void toggleVisited(string[] travelMap, int x, int y)
	{
		var token = travelMap[y][x];
		Console.Error.Write("Toggle (" + x +", "+ y+") from " + token + " to: ");
		switch (token)
		{
			case PATH_UNVISITED: token = PATH_VISITED_ONCE; break;
			case PATH_VISITED_ONCE: token = PATH_VISITED_ONCE; break;
			case PATH_VISITED_TWICE: token = PATH_VISITED_TWICE; break;
			//case PATH_VISITED_ONCE: token = PATH_VISITED_TWICE; break;
			//case PATH_VISITED_TWICE: token = DEAD_END; break;
			default:
				throw new NotImplementedException();
		}
		Console.Error.WriteLine(token);
		toggleVisited(travelMap, x, y, token);
	}

	private static void toggleVisited(string[] travelMap, int x, int y, char token)
	{
		var left = x == 0 ? "" : travelMap[y].Substring(0, x);
		var right = x == travelMap[y].Length - 1 ? "" : travelMap[y].Substring(x + 1);
		travelMap[y] = left + token.ToString() + right;
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