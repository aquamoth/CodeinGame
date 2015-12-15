using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
	static void Main(string[] args)
	{
		var sw0 = new Stopwatch();
		var sw1 = new Stopwatch();
		var sw2 = new Stopwatch();


		sw0.Start();
		int N = int.Parse(Console.ReadLine());
		var rooms = new Room[N];
		for (int i = 0; i < N; i++)
		{
			string room = Console.ReadLine();
			rooms[i] = new Room(room);
		}
		foreach (var room in rooms)
		{
			room.Exits = room.Neighbours.Where(id => id != "E").Select(id => rooms[int.Parse(id)]).ToArray();
		}


		var untestedPositions = new Queue<Position>();

		var startingRoom = rooms[0];
		var position = new Position
		{
			At = startingRoom,
			RoomsLeft = new HashSet<Room>(rooms.Except(new[] { startingRoom })),
			MoneyFound = startingRoom.Money
		};
		untestedPositions.Enqueue(position);
		sw0.Stop();


		sw1.Start();
		Position bestTrack = null;
		while (untestedPositions.Any())
		{
			position = untestedPositions.Dequeue();
			var unvisitedNeighbours = position.RoomsLeft.Where(r => position.At.Exits.Contains(r)).ToArray();
			if (unvisitedNeighbours.Any())
			{
				sw2.Start();
				foreach (var room in unvisitedNeighbours)
				{
					var nextPosition = new Position
					{
						At = room,
						MoneyFound = position.MoneyFound + room.Money,
						RoomsLeft = position.RoomsLeft.Except(new[] { room })
					};
					untestedPositions.Enqueue(nextPosition);
				}
				sw2.Stop();
			}
			else
			{
				if (bestTrack == null || position.MoneyFound > bestTrack.MoneyFound)
					bestTrack = position;
			}
		}
		sw1.Stop();

		Console.Error.WriteLine("Timer0: {0}", sw0.ElapsedMilliseconds);
		Console.Error.WriteLine("Timer1: {0}", sw1.ElapsedMilliseconds);
		Console.Error.WriteLine("Timer2: {0}", sw2.ElapsedMilliseconds);

		Console.WriteLine(bestTrack.MoneyFound);
	}
}

public class Position
{
	public Room At { get; set; }
	public IEnumerable<Room> RoomsLeft { get; set; }
	public int MoneyFound { get; set; }

	public override string ToString()
	{
		return string.Format("{0}, $${1} with {2} rooms left", At, MoneyFound, RoomsLeft.Count());
	}
}


public class Room
{
	public string Id { get; set; }
	public string[] Neighbours { get; set; }
	public Room[] Exits { get; set; }
	public int Money { get; set; }

	public Room()
	{

	}
	public Room(string addressLine):this()
	{
		var parts = addressLine.Split(' ');
		this.Id = parts[0];
		this.Money = int.Parse(parts[1]);
		this.Neighbours = new[] { parts[2], parts[3] };
	}

	public override string ToString()
	{
		return string.Format("#{0} ${1}. {2}", Id, Money, string.Join(", ", Neighbours));
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
	public override bool Equals(object obj)
	{
		return this.Id.Equals(((Room)obj).Id);
	}
}
