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
#warning No support for multiple squads at same location at this time
					Console.Error.WriteLine("Ignoring multiple squads at same location and hoping for best.");
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
			return new MazeRunner(pods, zoneId);
		}
		else
		{
			return new DeadDuck(pods, zoneId);
			//return new Torpedo(pods, zoneId, gameState.TheirBase, gameState.Zones);
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
		//Log("{0} pods at zone #{1} moves to zone #{2}.", this.Pods, this.ZoneId, toZone);
		var command = string.Format("{0} {1} {2}", this.Pods, this.ZoneId, toZone);
		this.ZoneId = toZone;
		return command;
	}

	protected void Log(string format, params object[] args)
	{
		Console.Error.WriteLine(string.Format(format, args));
	}
}

#endregion Main objects

#region Helpers

public class Dijkstra
{
	class Node
	{
		public int Id { get; set; }
		public int[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public int[] Path { get; set; }
	}

	Node[] _nodes;
	public int From { get; private set; }
	Queue<Node> unvisitedNodes = new Queue<Node>();

	public Dijkstra(Zone[] zones, int from)
	{
		_nodes = zones.Select(x => new Node { Id = x.Id, Neighbours = x.Neighbours.ToArray() }).ToArray();
		Reset(from);
	}

	public void Reset(int from)
	{
		foreach (var node in _nodes)
		{
			node.Path = null;
			//node.Distance = int.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		_nodes[from].Path = new int[] { from };
		unvisitedNodes.Enqueue(_nodes[from]);
	}

	public int[] Path(int to)
	{
		if (to >= _nodes.Length)
			return null;//No paths to the destination at all

		if (_nodes[to].Path != null)
			return _nodes[to].Path;

		while (unvisitedNodes.Any())
		{
			var currentNode = unvisitedNodes.Dequeue();
			currentNode.Visited = true;
			var tentativeDistance = currentNode.Path.Length + 1;

			foreach (var neighbour in currentNode.Neighbours.Select(id => _nodes[id]).Where(node => !node.Visited))
			{
				if (neighbour.Path == null || neighbour.Path.Length > tentativeDistance)
					neighbour.Path = currentNode.Path.Concat(new[] { neighbour.Id }).ToArray();
				unvisitedNodes.Enqueue(neighbour);
			}
			if (currentNode.Id == to)
				break;
		}

		return _nodes[to].Path;
	}
}

#endregion Helpers

#region Squads

public class DeadDuck : BaseSquad
{
	public DeadDuck(int pods, int zoneId) 
		: base(pods, zoneId)
	{
	}

	public override IEnumerable<string> Move(GameState gameState)
	{
		Log("DeadDuck at #{0} is not moving.", ZoneId);
		return new string[0];
	}
}

public class Torpedo : BaseSquad
{
	readonly Queue<int> _queue;

	public Torpedo(int pods, int zoneId, int targetZoneId, Zone[] map)
		: base(pods, zoneId)
	{
		Log("Creating torpedo at #{0} heading to #", zoneId, targetZoneId);
		var bfs = new Dijkstra(map, zoneId);
		var path = bfs.Path(targetZoneId).Skip(1);
		//Log("Queueing waypoints: {0}", string.Join(", ", path));
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
}

//public class RandomRunner : BaseSquad
//{
//	Random r;

//	public RandomRunner(int pods, int zoneId) 
//		: base(pods, zoneId)
//	{
//		r = new Random();
//	}

//	public override IEnumerable<string> Move(GameState gameState)
//	{
//		var neighbours = gameState.Zones[this.ZoneId].Neighbours;
//		var index = r.Next(neighbours.Count);
//		var nextZoneId = neighbours[index];
//		//Console.Error.WriteLine(string.Format("Randomly moving {0} pods from {1} to {2}", this.Pods, this.ZoneId, nextZoneId));
//		//return new[] { string.Format("{0} {1} {2}", this.Pods, this.ZoneId, nextZoneId) };
//		return new[] { MoveTo(nextZoneId) };
//	}
//}

public class MazeRunner : BaseSquad
{
	public int PreviousZoneId { get; set; }

	public MazeRunner(int pods, int zoneId)
		: base(pods, zoneId)
	{
	}

	public override void BeforeMove(GameState gameState)
	{
		var currentZone = gameState.Zones[ZoneId];

		if (currentZone.MazeVisitedCount == 0)
			currentZone.MazeVisitedCount = 1;
	}

	public override IEnumerable<string> Move(GameState gameState)
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
			return transformSquad(gameState, currentZone);
		}
	}

	private string[] transformSquad(GameState gameState, Zone currentZone)
	{
		Log("Squad at #{0} is transformed to a new EdgeFinder", this.ZoneId);
		var squad = new EdgeFinder(this.Pods, this.ZoneId);
		gameState.Squads.Remove(this);
		gameState.Squads.Add(squad);
		squad.BeforeMove(gameState);
		var result = squad.Move(gameState).ToArray();
		return result;
		//TODO: When to call AfterMove()?
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
			var squad = new MazeRunner(podsInGroup, this.ZoneId);
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
}

public class EdgeFinder : BaseSquad
{
	Zone TargetZone;
	Queue<int> _path;

	public EdgeFinder(int pods, int zoneId)
		: base(pods, zoneId)
	{
	}

	public override void BeforeMove(GameState gameState)
	{
		base.BeforeMove(gameState);

		if (TargetZone != null && !isValidTarget(gameState, TargetZone))
			TargetZone = null;

		if (TargetZone == null)
		{
			Log("EdgeFinder at #{0} needs to determine where to go.", ZoneId);
			var possibleTargets = gameState.Zones
				.Where(zone => isValidTarget(gameState, zone))
				.ToArray();

			var dfs = new Dijkstra(gameState.Zones, this.ZoneId);
			var target = possibleTargets
				.Select(zone => new { Zone = zone, Path = dfs.Path(zone.Id) })
				.Where(x => x.Path != null)
				.OrderBy(x => x.Path.Length)
				.FirstOrDefault();

			if (target == null)
			{
#warning What if we can't find a valid target?
				Log("EdgeFinder at zone #{0} could not find a valid target. Idling.", this.ZoneId);
			}
			else
			{
				TargetZone = target.Zone;
				_path = new Queue<int>(target.Path.Skip(1));
				Log("EdgeFinder at zone #{0} is heading for zone #{1} through path {2}", this.ZoneId, this.TargetZone.Id, string.Join(", ", _path.ToArray()));
			}
		}
	}

	public override IEnumerable<string> Move(GameState gameState)
	{
		if (_path != null && _path.Any())
		{
			var nextId = _path.Dequeue();
			//TODO: Split into multiple in case optimal
			return new[] { MoveTo(nextId) };
		}
		else
		{
#warning Convert EdgeFinder to something else here
			Log("EdgeFinder at #{0} has nowhere to go.", ZoneId);
			return new string[0];
			//return base.Move(gameState);
		}
	}

	public override void AfterMove(GameState gameState)
	{
		base.AfterMove(gameState);
		if (_path != null && !_path.Any())
		{
			Log("EdgeFinder at #{0} has no more moves.", ZoneId);
			if (isValidTarget(gameState, gameState.Zones[ZoneId]))
			{
				Log("EdgeFinder at #{0} has reached its target. Converting it to a MazeRunner.", ZoneId);
				var squad = new MazeRunner(this.Pods, this.ZoneId);
				gameState.Squads.Remove(this);
				gameState.Squads.Add(squad);
			}
			else
			{
				Log("... but EdgeFinder at #{0} is not at a valid target.", ZoneId);
				//TargetZone = null;
				//_path = null;
			}
		}
	}


	private bool isValidTarget(GameState gameState, Zone zone)
	{
		if (zone.MazeVisitedCount != 1)
			return false;
		if (gameState.Squads.OfType<MazeRunner>().Any(squad => squad.ZoneId == zone.Id))
			return false;
		var unvisitedNeighbours = zone.Neighbours.Where(id => gameState.Zones[id].MazeVisitedCount == 0);

		var otherEdgeFindersHeadingForZone = gameState.Squads
			.OfType<EdgeFinder>()
			.Except(new[] { this })
			.Where(squad => squad.TargetZone == zone);
		if (unvisitedNeighbours.Count() <= otherEdgeFindersHeadingForZone.Count())
			return false;

		return true;
	}
}

#endregion Squads
