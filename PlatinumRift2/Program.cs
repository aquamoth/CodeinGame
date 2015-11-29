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
		if (gameState.IsFirstTurn)
			firstTurnInitialization(gameState);

		mergePodSquads(gameState);
		cleanupKilledSquads(gameState);
		updateSquadPods(gameState);
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

	private static void updateSquadPods(GameState gameState)
	{
		var zonesWithMyPods = gameState.Zones.Where(x => x.MyPods > 0);
		foreach (var zone in zonesWithMyPods)
		{
			//Console.Error.WriteLine("Refreshing squads at zone #" + zone.Id);

			var squads = gameState.Squads.Where(s => s.ZoneId == zone.Id);
			if (squads.Sum(x => x.Pods) != zone.MyPods)
			{
				if (squads.Count() == 0)
				{
					var squad = createSquad(gameState, zone.Id, zone.MyPods);
					gameState.Squads.Add(squad);
				}
				else if (squads.Count() == 1)
				{
					var squad = squads.Single();
					if (squad.Pods != zone.MyPods)
					{
						Console.Error.WriteLine(string.Format("Updating squad at zone #{0} from {1} to {2} pods.", zone.Id, squad.Pods, zone.MyPods));
						squad.Pods = zone.MyPods;
					}
				}
				else
				{
					throw new NotImplementedException("No support for multiple squads at same location at this time");
				}
			}
		}
	}

	private static void cleanupKilledSquads(GameState gameState)
	{
		foreach (var squad in gameState.Squads.ToArray())
		{
			if (gameState.Zones[squad.ZoneId].MyPods == 0)
			{
				Console.Error.WriteLine(string.Format("Squad at zone #{0} with {1} pods was killed and is removed.", squad.ZoneId, squad.Pods));
				Console.Error.WriteLine(gameState.Zones[squad.ZoneId]);
				gameState.Squads.Remove(squad);
			}
		}
	}

	private static void mergePodSquads(GameState gameState)
	{
		var squadsOfSameTypeInSameZone = gameState.Squads
			//.Select(x => new { id = x.ZoneId, type = x.GetType() })
				  .GroupBy(x => new { id = x.ZoneId, type = x.GetType() })
				  .Where(grp => grp.Count() > 1);

		foreach (var squadsToMerge in squadsOfSameTypeInSameZone)
		{
			Console.Error.WriteLine(string.Format("Merging {0} squads of type {1} at zone #{2} with a total of {3} pods.",
				squadsToMerge.Count(),
				squadsToMerge.Key.type.ToString(),
				squadsToMerge.Key.id,
				squadsToMerge.Sum(x => x.Pods)
				));
			var squadToKeep = squadsToMerge.First();
			foreach (var squad in squadsToMerge.Skip(1))
			{
				squadToKeep.Pods += squad.Pods;
				gameState.Squads.Remove(squad);
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

	private static IPodSquad createSquad(GameState gameState, int zoneId, int pods)
	{
		Console.Error.WriteLine(string.Format("Creating new squad with {0} pods at zone #{1}", pods, zoneId));
		if (gameState.Zones[zoneId].Neighbours.Where(id => gameState.Zones[id].MazeVisitedCount == 0).Any())
		{
			return new MazeRunner { ZoneId = zoneId, Pods = pods };
		}
		else
		{
			return new Torpedo(pods, zoneId, gameState.TheirBase, gameState.Zones);
		}
	}
}

#region Main objects

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
	int Pods { get; set; }

	void BeforeMove(GameState gameState);
	IEnumerable<string> Move(GameState gameState);
	void AfterMove(GameState gameState);
}

public abstract class BaseSquad : IPodSquad
{
	public int ZoneId { get; set; }
	public int Pods { get; set; }

	public BaseSquad(int pods, int zoneId)
	{
		this.Pods = pods;
		this.ZoneId = zoneId;
	}

	public virtual void BeforeMove(GameState gameState)
	{
	}
	public virtual IEnumerable<string> Move(GameState gameState)
	{
		Console.Error.WriteLine("Moving has not been implemented for squad of type " + this.GetType().ToString());
		return new string[0];
	}
	public virtual void AfterMove(GameState gameState)
	{
	}

	protected virtual string MoveTo(int toZone)
	{
		Console.Error.WriteLine(string.Format("{0} pods at zone #{1} moves to zone #{2}.", this.Pods, this.ZoneId, toZone));
		var command = string.Format("{0} {1} {2}", this.Pods, this.ZoneId, toZone);
		this.ZoneId = toZone;
		return command;
	}
}

#endregion Main objects

public class Torpedo : BaseSquad
{
	readonly Queue<int> _queue;

	public Torpedo(int pods, int zoneId, int targetZoneId, Zone[] map)
		: base(pods, zoneId)
	{
		Console.Error.WriteLine("Creating torpedo from " + zoneId + " to " + targetZoneId);
		var bfs = new Dijkstra(map);
		//Console.Error.WriteLine("Generating waypoints");
		var path = bfs.Path(zoneId, targetZoneId).Skip(1);
		Console.Error.WriteLine("Queueing waypoints: " + string.Join(", ", path));
		_queue = new Queue<int>(path);
	}

	public override IEnumerable<string> Move(GameState gameState)
	{
		if (_queue.Any())
		{
			var nextZoneId = _queue.Dequeue();
			yield return MoveTo(nextZoneId);
		}
		else
		{
			Console.Error.WriteLine("Torpedo reached its target and idles at #" + this.ZoneId);
		}
	}




	class Dijkstra
	{
		public class Node
		{
			public Node(int position)
			{
				Position = position;
				Distance = int.MaxValue;
			}

			public int Position { get; private set; }
			public Node[] Neighbours { get; set; }
			public string ShortestPath { get; set; }
			public int Distance { get; set; }
			public bool Visited { get; set; }
		}

		IDictionary<int, Node> _nodes;

		public Dijkstra(Zone[] map)
		{
			_nodes = map.Select(zone => new Node(zone.Id)).ToDictionary(x => x.Position);
			foreach (var node in _nodes.Values)
			{
				node.Neighbours = map[node.Position].Neighbours.Select(id => _nodes[id]).ToArray();
			}
		}

		public int[] Path(int from, int to)
		{
			if (!_nodes.ContainsKey(to))
				return null;//No paths to the destination at all

			if (!_nodes.ContainsKey(from))
				return null;//No paths from the source at all


			//Initialize the traversal
			var currentNode = _nodes[from];
			currentNode.Distance = 0;
			var unvisitedNodes = new List<Node>(_nodes.Values);

			do
			{
				var tentativeDistance = currentNode.Distance + 1;
				var unvisitedNeighbours = currentNode.Neighbours.Where(x => !x.Visited);

				foreach (var neighbour in unvisitedNeighbours)
				{
					if (neighbour.Distance > tentativeDistance)
					{
						neighbour.Distance = tentativeDistance;
						neighbour.ShortestPath = currentNode.ShortestPath + " " + currentNode.Position;
					}
				}

				currentNode.Visited = true;
				unvisitedNodes.Remove(currentNode);

				if (currentNode.Position == to)
					break;

				currentNode = unvisitedNodes.OrderBy(x => x.Distance).FirstOrDefault();
			}
			while (currentNode != null && currentNode.Distance != int.MaxValue);

			//Determine output
			var toNode = _nodes[to];
			if (toNode.Distance == int.MaxValue)
				return null; // No path to this gateway exists
			else
				return (currentNode.ShortestPath + " " + currentNode.Position).TrimStart().Split(' ').Select(x => int.Parse(x)).ToArray();
		}
	}
}

public class RandomRunner : BaseSquad
{
	Random r;

	public RandomRunner(int pods, int zoneId) 
		: base(pods, zoneId)
	{
		r = new Random();
	}

	public override IEnumerable<string> Move(GameState gameState)
	{
		var neighbours = gameState.Zones[this.ZoneId].Neighbours;
		var index = r.Next(neighbours.Count);
		var nextZoneId = neighbours[index];
		//Console.Error.WriteLine(string.Format("Randomly moving {0} pods from {1} to {2}", this.Pods, this.ZoneId, nextZoneId));
		//return new[] { string.Format("{0} {1} {2}", this.Pods, this.ZoneId, nextZoneId) };
		return new[] { MoveTo(nextZoneId) };
	}
}

public class MazeRunner : IPodSquad
{
	public int ZoneId { get; set; }
	public int Pods { get; set; }

	public int PreviousZoneId { get; set; }

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
		//Console.Error.WriteLine(string.Format("Squad of {0} pods at #{1} has {2} unvisited neighbours.", Pods, ZoneId, unvisitedNeighbours.Length));

		if (unvisitedNeighbours.Any())
		{
			return moveToUnvisitedZones(gameState, currentZone, unvisitedNeighbours);
		}
		else
		{
			return backtrack(gameState, currentZone);
		}
	}

	private string[] backtrack(GameState gameState, Zone currentZone)
	{
		Console.Error.WriteLine("Backtracking from zone #" + ZoneId + " by converting into a random runner");
		var squad = new RandomRunner(this.Pods, this.ZoneId);
		gameState.Squads.Remove(this);
		gameState.Squads.Add(squad);
		return squad.Move(gameState).ToArray();

//		Console.Error.WriteLine("Backtracking from zone #" + ZoneId);
//		if (currentZone.MazeVisitedCount == 1)
//		{
//			Console.Error.WriteLine("Marking zone #" + ZoneId + " as a dead end");
//			currentZone.MazeVisitedCount = 2;
//		}

//		var neighbours = gameState.Zones.Where(zone=> currentZone.Neighbours.Contains(zone.Id));
//		var neighboursVisitedOnce = neighbours.Where(z => z.MazeVisitedCount == 1);
//		if (neighboursVisitedOnce.Count() == 0)
//		{
//			Console.Error.WriteLine("All exists are marked as double visited. Idling!");
//#warning Convert this Squad to another type
//			return new string[0];
//		}
//		else if (neighboursVisitedOnce.Count() == 1)
//		{
//			var backtrackZone = neighboursVisitedOnce.Single().Id;
//			//It's obvious where we came from, so go back there.
//			Console.Error.WriteLine("Backtracking to zone #" + backtrackZone);
//			return new[] { this.MoveTo(backtrackZone) };
//		}
//		else
//		{
//			Console.Error.WriteLine(string.Format("Zone #{0} has multiple exits visited just once: {1}",
//				this.ZoneId,
//				string.Join(", ", neighboursVisitedOnce.Select(x => x.Id).ToArray())
//				));
//			return new[] { this.MoveTo(this.PreviousZoneId) };
//		}
	}

	public string MoveTo(int toZone)
	{
		this.PreviousZoneId = this.ZoneId;
		this.ZoneId = toZone;
		Console.Error.WriteLine(string.Format("{0} pods at zone #{1} moves to zone #{2}.", this.Pods, this.PreviousZoneId, this.ZoneId));
		return string.Format("{0} {1} {2}", this.Pods, this.PreviousZoneId, this.ZoneId);
	}


	private string[] moveToUnvisitedZones(GameState gameState, Zone currentZone, int[] unvisitedNeighbours)
	{
		var commands = new List<string>();

		var zonesToVisit = unvisitedNeighbours.Count();
		for (int i = 0; i < zonesToVisit - 1; i++)
		{
			if (Pods == 1)
			{
				//Console.Error.WriteLine("Only one pod left in the squad. Sending it to last zone.");
				break;
			}
			var podsInGroup = (int)Math.Ceiling(Pods / (double)(zonesToVisit - i));
			var squad = new MazeRunner { ZoneId = this.ZoneId, Pods = podsInGroup };
			gameState.Squads.Add(squad);
			this.Pods -= podsInGroup;

			commands.Add(squad.MoveTo(unvisitedNeighbours[i]));
			if (gameState.Zones[squad.ZoneId].MazeVisitedCount == 0)
				gameState.Zones[squad.ZoneId].MazeVisitedCount = 1;
		}

		//We keep ourselves around for the last neighbour
		commands.Add(this.MoveTo(unvisitedNeighbours.Last()));
		if (gameState.Zones[this.ZoneId].MazeVisitedCount == 0)
			gameState.Zones[this.ZoneId].MazeVisitedCount = 1;

		return commands.ToArray();
	}

	public void AfterMove(GameState gameState)
	{
	}
}


