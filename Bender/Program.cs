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
class Solution
{
	static void Main(string[] args)
	{
		string[] inputs = Console.ReadLine().Split(' ');
		int L = int.Parse(inputs[0]);
		int C = int.Parse(inputs[1]);

		Bender bender = null;

		var map = new List<MapPoint[]>();
		for (int i = 0; i < L; i++)
		{
			string row = Console.ReadLine();
			map.Add(row.Select(c => new MapPoint { Symbol = c }).ToArray());

			var startIndex = row.IndexOf("@");
			if (startIndex >= 0)
			{
				bender = new Bender { X = startIndex, Y = i, Heading = Direction.SOUTH };
			}
		}

		var movements = new List<Direction>();
		var visitedNodes = new List<Point>();

		var stateChange = StateChanges.None;
		var hasLoop = false;
		do
		{
			visitedNodes.Add(bender);
			bender = bender.Clone();
			bender.step();
			var inputHeading = bender.Heading;
			stateChange = applyStateChange(bender, map);
			if (stateChange != StateChanges.None && visitedNodes.Contains(bender))
			{
				hasLoop = true;
				break;
			}
			else if (stateChange == StateChanges.Turned)
			{
				if (visitedNodes.Contains(bender))
					hasLoop = true;
			}
			else
			{
				movements.Add(inputHeading);
			}
		}
		while (!hasLoop && stateChange != StateChanges.Killed);

		if (hasLoop)
		{
			Console.WriteLine("Loop");
		}
		else
		{
			foreach (var movement in movements)
			{
				Console.WriteLine(movement.ToString());
			}
		}
	}

	private static StateChanges applyStateChange(Bender bender, List<MapPoint[]> map)
	{
		var mapPoint = map[bender.Y][bender.X];
		//Console.Error.WriteLine(mapPoint.Symbol);
		switch (mapPoint.Symbol)
		{
			case ' ':
			case '@':
				break;
			case '$':
				return StateChanges.Killed;
			case 'X':
				if (bender.IsBenderMode)
				{
					mapPoint.Symbol = ' ';
					break;
				}
				else
				{
					bender.stepBack();
					bender.turn(map);
					return StateChanges.Turned;
				}
			case '#':
				bender.stepBack();
				bender.turn(map);
				return StateChanges.Turned;
			case 'B':
				bender.IsBenderMode = !bender.IsBenderMode;
				if (bender.IsBenderMode)
					mapPoint.Symbol = ' ';
				break;

			case 'I':
				bender.IsInvertedDirections = !bender.IsInvertedDirections;
				//mapPoint.Symbol = '-';
				break;

			case 'T':
				for (int y = 0; y < map.Count; y++)
				{
					for (int x = 0; x < map[y].Length; x++)
					{
						if (x != bender.X || y != bender.Y)
						{
							if (map[y][x].Symbol == 'T')
							{
								bender.X = x;
								bender.Y = y;
								return StateChanges.None;
							}
						}
					}
				}
				throw new ApplicationException("Found only ONE teleporter!");

			case 'S': bender.Heading = Direction.SOUTH; break;
			case 'E': bender.Heading = Direction.EAST; break;
			case 'N': bender.Heading = Direction.NORTH; break;
			case 'W': bender.Heading = Direction.WEST; break;

			default:
				throw new NotImplementedException("Unknown mark: " + mapPoint.Symbol);
		}
		return StateChanges.None;
	}
}

class Point
{
	public int X { get; set; }
	public int Y { get; set; }
}

class Bender : Point, ICloneable
{
	public Direction Heading { get; set; }
	public bool IsBenderMode { get; set; }
	public bool IsInvertedDirections { get; set; }


	public void step()
	{
		switch (Heading)
		{
			case Direction.SOUTH: Y = Y + 1; break;
			case Direction.EAST: X = X + 1; break;
			case Direction.NORTH: Y = Y - 1; break;
			case Direction.WEST: X = X - 1; break;
			default:
				throw new NotSupportedException();
		}
	}

	public void stepBack()
	{
		switch (Heading)
		{
			case Direction.SOUTH: Y = Y - 1; break;
			case Direction.EAST: X = X - 1; break;
			case Direction.NORTH: Y = Y + 1; break;
			case Direction.WEST: X = X + 1; break;
			default:
				throw new NotSupportedException();
		}
	}

	public void turn(List<MapPoint[]> map)
	{
		if (IsInvertedDirections)
		{
			if (!map[Y][X - 1].IsObstacle)
				Heading = Direction.WEST;
			else if (!map[Y - 1][X].IsObstacle)
				Heading = Direction.NORTH;
			else if (!map[Y][X + 1].IsObstacle)
				Heading = Direction.EAST;
			else if (!map[Y + 1][X].IsObstacle)
				Heading = Direction.SOUTH;
			else 
				throw new NotSupportedException("No where to turn!");
		}
		else
		{
			if (!map[Y + 1][X].IsObstacle)
				Heading = Direction.SOUTH;
			else if (!map[Y][X + 1].IsObstacle)
				Heading = Direction.EAST;
			else if (!map[Y - 1][X].IsObstacle)
				Heading = Direction.NORTH;
			else if (!map[Y][X - 1].IsObstacle)
				Heading = Direction.WEST;
			else
				throw new NotSupportedException("No where to turn!");
		}
	}

	object ICloneable.Clone()
	{
		return this.MemberwiseClone();
	}
	public Bender Clone()
	{
		return (Bender)(this as ICloneable).Clone();
	}

}

enum StateChanges
{
	None,
	Turned,
	Killed
}

enum Direction
{
	SOUTH = 0,
	EAST = 1,
	NORTH = 2,
	WEST = 3
}

class MapPoint
{
	public Char Symbol { get; set; }
	public List<Direction> VisitedDirections { get; set; }

	public bool IsObstacle { get { return new[] { '#', 'X' }.Contains(Symbol); } }
}