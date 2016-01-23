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
class Player
{
	static Stopwatch Timer = new Stopwatch();
	static Dijkstra.Node[] nodes;

	static void Main(string[] args)
	{
		//test2();

		var map = new Map(30, 20);
		Point[] players = null;
		nodes = nodesFrom(map);

		// game loop
		var gameTurn = 0;
		var meanMilliseconds = 0.0;
		while (true)
		{
			var inputs = Console.ReadLine().Split(' ');
			var numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
			int myPlayerNumber = int.Parse(inputs[1]); // your player number (0 to 3).
			var positions = readPositionsFromConsole(numberOfPlayers);
			Timer.Restart();

			var heading = processGameTurn(map, ref players, myPlayerNumber, positions);

			Timer.Stop();
			gameTurn++;
			meanMilliseconds += (Timer.ElapsedMilliseconds - meanMilliseconds) / gameTurn;
			Debug("End of turn. Mean is {2} ms.", gameTurn, Timer.ElapsedMilliseconds, meanMilliseconds);

			Console.WriteLine(heading);
		}
	}

	public static Direction processGameTurn(Map map, ref Point[] players, int myPlayerNumber, Point[] positions)
	{
		var firstStep = players == null;
		if (firstStep)
		{
			players = positions;
		}
		else
		{
			updatePlayerPositions(map, players, positions);
		}

		putPlayersOn(map, players, nodes);
		updatePlayerPaths(map, nodes, players);
		//printMap(map);

		var heading = selectNextHeading(map, players, myPlayerNumber, firstStep);
		return heading;
	}

	private static void updatePlayerPaths(Map map, Dijkstra.Node[] nodes, Point[] players)
	{
		foreach (var player in players.Where(p => p.IsAlive))
		{
			//var playerMoves = validMoves(player, map, true).Select(x => map.IndexOf(x)).ToArray();
			//Debug("Valid moves from #{0} is: {1}", map.IndexOf(player), string.Join(", ", playerMoves));
			//foreach (var move in playerMoves)
			//{
			//	Debug("Valid moves from #{0} is: {1}", move, string.Join(", ", validMoves(Point.From(move, map.Width), map, true).Select(x => map.IndexOf(x)).ToArray()));
			//}

			player.Paths = new Dijkstra(nodes, map.IndexOf(player), map.IndexOf(player));
			//Debug("Path to self: {0}", player.Paths.Path(map.IndexOf(player)).Length);

			//var paths = players.Where(p => p != player).First().Paths;
			//var path = paths == null ? null : paths.Path(map.IndexOf(player));
			//Debug("Path to opponent: {0}", path == null ? "NULL" : path.Length.ToString());
		}
	}

	#region AI

	private static Direction selectNextHeading(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		var me = players[myPlayerNumber];
		var allOpponents = players.Except(new[] { me }).ToArray();
		var reachableOpponents = allOpponents
			.Select(p => new { Player = p, Tiles = validMoves(p, map, firstStep).Select(move => map.IndexOf(move)).ToArray() }) //Opponent proximity rachable w/o walking into the opponent
			.Where(x => x.Tiles.Any(index => me.Paths.Path[index] != null && !me.Paths.Path[index].Contains(map.IndexOf(x.Player))))
			.Select(x => x.Player)
			.ToArray();

		switch (reachableOpponents.Length)
		{
			case 0: return selectNextHeading_NoReachableOpponents(map, players, myPlayerNumber, firstStep);
			case 1: return selectNextHeading_OneOpponent(map, players, myPlayerNumber, firstStep);
			default: return selectNextHeading_MultipleOpponents(map, players, myPlayerNumber, firstStep);
		}
	}

	private static Direction selectNextHeading_NoReachableOpponents(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		Debug("Using strategy for no reachable opponents");
		printMap(map);
		var me = players[myPlayerNumber];
		var allValidMoves = validMoves(me, map, firstStep);
		//Console.Error.WriteLine("Valid moves: " + string.Join(", ", allValidMoves));
		var possibleMoves = allValidMoves
			;//.Where(m => !articulationPoints.Contains(map.IndexOf(m.X, m.Y)));
		//Console.Error.WriteLine("Moves-APs: " + string.Join(", ", possibleMoves));
		if (!possibleMoves.Any())
			possibleMoves = allValidMoves;


		var scoredMoves = possibleMoves
			.Select(move => new { Heading = move.Heading, Score = score(map, move) })
			.OrderByDescending(x => x.Score)
			.ToArray();

		//Console.Error.WriteLine("Scored moves: " + string.Join(", ", scoredMoves.Select(x => string.Format("{0}={1}", x.Heading, x.Score))));

		return scoredMoves
			.Select(p => p.Heading)
			.DefaultIfEmpty(Direction.RIGHT) //If default we are dead anyway and just need to write something to die happily
			.FirstOrDefault();
	}

	private static Direction selectNextHeading_OneOpponent(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		Debug("");
		Debug("{0}", string.Join(", ", players.Select(p => p.ToString())));
		Debug("Using strategy for one reachable opponent");


		//Debug("Determine controlled regions");
		//var ownedTiles = calculateControlledAreas(map, players, nodes);
		//printMap(ownedTiles.Select(x => x.HasValue ? char.Parse(x.Value.ToString()) : '#').ToArray(), map.Width);



		var me = players[myPlayerNumber];
		var opponent = players.Except(new[] { me }).Where(p => p.IsAlive).Single();

		Debug("Identify rooms and articulation points");
		map[map.IndexOf(me)] = null;//foreach (var player in players) map[map.IndexOf(player)] = null;
		var vertexes = vertexesFrom(map);
		map[map.IndexOf(me)] = me.Id;//foreach (var player in players) map[map.IndexOf(player)] = player.Id;
		Debug("Created vertexes from map");
		var tarjan = new HopcraftTarjan(vertexes, map.IndexOf(me));
		//printMap(map, tarjan.Components);
		//Debug("APs: {0}", string.Join(", ", tarjan.ArticulationPoints.Select(x => Point.From(x.Id, map.Width).ToString())));

		Debug("We found {0} rooms, sized: {1}", tarjan.Components.Count(), string.Join(", ", tarjan.Components.Select(x => x.Count()).ToArray()));
		//Debug("{0}", string.Join(", ", tarjan.Components.Select((x, index) => "C" + index + ": " + x.Count()).ToArray()));
		var dic = dictionaryFrom(tarjan.Components);

		var myRoom = dic[map.IndexOf(me)];
		var opponentRooms = validMoves(opponent, map, firstStep)
			.Select(move => dic.ContainsKey(map.IndexOf(move)) ? dic[map.IndexOf(move)] : -1)
			.Distinct();
		Debug("I'm in room {0}. Opponent is in room: {1}", myRoom, string.Join(", ", opponentRooms));
		if (opponentRooms.Contains(myRoom))
		{
			return selectNextHeading_SameRoomStrategy(map, players, myPlayerNumber, firstStep);
		}
		else
		{
			return selectNextHeading_DifferentRoomStrategy(map, players, myPlayerNumber, firstStep, vertexes, tarjan, dic);
		}
	}

	private static Direction selectNextHeading_DifferentRoomStrategy(Map map, Point[] players, int myPlayerNumber, bool firstStep, HopcraftTarjan.Vertex[] vertexes, HopcraftTarjan tarjan, Dictionary<int, int> dic)
	{
		Debug("Players are in different rooms");
		//Debug("{0}", string.Join(", ", players.Where(p => p.IsAlive).Select(p => "#" + p.Id + ": " + dic[map.IndexOf(p)]).ToArray()));
		//printMap(map, tarjan.Components);

		var me = players[myPlayerNumber];
		var myVertex = vertexes.Where(v => v.Id == map.IndexOf(me)).Single();
		if (myVertex.IsArticulationPoint)
		{
			Debug("TODO! Stay in the current room if we are in a bigger world than our opponent, otherwise move into his room");
			return selectNextHeading_NoReachableOpponents(map, players, myPlayerNumber, firstStep);//TODO: Only until this is really implemented
		}
		else
		{
			//Debug("Check if all opponents are in disjoint rooms from us");
			//var pathToOpponent = players
			//	.Where(p => p.IsAlive && p.Id != me.Id)
			//	.Select(p => me.Paths.Path[map.IndexOf(p)])
			//	.Where(path => path != null)
			//	.FirstOrDefault();
			//if (pathToOpponent == null)
			//	return selectNextHeading_NoReachableOpponents(map, players, myPlayerNumber, firstStep); //No opponent is reachable

			var allOpponents = players.Except(new[] { me }).ToArray();
			var opponentNeighbours = allOpponents
				.SelectMany(p => validMoves(p, map, firstStep).Select(move => new { Player = p, Neighbour = map.IndexOf(move) }).ToArray())
				.ToArray();
			var reachableNeighbours = opponentNeighbours
				.Where(x => me.Paths.Path[x.Neighbour] != null)
				.Select(x => new { x.Player, x.Neighbour, Path = me.Paths.Path[x.Neighbour] })
				.ToArray();
			var closestOpponent = reachableNeighbours
				.OrderBy(x => x.Path.Length)
				.FirstOrDefault();

			if (closestOpponent == null)
			{
				Debug("No reachable opponents");
				return selectNextHeading_NoReachableOpponents(map, players, myPlayerNumber, firstStep);
			}
			else
			{
				Debug("Move towards the other player until we reach an articulation point");
				var pathToOpponent = closestOpponent.Path;
				var bestMove = pathToOpponent.Skip(1).First();
				return Player.directionTo(map, me, bestMove);
			}
		}
	}

	private static Direction selectNextHeading_MultipleOpponents(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		//Debug("Using strategy for multiple opponents");
		//return selectNextHeading_NoReachableOpponents(map, players, myPlayerNumber, firstStep); //TODO: Just until this is really implemented
		return selectNextHeading_SameRoomStrategy(map, players, myPlayerNumber, firstStep);
	}

	private static Direction selectNextHeading_SameRoomStrategy(Map map, Point[] players, int myPlayerNumber, bool firstStep)
	{
		Debug("Players are in same room");
		Debug("Find Volornov line, go there and cut off the other player");

		var me = players[myPlayerNumber];

		var oldPaths = me.Paths;
		var bestMove = validMoves(me, map, firstStep)
			.Select(move =>
			{
				//var oldP = new Point(me.Id, me.X, me.Y, me.X0, me.Y0);
				me.Paths = new Dijkstra(nodes, map.IndexOf(move), map.IndexOf(me));
				//me.MoveTo(move);
				var ownedTiles = calculateControlledAreas(map, players, nodes);
				//me.ResetPositionTo(oldP);
				var score = ownedTiles.Where(x => x == me.Id).Count();
				Debug("Moving to {0} scores {1}", move, score);
				return new { Move = move, Score = score };
			}).OrderByDescending(x => x.Score)
			.First()
			.Move;
		me.Paths = oldPaths;
		return directionTo(me, bestMove);




		//var opponent = players.Where(p => p.X >= 0 && p.Id != myPlayerNumber).First();
		////Debug("Heading towards the opponent: {0} -> {1}", map.IndexOf(me), map.IndexOf(opponent));
		//var myMoves = validMoves(me, map, true);
		//var myMovesArray = myMoves.Select(move => map.IndexOf(move)).ToArray();
		//var myMovesString = string.Join(", ", myMovesArray);
		////Debug("Valid moves: {0}", myMovesString);
		//var bestPath = me.Paths.Path[map.IndexOf(opponent)];
		////Debug("Path: {0}", string.Join(", ", bestPath));
		//var destination = bestPath.Skip(1).First();

		//return directionTo(map, me, destination);
	}

	private static Direction directionTo(Map map, Point me, int destination)
	{
		var myMapIndex = map.IndexOf(me);
		if (myMapIndex - 1 == destination) return Direction.LEFT;
		if (myMapIndex - map.Width == destination) return Direction.UP;
		if (myMapIndex + 1 == destination) return Direction.RIGHT;
		if (myMapIndex + map.Width == destination) return Direction.DOWN;
		throw new NotSupportedException("Unknown direction to destination");
	}

	private static Direction directionTo(Point me, Point destination)
	{
		if (me.X > destination.X) return Direction.LEFT;
		if (me.Y > destination.Y) return Direction.UP;
		if (me.X < destination.X) return Direction.RIGHT;
		if (me.Y < destination.Y) return Direction.DOWN;
		throw new NotSupportedException("Unknown direction to destination");
	}

	private static int?[] calculateControlledAreas(Map map, Point[] players, Dijkstra.Node[] nodes)
	{
		var distanceMaps = players.Select(p => new
		{
			Player = p,
			DistanceMap = map.Array
				.Select((x, index) =>
				{
					if (x.HasValue)
						return null;
					if (!p.Paths.Path.ContainsKey(index))
						return null;
					var path = p.Paths.Path[index];
					if (path == null)
						return null;
					return (int?)p.Paths.Path[index].Length;
				})
				.ToArray()
		}).ToArray();

		//Debug("Calculated distances");
		var ownedTiles = map.Array.Select((x, index) =>
		{
			if (x.HasValue)
				return null; //Noone owns a wall

			var distancesToTile = distanceMaps
				.Select((d, playerIndex) => new
				{
					PlayerId = playerIndex,
					distance = d.DistanceMap[index]
				}).Where(d => d.distance != null)
				.OrderBy(d => d.distance);

			var closestPlayer = distancesToTile.FirstOrDefault();
			var secondPlayer = distancesToTile.Skip(1).FirstOrDefault();
			if (closestPlayer == null)
				return null; // No players reach this tile

			if (secondPlayer != null && closestPlayer.distance == secondPlayer.distance)
				return null;//Two or more players share this tile

			return (int?)closestPlayer.PlayerId;
		}).ToArray();

		//Debug("Created map of owned tiles");
		//var controlledAreaPerPlayer = players.Select((p, index) => new { p.Id, Count = ownedTiles.Where(x => x == index).Count() }).OrderBy(x => x.Id).Select(x => x.Count).ToArray();

		//Debug("Calculated player areas");

		//for (var i = 0; i < controlledAreaPerPlayer.Length;i++ )
		//{
		//	Debug("Player #{0} controls {1} tiles.", i, controlledAreaPerPlayer[i]);
		//}

		return ownedTiles;
	}

	public static void Debug(string format, params object[] args)
	{
		Console.Error.WriteLine(Timer.ElapsedMilliseconds + " ms: " + string.Format(format, args));
	}

	private static int score(Map map, Point p)
	{
		return wallsAt(map, p);
	}

	#endregion AI

	#region Business Logic

	private static void updatePlayerPositions(Map map, Point[] players, Point[] positions)
	{
		bool recreateNodesArray = false;
		for (var i = 0; i < positions.Length; i++)
		{
			if (players[i].IsAlive && positions[i].X == -1)
			{
				Debug("Player #{0} died and is removed from the map", i);
				map.RemoveAll(i);
				recreateNodesArray = true;
			}
			players[i].MoveTo(positions[i]);
		}
		if (recreateNodesArray)
		{
			Debug("Recreating nodes array because at least one player has been removed from the game");
			nodes = nodesFrom(map);
		}
	}

	private static void putPlayersOn(Map map, Point[] players, Dijkstra.Node[] nodes)
	{
		foreach (var player in players.Where(p => p.IsAlive))
		{
			//Debug("Marking player #{0} on map", player.Id);
			map.Put(player.X, player.Y, player.Id);
			var playersNode = nodes[map.IndexOf(player)];
			foreach (var neighbourNode in playersNode.Neighbours)
			{
				neighbourNode.Neighbours = neighbourNode.Neighbours.Except(new[] { playersNode }).ToArray();
			}
		}
	}

	private static void putPlayerTailsOn(Map map, Point[] players)
	{
		foreach (var player in players.Where(p => p.IsAlive))
		{
			//Console.Error.WriteLine("Marking tail on map for {0}: ({1}, {2})" , player.Id, player.X0, player.Y0);
			map.Put(player.X0, player.Y0, player.Id);
		}
	}

	public static IEnumerable<Point> validMoves(Point me, Map map, bool firstStep)
	{
		Point p;

		p = me.NextPosition(me.Heading);
		if (map.IsFree(p))
			yield return p;

		p = me.NextPosition(turn(me.Heading, 1));
		if (map.IsFree(p))
			yield return p;

		if (firstStep)
		{
			p = me.NextPosition(turn(me.Heading, 2));
			if (map.IsFree(p))
				yield return p;
		}

		p = me.NextPosition(turn(me.Heading, 3));
		if (map.IsFree(p))
			yield return p;
	}

	#endregion

	#region Print Map methods

	public static void printMap(Map map, IEnumerable<HopcraftTarjan.Vertex[]> components)
	{
		var dic = dictionaryFrom(components);
		var charArray = map.Array.Select((c, index) => c.HasValue ? ((char)('a' + c.Value)) : dic.ContainsKey(index)? char.Parse(dic[index].ToString()) : '?').ToArray();
		Player.printMap(charArray, map.Width);
	}

	private static Dictionary<int, int> dictionaryFrom(IEnumerable<HopcraftTarjan.Vertex[]> components)
	{
		var dic = components.SelectMany((array, index) => array.Select(v => new { v.Id, index })).ToDictionary(x => x.Id, x => x.index);
		return dic;
	}

	public static void printMap(Map map)
	{
		printMap(map.Array.Select(value => value.HasValue ? (char)('@' + value.Value) : '.').ToArray(), map.Width);
	}

	public static void printMap(char[] map, int width)
	{
		var height = map.Length / width;
		//Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		for (var y = 0; y < height; y++)
		{
			//Console.Error.Write('¤');
			for (var x = 0; x < width; x++)
			{
				var token = map[y * width + x];
				Console.Error.Write(token);
			}
			Console.Error.WriteLine("");//¤
		}
		//Console.Error.WriteLine(string.Join("", Enumerable.Repeat('¤', width + 2)));
		Console.Error.WriteLine();
	}

	#endregion Print Map methods

	#region Helpers

	private static Direction turn(Direction direction, int clockwiseTurns)
	{
		return (Direction)(((int)direction + clockwiseTurns) % 4);
	}

	private static int wallsAt(Map map, Point p)
	{
		var walls = 0;
		if (p.X == 0 || map[p.NextPosition(Direction.LEFT)] != null) walls++;
		if (p.X == map.Width - 1 || map[p.NextPosition(Direction.RIGHT)] != null) walls++;
		if (p.Y == 0 || map[p.NextPosition(Direction.UP)] != null) walls++;
		if (p.Y == map.Height - 1 || map[p.NextPosition(Direction.DOWN)] != null) walls++;
		return walls;
	}

	private static HopcraftTarjan.Vertex[] vertexesFrom(Map map)
	{
		var vertices = map.Array
			.Select((cell, index) => new { Id = index, IsWall = cell.HasValue })
			.Where(x => !x.IsWall)
			.Select(x => new HopcraftTarjan.Vertex(x.Id))
			.ToArray();
		foreach (var v in vertices)
		{
			v.Dependents = validMoves(Point.From(v.Id, map.Width), map, true)
				.Select(move => vertices.Where(x => x.Id == map.IndexOf(move)).Single())
				.ToArray();
		}
		return vertices;
	}

	private static Dijkstra.Node[] nodesFrom(Map map)
	{
		var nodes = map.Array.Select((x, index) => new Dijkstra.Node { Id = index }).ToArray();
		foreach (var node in nodes)
		{
			var moves = validMoves(Point.From(node.Id, map.Width), map, true).ToArray();
			node.Neighbours = moves
				.Select(move => nodes[map.IndexOf(move)])
				.ToArray();
		}
		return nodes;
	}

	private static Point[] readPositionsFromConsole(int N)
	{
		var playersTurn = Enumerable
						 .Range(0, N)
						 .Select(i =>
						 {
							 var inputs = Console.ReadLine().Split(' ');
							 int X0 = int.Parse(inputs[0]); // starting X coordinate of lightcycle (or -1)
							 int Y0 = int.Parse(inputs[1]); // starting Y coordinate of lightcycle (or -1)
							 int X1 = int.Parse(inputs[2]); // starting X coordinate of lightcycle (can be the same as X0 if you play before this player)
							 int Y1 = int.Parse(inputs[3]); // starting Y coordinate of lightcycle (can be the same as Y0 if you play before this player)
							 var p = new Point(i, X1, Y1, X0, Y0);
							 Debug("{0}", p);
							 return p;
						 }).ToArray();
		return playersTurn;
	}

	#endregion helpers

	#region Testing

	private static void test()
	{
//		var mapString = @"
//...........###################
//...........#.................#
//.............................#
//.............................#
//.............................#
//.............................#
//............................A#
//............................@#
//.............................#
//.............................#
//.............................#
//.............................#
//.............................#
//.........#####################
//..............................
//..............................
//..............................
//..............................
//..............................
//..............................";
//		var mapString = @"
//..#....
//..#.#..
//....#..
//.......
//";
//		var width = mapString.IndexOf('\r', 1) - 2;
//		var mapString2 = mapString.Replace("\r\n", "");
//		var map = new Map(mapString2.Select(c => c == '.' ? null : (int?)c).ToArray(), width);
//		//printMap(map);
//		var players = new[]{ 
//			Point.From(mapString2.IndexOf('@'), width, 0),
//			Point.From(mapString2.IndexOf('A'), width, 1)
//		};
//		var player = new Point(0, 3, 1, 2, 1);
//		var turn = new[] { 4, 0 };
//		nodes = nodesFrom(map);



		var map = new Map(7, 4);
		Point[] players = null;
		nodes = nodesFrom(map);
		var player = new Point(0, 2, 0, 2, 0);
		foreach (var turn in new[] { 
			new[] { 4, 2 }, 
			new[] { 4, 1 }, 
			new[] { 4, 0 }, 
			//new[] { 25, 0 }, 
			//new[] { 26, 0 }, 
			//new[] { 15, 0 }, 
			//new[] { 16, 0 }, 
			//new[] { 17, 0 }, 
			//new[] { 18, 0 }, 
			//new[] { 19, 0 }, 
			//new[] { 20, 0 }, 
			//new[] { 21, 0 }, 
			//new[] { 22, 0 }, 
			//new[] { 23, 0 }, 
			//new[] { 24, 0 }, 
			//new[] { 25, 0 }, 
			//new[] { 26, 0 }, 
			//new[] { 27, 0 }, 
			//new[] { 28, 0 }, 
			//new[] { 29, 0 }, 
			//new[] { 29, 1 }, 
			//new[] { 29, 2 }, 
			//new[] { 29, 3 }, 
			//new[] { 29, 4 }, 
			//new[] { 29, 5 }, 
			//new[] { 29, 6 }, 
			//new[] { 28, 6 }, 
		})

		//var map = new Map(30,20);
		//Point[] players = null;
		//nodes = nodesFrom(map);
		//var player = new Point(0, 22, 0, 22, 0);
		//foreach (var turn in new[] { 
		//	new[] { 24, 2 }, 
		//	new[] { 24, 1 }, 
		//	new[] { 24, 0 }, 
		//	new[] { 25, 0 }, 
		//	//new[] { 26, 0 }, 
		//	//new[] { 15, 0 }, 
		//	//new[] { 16, 0 }, 
		//	//new[] { 17, 0 }, 
		//	//new[] { 18, 0 }, 
		//	//new[] { 19, 0 }, 
		//	//new[] { 20, 0 }, 
		//	//new[] { 21, 0 }, 
		//	//new[] { 22, 0 }, 
		//	//new[] { 23, 0 }, 
		//	//new[] { 24, 0 }, 
		//	//new[] { 25, 0 }, 
		//	//new[] { 26, 0 }, 
		//	//new[] { 27, 0 }, 
		//	//new[] { 28, 0 }, 
		//	//new[] { 29, 0 }, 
		//	//new[] { 29, 1 }, 
		//	//new[] { 29, 2 }, 
		//	//new[] { 29, 3 }, 
		//	//new[] { 29, 4 }, 
		//	//new[] { 29, 5 }, 
		//	//new[] { 29, 6 }, 
		//	//new[] { 28, 6 }, 
		//})
		{
			var direction = processGameTurn(map, ref players, 0, new[] { player, new Point(1, turn[0], turn[1], 11, 1) });
			Debug("{0} moves to {1}", player, direction);
			player = player.NextPosition(direction);
		}

		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 9, 13, 9, 13), new Point(1, 11, 1, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 10, 13, 9, 13), new Point(1, 11, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 11, 13, 9, 13), new Point(1, 12, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 12, 13, 9, 13), new Point(1, 13, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 13, 13, 9, 13), new Point(1, 14, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 14, 13, 9, 13), new Point(1, 15, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 15, 13, 9, 13), new Point(1, 16, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 16, 13, 9, 13), new Point(1, 17, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 17, 13, 9, 13), new Point(1, 18, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 18, 13, 9, 13), new Point(1, 19, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 19, 13, 9, 13), new Point(1, 20, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 20, 13, 9, 13), new Point(1, 21, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 21, 13, 9, 13), new Point(1, 22, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 22, 13, 9, 13), new Point(1, 23, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 23, 13, 9, 13), new Point(1, 24, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 24, 13, 9, 13), new Point(1, 25, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 25, 13, 9, 13), new Point(1, 26, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 26, 13, 9, 13), new Point(1, 27, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 27, 13, 9, 13), new Point(1, 28, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 28, 13, 9, 13), new Point(1, 29, 0, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 13, 9, 13), new Point(1, 29, 1, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 12, 9, 13), new Point(1, 29, 2, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 11, 9, 13), new Point(1, 29, 3, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 10, 9, 13), new Point(1, 29, 4, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 9, 9, 13), new Point(1, 29, 5, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 8, 9, 13), new Point(1, 29, 6, 11, 1) }));
		//Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 29, 7, 9, 13), new Point(1, 28, 6, 11, 1) }));

		printMap(map);
		foreach(var p in players) Debug("{0}", p);
		Debug("{0}", processGameTurn(map, ref players, 0, new[] { new Point(0, 28, 7, 9, 13), new Point(1, 28, 5, 11, 1) }));
		printMap(map);
		foreach (var p in players) Debug("{0}", p);





		//players[1].MoveTo(Point.From(map.IndexOf(players[1]) - map.Width, map.Width, 1));


		////var me = 1;

		//var nodes = nodesFrom(map);
		//updatePlayerPaths(map, nodes, players);

		//var ownedTiles = calculateControlledAreas(map, players, nodes);
		//printMap(ownedTiles.Select(x => x.HasValue ? char.Parse(x.Value.ToString()) : '#').ToArray(), map.Width);

		////var distances = players.Select(p =>
		////{
		////	var d = new Dijkstra(nodes, map.IndexOf(p));
		////	return new { Player = p, DistanceMap = map.Array.Select((x, index) => x.HasValue ? null : (int?)d.Path(index).Length).ToArray() };
		////}).ToArray();
		//var direction = selectNextHeading(map, players, 0, false);

		//foreach (var player in players) map[map.IndexOf(player)] = null;
		//var tarjan = new HopcraftTarjan(vertexesFrom(map));
		//foreach (var player in players) map[map.IndexOf(player)] = player.Id;

		//Debug("Found articulation points:");
		//foreach (var ap in tarjan.ArticulationPoints)
		//{
		//	Debug("{0}", ap);

		//}
		//printMap(map, tarjan.Components);

		//var dic = dictionaryFrom(tarjan.Components);
		//var room0 = dic[map.IndexOf(players[0])];
		//var room1 = dic[map.IndexOf(players[1])];
		//var isInSameRoom = room0 == room1;
		//Debug("Player 1 is in room {0} and player 2 is in room {1}. Same={2}", room0, room1, isInSameRoom);


		//var heading = selectNextHeading(map, players, 1, false);
		//Debug("Heading: {0}", heading);
	}



	private static void test2()
	{
		var map = new Map(30, 20);
		Point[] players = null;
		var positions = new[]{
			new Point(0,10,10,10,10),
			new Point(1,5,5,5,5)
		};
		Player.nodes = nodesFrom(map);

		while (positions.Where(x => x.IsAlive).Count() >= 2)
		{
			for (var i = 0; i < positions.Length; i++)
			{
				if (positions[i].IsAlive)
				{
					var direction = processGameTurn(map, ref players, i, positions);
					Debug("Player #{0} moves to {1}", i, direction);
					positions[i] = positions[i].NextPosition(direction);
					if (map[positions[i]].HasValue)
					{
						Debug("Player #{0} moved into a wall at {1} and died.", i, positions[i]);
						positions[i] = new Point(0, -1, -1, -1, -1);
					}
				}
			}
		}
		Debug("WINNER is player #{0}", players.Where(x => x.IsAlive).Single().Id);
	}

	#endregion Testing
}

#region Classes and Enums

public class Map
{
	public const int TILE_IS_WALL = 29;

	readonly int?[] _array;
	public IEnumerable<int?> Array { get { return _array; } }
	public int Length { get { return _array.Length; } }

	public int Width { get; private set; }
	public int Height { get; private set; }
	
	public Map(int width, int height)
	{
		Width = width;
		Height = height;
		_array = new int?[width * height];
	}

	public Map(int?[] array, int width)
	{
		Width = width;
		Height = array.Length / width;
		_array = array;
	}

	public int? this[int index] 
	{ 
		get {
			if (index < 0 || index > this.Length)
				return TILE_IS_WALL;
			return _array[index]; 
		} 
		set {
			if (index < 0 || index > this.Length)
				return;
			_array[index] = value; 
		} 
	}

	public int? this[Point p]
	{
		get { return this[IndexOf(p.X, p.Y)]; }
		set { this[IndexOf(p.X, p.Y)] = value; }
	}

	public int? Get(int x, int y)
	{
		if (x < 0 || x >= Width)
			return TILE_IS_WALL;
		if (y < 0 || y >= Height)
			return TILE_IS_WALL;
		return _array[IndexOf(x, y)];
	}

	public void Put(int x, int y, int token)
	{
		this[IndexOf(x, y)] = token;
	}

	public int IndexOf(int x , int y) {
		if (x < 0 || x >= this.Width) return -1;
		if (y < 0 || y >= this.Height) return -1;
		return y * this.Width + x; 
	}
	public int IndexOf(Point p) { return IndexOf(p.X, p.Y); }

	public bool IsFree(Point p) { return !Get(p.X, p.Y).HasValue; }

	internal void RemoveAll(int token)
	{
		for (var i = 0; i < this.Length; i++)
		{
			if (_array[i].HasValue && _array[i].Value == token)
				_array[i] = null;
		}
	}
}

public class Point
{
	public int Id { get; private set; }
	public int X { get; set; }
	public int Y { get; set; }
	public int X0 { get; private set; }
	public int Y0 { get; private set; }
	public bool IsAlive { get { return X >= 0; } }

	public Dijkstra Paths { get; set; }

	public Point(int id, int x, int y, int x0, int y0)
	{
		Id = id;
		X = x;
		Y = y;
		X0 = x0;
		Y0 = y0;
	}

	public Direction Heading
	{
		get
		{
			if (X > X0) return Direction.RIGHT;
			if (X < X0) return Direction.LEFT;
			if (Y > Y0) return Direction.DOWN;
			if (Y < Y0) return Direction.UP;
			return Direction.RIGHT;//Only at startup. This defaults to starting to the right...
		}
	}

	public Point NextPosition(Direction d)
	{
		switch (d)
		{
			case Direction.LEFT: return new Point(Id, X - 1, Y, X, Y);
			case Direction.UP: return new Point(Id, X, Y - 1, X, Y);
			case Direction.RIGHT: return new Point(Id, X + 1, Y, X, Y);
			case Direction.DOWN: return new Point(Id, X, Y + 1, X, Y);
			default:
				throw new NotSupportedException();
		}
	}

	public void MoveTo(Point p)
	{
		if (p.X != X || p.Y != Y)
		{
			X0 = X;
			Y0 = Y;
			X = p.X;
			Y = p.Y;
		}
	}

	public void ResetPositionTo(Point p)
	{
		X = p.X;
		Y = p.Y;
		X0 = p.X0;
		Y0 = p.Y0;
	}

	//public int MapIndex { get { return this.X + this.Y * Player.MAP_WIDTH; } }

	public override string ToString()
	{
		return string.Format("#{0} ({1}, {2})", this.Id, this.X, this.Y);
	}

	internal static Point From(int idx, int width, int id = 0)
	{
		return new Point(id, idx % width, idx / width, 0, 0);
	}
}

public enum Direction
{
	LEFT = 0,
	UP = 1,
	RIGHT = 2,
	DOWN = 3
}

#region Hopcraft/Tarjan algorithm for finding biconnected components and articulation points

public class HopcraftTarjan
{
	List<Vertex> _component;

	public IEnumerable<Vertex[]> Components { get { return _components.AsEnumerable(); } }
	List<Vertex[]> _components;

	public IEnumerable<Vertex> ArticulationPoints { get; private set; }

	public HopcraftTarjan(IEnumerable<Vertex> vertexes, int startId)
	{
		_component = new List<Vertex>();
		_components = new List<Vertex[]>();

		var startVertex = vertexes.Where(v => v.Id == startId).Single();
	
		Traverse(startVertex, 0);

		if (_component != null && _component.Count > 0)
		{
			_components.Add(_component.ToArray());
			_component = null;
		}

		ArticulationPoints = vertexes.Where(v => v.IsArticulationPoint).ToArray();
	}

	private void Traverse(IEnumerable<Vertex> vertexes)
	{
		foreach (var v in vertexes)
		{
			if (!v.Depth.HasValue)
			{
				Traverse(v, 0);
			}
		}

		_components.Add(_component.ToArray());
		_component = null;
	}

	private void Traverse(Vertex vertex, int depth)
	{
		vertex.Depth = depth;
		vertex.Low = depth;
		var childCount = 0;
		//Player.Debug("#{0} at depth {1}", vertex.Id, depth);
		foreach (var nextVertex in vertex.Dependents)
		{
			if (!nextVertex.Depth.HasValue)
			{
				nextVertex.Parent = vertex;
				Traverse(nextVertex, depth + 1);
				childCount++;
				if (vertex.Parent != null && nextVertex.Low >= vertex.Depth) vertex.IsArticulationPoint = true;
				if (vertex.Low.Value > nextVertex.Low.Value) vertex.Low = nextVertex.Low.Value;
			}
			else if (nextVertex != vertex.Parent)
			{
				if (vertex.Low.Value > nextVertex.Depth.Value) vertex.Low = nextVertex.Depth.Value;
			}

			if (vertex.Parent == null && _component.Count > 0)
			{
				_components.Add(_component.ToArray());
				_component = new List<Vertex>();
			}
		}

		if (vertex.Parent == null && childCount > 1) vertex.IsArticulationPoint = true;
		_component.Add(vertex);
		if (vertex.IsArticulationPoint)
		{
			// Form a component of tracked vertexes
			if (vertex.Low == vertex.Depth)
			{
				_components.Add(_component.ToArray());
				_component = new List<Vertex>();
			}
		}
		//Player.Debug("#{0} at depth {1}: LOW={2} {3}", vertex.Id, depth, vertex.Low, vertex.IsArticulationPoint ? "AP!" : "");
	}

	public class Vertex
	{
		public int Id { get; set; }
		public int? Depth { get; set; }
		public int? Low { get; set; }
		public Vertex Parent { get; set; }
		public bool IsArticulationPoint { get; set; }
		public Vertex[] Dependents { get; set; }

		public Vertex(int id) { Id = id; }
		public override string ToString()
		{
			return string.Format("#{0}: Low: {1}, Disc: {2}, Parent: {3}", Id, Low, Depth, Parent == null ? "" : Parent.Id.ToString());
		}
	}
}

#endregion Hopcraft/Tarjan algorithm for finding biconnected components and articulation points

#region Dijkstras algorithm for finding shortest path

public class Dijkstra
{
	public int PlayerPosition { get; set; }
	public int From { get; private set; }
	public IDictionary<int, int[]> Path { get; private set; }

	Queue<Node> unvisitedNodes = new Queue<Node>();

	public Dijkstra(Node[] nodes, int from, int playerPosition)
	{
		var nodeDictionary = nodes.ToDictionary(x => x.Id);
		Reset(nodeDictionary, from, playerPosition);

		Path = new Dictionary<int, int[]>();
		foreach (var to in nodeDictionary.Keys)
			Path.Add(to, PathTo(nodeDictionary, to));
	}

	private void Reset(IDictionary<int, Node> nodes, int from, int playerPosition)
	{
		foreach (var node in nodes.Values)
		{
			node.Path = null;
			//node.Distance = int.MaxValue;
			node.Visited = false;
		}
		unvisitedNodes.Clear();

		this.From = from;
		this.PlayerPosition = playerPosition;

		nodes[from].Path = new int[] { from };
		unvisitedNodes.Enqueue(nodes[from]);
	}

	private int[] PathTo(IDictionary<int,Node> nodes, int to)
	{
		if (!nodes.ContainsKey(to))
			return null;//No paths to the destination at all

		if (nodes[to].Path != null)
			return nodes[to].Path;

		while (unvisitedNodes.Any())
		{
			var currentNode = unvisitedNodes.Dequeue();
			currentNode.Visited = true;
			if (currentNode.Path == null)
			{
				Player.Debug("Node {0} has NO Path!", currentNode.Id);
				continue;
			}
			var tentativeDistance = currentNode.Path.Length + 1;

			foreach (var neighbour in currentNode.Neighbours.Where(node => !node.Visited && node.Id != PlayerPosition))
			{
				bool isUnvisited = neighbour.Path == null;
				if (isUnvisited || neighbour.Path.Length > tentativeDistance)
					neighbour.Path = currentNode.Path.Concat(new[] { neighbour.Id }).ToArray();
				if (isUnvisited)
					unvisitedNodes.Enqueue(neighbour);
			}
			if (currentNode.Id == to)
				break;
		}

		return nodes[to].Path;
	}


	public class Node
	{
		public int Id { get; set; }
		public Node[] Neighbours { get; set; }
		public bool Visited { get; set; }
		public int[] Path { get; set; }
	}
}

#endregion Dijkstras algorithm for finding shortest path

#endregion Classes and Enums
