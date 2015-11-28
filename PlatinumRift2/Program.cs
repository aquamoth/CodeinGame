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
		var gameState = loadInitialStateFromConsole();

		// game loop
		while (true)
		{
			updateGameState(gameState);
			
			var commands = processSquads(gameState);

			Console.WriteLine(string.Join(" ", commands.ToArray())); // first line for movement commands, second line no longer used (see the protocol in the statement for details)
			Console.WriteLine("WAIT");
		}
	}

	private static List<string> processSquads(GameState gameState)
	{
		var commands = new List<string>();

		var squads = gameState.Squads.ToArray(); //.ToArray() to allow the squads to split or join
		foreach (var squad in squads)
		{
			squad.BeforeMove(gameState);
		}
		foreach (var squad in squads)
		{
			commands.AddRange(squad.Move(gameState));
		}
		foreach (var squad in squads)
		{
			squad.AfterMove(gameState);
		}
		return commands;
	}

	#region Game State

	private static void updateGameState(GameState gameState)
	{
		updateGameStateFromConsole(gameState);

		updatePodSquads(gameState);

		if (gameState.IsFirstTurn)
			firstTurnInitialization(gameState);
	}

	private static void firstTurnInitialization(GameState gameState)
	{
		Console.Error.WriteLine("Detecting base zones");
		gameState.MyBase = gameState.Zones.Where(x => x.OwnerId == gameState.MyId).Single().Id;
		gameState.TheirBase = gameState.Zones.Where(x => x.OwnerId == (1 - gameState.MyId)).Single().Id;

		//foreach (var zone in gameState.Zones.Where(x=>x.Visible))
		//{
		//	Console.Error.WriteLine(zone);
		//}
	}

	private static void updatePodSquads(GameState gameState)
	{
		gameState.Squads.Clear(); // Delete any old squad states and start over. This should be removed once foreach(var squad in squads) works below

		var zonesWithMyPods = gameState.Zones.Where(x => x.MyPods > 0);
		foreach (var zone in zonesWithMyPods)
		{
			var podsToDeploy = zone.MyPods;

			var squads = gameState.Squads.Where(s => s.ZoneId == zone.Id);
			foreach (var squad in squads)
			{
				throw new NotImplementedException("No support for updating existing squads at this time");
			}
			if (podsToDeploy > 0)
			{
				if (squads.Any())
				{
					throw new ApplicationException("New squads should be distributed evenly into existing squads");
				}
				else
				{
					gameState.Squads.Add(createSquad(zone.Id, podsToDeploy));
				}
			}
		}
	}

	#region Read State from Console

	private static GameState loadInitialStateFromConsole()
	{
		var gameState = new GameState();

		string[] inputs;

		inputs = Console.ReadLine().Split(' ');

		gameState.PlayerCount = int.Parse(inputs[0]); // the amount of players (always 2)
		gameState.MyId = int.Parse(inputs[1]); // my player ID (0 or 1)
		//Console.Error.WriteLine("MyId: {0}, Platinum: {1}", gameState.MyId, gameState.MyPlatinum);

		int zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
		int linkCount = int.Parse(inputs[3]); // the amount of links between all zones

		gameState.Zones = new Zone[zoneCount];
		for (int i = 0; i < zoneCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int zoneId = int.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
			int platinumSource = int.Parse(inputs[1]); // Because of the fog, will always be 0
			gameState.Zones[i] = new Zone(zoneId) { PlatinumSource = platinumSource };
		}
		for (int i = 0; i < linkCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int zone1 = int.Parse(inputs[0]);
			int zone2 = int.Parse(inputs[1]);
			gameState.Zones[zone1].Neighbours.Add(zone2);
			gameState.Zones[zone2].Neighbours.Add(zone1);
		}

		return gameState;
	}

	private static void updateGameStateFromConsole(GameState gameState)
	{
		gameState.MyPlatinum = int.Parse(Console.ReadLine()); // your available Platinum

		//Console.Error.WriteLine(string.Format("Updating {0} zone states.", gameState.Zones.Length));
		for (int i = 0; i < gameState.Zones.Length; i++)
		{
			var str = Console.ReadLine();
			//Console.Error.WriteLine(str);
			var inputs = str.Split(' ');
			int zId = int.Parse(inputs[0]); // this zone's ID
			//if (zId != i)
			//	Console.Error.WriteLine("zID!=i: " + zId + ", " + i);
			gameState.Zones[zId].OwnerId = int.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
	
			var podsp0 = int.Parse(inputs[2]); // player 0's PODs on this zone
			var podsp1 = int.Parse(inputs[3]); // player 1's PODs on this zone
			gameState.Zones[zId].MyPods = gameState.MyId == 0 ? podsp0 : podsp1;
			gameState.Zones[zId].TheirPods = gameState.MyId != 0 ? podsp0 : podsp1;

			gameState.Zones[zId].Visible = int.Parse(inputs[4]) == 1; // 1 if one of your units can see this tile, else 0
			gameState.Zones[zId].PlatinumSource = int.Parse(inputs[5]); // the amount of Platinum this zone can provide (0 if hidden by fog)

		}
	}

	#endregion Read State from Console

	#endregion Game State

	private static IPodSquad createSquad(int zoneId, int podsToDeploy)
	{
		return new MazeRunner { ZoneId = zoneId, Pods = podsToDeploy };
	}
}


public class GameState
{
	public Zone[] Zones { get; set; }

	public int PlayerCount { get; set; }
	public int MyId { get; set; }
	public int MyPlatinum { get; set; }

	public int MyBase { get; set; }
	public int TheirBase { get; set; }
	public bool IsFirstTurn { get { return MyBase == -1; } }

	public List<IPodSquad> Squads { get; private set; }

	public GameState()
	{
		PlayerCount = 0;
		MyId = -1;
		MyBase = -1;
		TheirBase = -1;
		Squads = new List<IPodSquad>();
	}
}

public class Zone
{
	public int Id { get; private set; }
	public int OwnerId { get; set; }
	public int MyPods { get; set; }
	public int TheirPods { get; set; }
	public bool Visible { get; set; }
	public int PlatinumSource { get; set; }

	public int MazeVisitedCount { get; set; }

	public List<int> Neighbours { get; private set; }

	public Zone(int id)
	{
		Id = id;
		OwnerId = -1;
		Neighbours = new List<int>();
	}

	public override string ToString()
	{
		return string.Format("#{0}: Owner {1}, Visible: {2}, Platinum: {3}, Pods: {4}/{5}, Exits: {6}", Id, OwnerId, Visible, PlatinumSource, MyPods, TheirPods, string.Join(", ", Neighbours.ToArray()));
	}
}


public interface IPodSquad
{
	int ZoneId { get; }
	int Pods { get; }

	void BeforeMove(GameState gameState);
	IEnumerable<string> Move(GameState gameState);
	void AfterMove(GameState gameState);
}

public class MazeRunner : IPodSquad
{
	public int ZoneId { get; set; }
	public int Pods { get; set; }

	public void BeforeMove(GameState gameState)
	{
		var currentZone = gameState.Zones[ZoneId];

		if (currentZone.MazeVisitedCount == 0)
			currentZone.MazeVisitedCount = 1;
	}

	public IEnumerable<string> Move(GameState gameState)
	{
		var currentZone = gameState.Zones[ZoneId];

		var unvisitedNeighbours = currentZone.Neighbours
			.Where(id => gameState.Zones[id].MazeVisitedCount == 0)
			.ToArray();
		Console.Error.WriteLine(string.Format("Squad of {0} pods at #{1} has {2} unvisited neighbours.", Pods, ZoneId, unvisitedNeighbours.Length));
		if (unvisitedNeighbours.Any())
		{
			var zonesToVisit = unvisitedNeighbours.Count();
			for (int i = 0; i < zonesToVisit - 1; i++)
			{
				if (Pods == 1)
				{
					Console.Error.WriteLine("Only one pod left in the squad. Sending it to last zone.");
					break;
				}
				var podsInGroup = (int)Math.Ceiling(Pods / (double)(zonesToVisit - i));
				var squad = new MazeRunner { ZoneId = unvisitedNeighbours[i], Pods = podsInGroup };
				gameState.Squads.Add(squad);
				this.Pods -= podsInGroup;

				Console.Error.WriteLine(string.Format("{0} pods at zone #{1} moves to zone #{2}.", squad.Pods, currentZone.Id, squad.ZoneId));
				if (gameState.Zones[squad.ZoneId].MazeVisitedCount == 0)
					gameState.Zones[squad.ZoneId].MazeVisitedCount = 1;
				yield return string.Format("{0} {1} {2}", squad.Pods, currentZone.Id, squad.ZoneId);
			}
			//We keep ourselves around for the last neighbour
			this.ZoneId = unvisitedNeighbours.Last();
			Console.Error.WriteLine(string.Format("{0} pods at zone #{1} moves to zone #{2}.", this.Pods, currentZone.Id, this.ZoneId));
			if (gameState.Zones[this.ZoneId].MazeVisitedCount == 0)
				gameState.Zones[this.ZoneId].MazeVisitedCount = 1;
			yield return string.Format("{0} {1} {2}", this.Pods, currentZone.Id, this.ZoneId);
		}
		else
		{
			Console.Error.WriteLine("Backtracking hasn't been implemented yet.");
		}
	}

	public void AfterMove(GameState gameState)
	{
	}
}









//public class Link
//{
//	public int A { get; set; }
//	public int B { get; set; }

//	public int[] Nodes { get { return new[] { A, B }; } }

//	//public override bool Equals(object obj)
//	//{
//	//	var other = obj as Link;
//	//	return obj != null
//	//		&& (
//	//			(this.A.Equals(other.A) && this.B.Equals(other.B))
//	//		|| (this.A.Equals(other.B) && this.B.Equals(other.A))
//	//		);
//	//}

//	//public override int GetHashCode()
//	//{
//	//	return A.GetHashCode() ^ B.GetHashCode();
//	//}

//	public override string ToString()
//	{
//		return A + " " + B;
//	}

//	//public static Direction DirectionOf(int from, int to)
//	//{
//	//	if (to == from + 1)
//	//		return Direction.RIGHT;
//	//	else if (to == from - 1)
//	//		return Direction.LEFT;
//	//	else if (to < from)
//	//		return Direction.UP;
//	//	else if (to > from)
//	//		return Direction.DOWN;
//	//	else
//	//		throw new NotSupportedException();
//	//}

//}
