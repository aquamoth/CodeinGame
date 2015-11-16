﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	//            ****
	//        ****   *
	//    ****       * Y
	//****   )a      *
	//****************
	//       X
	// a = atan(Y/X)
	public int Angle 
	{ 
		get
		{
			return (int)Math.Round(Math.Atan((double)Y / X) * 180 / Math.PI);
		}
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
	const double GRAVITY = 3.711;
	const int CRITICAL_VERTICAL_SPEED = -39;
	const int CRITICAL_HORIZONTAL_SPEED = 20;
	const int MAX_ANGLE_KEEP_VSPEED = 21;//21.91; // acos(GRAVITY / MAX_THROTTLE)

	static void Main(string[] args)
	{
		string[] inputs;
		int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

		var topology = new List<Point>();
		for (int i = 0; i < surfaceN; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
			int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
			topology.Add(new Point(landX, landY));
		}

		var lz = findLandingZone(topology);
		Console.Error.WriteLine(string.Format("LZ at {0} - {1}", lz.Item1, lz.Item2));


		// game loop
		while (true)
		{
			inputs = Console.ReadLine().Split(' ');
			int X = int.Parse(inputs[0]);
			int Y = int.Parse(inputs[1]);
			int hSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
			int vSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
			int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
			int rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
			int power = int.Parse(inputs[6]); // the thrust power (0 to 4).

			var position = new Point(X, Y);
			var speed = new Point(hSpeed, vSpeed);

			Tuple<int, int> command;

			var expectedLandingPoint = plotTrajectory(position, speed, lz.Item1.Y);
			Console.Error.WriteLine("If we initiate landing now, we end up at " + expectedLandingPoint);
			if (expectedLandingPoint.X < lz.Item1.X)
			{
				Console.Error.WriteLine("Need to go more RIGHT");
				var waypoint = findWaypointToRight(position, speed, lz.Item1);
				var foundObstacleInTrajectory = false;//TODO:
				if (foundObstacleInTrajectory)
				{
					throw new NotImplementedException("Plot course to the right that avoids the obstactle");
				}
				else
				{
					command = buildCommand_Waypoint(position, speed, waypoint);
				}
			}
			else if (expectedLandingPoint.X > lz.Item2.X)
			{
				throw new NotImplementedException("Need to go more left");
			}
			else
			{
				if (speed.X>CRITICAL_HORIZONTAL_SPEED)
				{
					Console.Error.WriteLine("Reduce speed to right in a safe manner, avoiding obstacles");
					command = new Tuple<int, int>(15, 4);
				}
				else if (speed.X < -CRITICAL_HORIZONTAL_SPEED)
				{
					Console.Error.WriteLine("Reduce speed to left in a safe manner, avoiding obstacles");
					command = new Tuple<int, int>(15, 4);
				}
				else
				{
					var foundObstacleInTrajectory = false;//TODO:
					if (foundObstacleInTrajectory)
					{
						throw new NotImplementedException("Plot course to start of landing that avoids the obstactle");
					}
					else
					{
						command = buildCommand_LAND(position, speed, lz.Item1.Y);
					}
				}
			}

			Console.WriteLine(string.Format("{0} {1}", command.Item1, command.Item2));
		}
	}

	private static Point findWaypointToRight(Point startPosition, Point startSpeed, Point nextWaypoint)
	{
		var halfwayX = (nextWaypoint.X + startPosition.X) / 2;
		var halfwayY = (nextWaypoint.Y + startPosition.Y) / 2;
		var waypoint = new Point(halfwayX, halfwayY);
		return waypoint;
		//throw new NotImplementedException("Finding waypoint to right not implemented");
		//var reverseTrajectory = plotReverseTrajectory(lz.Item1, new Point(CRITICAL_HORIZONTAL_SPEED, -CRITICAL_VERTICAL_SPEED), position.Y);
	}

	/// <summary>
	/// Determines where the lander with end up if we only break vertically for landing
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="startSpeed"></param>
	/// <param name="endY"></param>
	/// <returns></returns>
	private static Point plotTrajectory(Point startPosition, Point startSpeed, int endY)
	{
		var distanceY = endY - startPosition.Y;
		var verticalAcceleration = accelerationFor(startSpeed.Y, -CRITICAL_VERTICAL_SPEED, distanceY);
		var time = trajectoryTime(distanceY, startSpeed.Y, verticalAcceleration);
		var horizontalMovement = trajectoryDistance(startSpeed.X, 0, time);
		var endX = startPosition.X + (int)Math.Round(horizontalMovement);
		Console.Error.WriteLine("Trajectory: Vertical:{0}m, {1}m/s2, {2}s => {3}m to RIGHT.", distanceY, verticalAcceleration, time, horizontalMovement);
		return new Point(endX, endY);
	}

	//private static Tuple<Point, Point> nextWaypoint(Point position, Tuple<Point,Point> lz, List<Point> topology)
	//{
	//	//TODO: Naive waypoint!
	//	return lz;
	//}

	//private static Tuple<int, int> buildCommand(Point position, Point speed, int currentAngle, Tuple<Point, Point> waypoint)
	//{
	//	//Do we need to go upwards to get to the next waypoint?
	//	if (position.Y < waypoint.Item1.Y)
	//	{
	//		throw new NotSupportedException("The lander does not handle going upwards to land");
	//	}

	//	//Do we need to go horizontally to get to the next waypoint?
	//	var needToGoLeft = position.X > waypoint.Item2.X;
	//	if (needToGoLeft)
	//		return buildCommand_GoLeft(position, speed, currentAngle, waypoint);

	//	var needToGoRight = position.X < waypoint.Item1.X;
	//	if (needToGoRight)
	//		return buildCommand_GoRight(position, speed, currentAngle, waypoint);



	//	return buildCommand_VerticalLanding(position, speed, currentAngle, waypoint);
	//}

	//private static Tuple<int, int> buildCommand_GoRight(Point position, Point speed, int currentAngle, Tuple<Point, Point> waypoint)
	//{
	//	//TODO: What if we are already decending? Need to stop decent first?
	//	Console.Error.Write("Going to the right");
	//	var throttle = currentAngle < 0 ? 0 : 4; 

	//	var desiredFinalSpeed = CRITICAL_HORIZONTAL_SPEED; // Determine based on the next waypoint on the way
	//	var possibleAcceleration = Math.Sin(Math.PI * MAX_ANGLE_KEEP_VSPEED / 180);
	//	var breakingDistance = Math.Sign(speed.X) * distanceFor(speed.X, desiredFinalSpeed, possibleAcceleration);
	//	//Console.Error.WriteLine("Breaking distance is: " + breakingDistance.ToString());

	//	var centerOfWaypoint = (waypoint.Item1.X + waypoint.Item2.X) / 2;
	//	var leftAreaOfWaypoint = (3 * waypoint.Item1.X + waypoint.Item2.X) / 4;
	
	//	if (position.X + breakingDistance > centerOfWaypoint)
	//	{
	//		Console.Error.WriteLine(", breaking for center.");
	//		//Console.Error.WriteLine(string.Format(", breaking for center ({0}, {1}, {2}.", position, breakingDistance, centerOfWaypoint));
	//		return new Tuple<int, int>(MAX_ANGLE_KEEP_VSPEED, throttle);
	//	}
	//	else if (position.X + breakingDistance > leftAreaOfWaypoint && speed.X>desiredFinalSpeed)
	//	{
	//		Console.Error.WriteLine(", breaking for desired speed.");
	//		return new Tuple<int, int>(MAX_ANGLE_KEEP_VSPEED, throttle);
	//	}
	//	else if (position.X + breakingDistance > waypoint.Item1.X)
	//	{
	//		Console.Error.WriteLine(" with good speed.");
	//		return new Tuple<int, int>(0, speed.Y > 0 ? 3 : 4);
	//	}
	//	else
	//	{
	//		Console.Error.WriteLine(", accelerating.");
	//		return new Tuple<int, int>(-MAX_ANGLE_KEEP_VSPEED, 4);
	//	}
	//}

	//private static Tuple<int, int> buildCommand_GoLeft(Point position, Point speed, int currentAngle, Tuple<Point, Point> waypoint)
	//{
	//	//TODO: What if we are already decending? Need to stop decent first?
	//	Console.Error.Write("Going to the left");
	//	var throttle = currentAngle > 0 ? 0 : 4; 

	//	var desiredFinalSpeed = -CRITICAL_HORIZONTAL_SPEED; // Determine based on the next waypoint on the way
	//	var possibleAcceleration = Math.Sin(Math.PI * -MAX_ANGLE_KEEP_VSPEED / 180);
	//	var breakingDistance = Math.Sign(speed.X) * distanceFor(speed.X, desiredFinalSpeed, possibleAcceleration);

	//	var centerOfWaypoint = (waypoint.Item1.X + waypoint.Item2.X) / 2;
	//	var rightAreaOfWaypoint = (waypoint.Item1.X + 3 * waypoint.Item2.X) / 4;

	//	if (position.X + breakingDistance < centerOfWaypoint)
	//	{
	//		Console.Error.WriteLine(", breaking for center.");
	//		return new Tuple<int, int>(-MAX_ANGLE_KEEP_VSPEED, throttle);
	//	}
	//	else if (position.X + breakingDistance < rightAreaOfWaypoint && speed.X < desiredFinalSpeed)
	//	{
	//		Console.Error.WriteLine(", breaking for desired speed.");
	//		return new Tuple<int, int>(-MAX_ANGLE_KEEP_VSPEED, throttle);
	//	}
	//	else if (position.X + breakingDistance < waypoint.Item2.X)
	//	{
	//		Console.Error.WriteLine(" with good speed.");
	//		return new Tuple<int, int>(0, speed.Y > 0 ? 3 : 4);
	//	}
	//	else
	//	{
	//		Console.Error.WriteLine(", accelerating.");
	//		return new Tuple<int, int>(-MAX_ANGLE_KEEP_VSPEED, 4);
	//	}
	//}

	const int MAX_WAYPOINT_TIME = 30;

	private static Tuple<int, int> buildCommand_Waypoint(Point startPosition, Point startSpeed, Point endPosition)
	{
		//ax^2 + bx + c = 0     =>    x = -b +- SQRT(b^2 -4ac) / 2a

		// d = vt + 0.5*a*t^2
		// 0.5a * t^2 + v * t -d = 0     => t = -v +- SQRT(v^2 - 4*0.5a*(-d)) / (2*0.5a)

		var distanceUpwards = endPosition.Y - startPosition.Y;
		var speedUpwards = startSpeed.Y;

		////Fastest way to get to correct vertical pos
		//var accUpMinTime = Math.Sign(distanceUpwards) * 4 - GRAVITY;
		//var minTime = trajectoryTime(distanceUpwards, speedUpwards, accUpMinTime);
		//Console.Error.WriteLine(string.Format("Fastest vertical from {0} to {1} is {2} m/s2 in {3}s.", startPosition, endPosition, accUpMinTime, minTime));

		//Required acceleration to make it in MAX_WAYPOINT_TIME. If we can't make that we are dead anyway
		var maxTime = (double)MAX_WAYPOINT_TIME;
		double accUpMaxTime = trajectoryAcceleration(distanceUpwards, speedUpwards, maxTime);
		if (accUpMaxTime > 4 - GRAVITY)
		{
			//With max burst UP we can prolong it a while, but not MAX_WAYPOINT_TIME seconds
			accUpMaxTime = 4 - GRAVITY;
			maxTime = trajectoryTime(distanceUpwards, speedUpwards, accUpMaxTime);
		}
		else if (accUpMaxTime < -4 - GRAVITY)
		{
			//With max burst DOWN we can prolong it a while, but not MAX_WAYPOINT_TIME seconds
			throw new NotImplementedException("Not sure how to calculate max burst down..");
		}
		Console.Error.WriteLine(string.Format("Slowest vertical from {0} to {1} is {2} m/s2 in {3}s.", startPosition, endPosition, accUpMaxTime, maxTime));


		////So, we have between minT and maxT seconds to get horizontally where we want to go
		var distanceRight = endPosition.X - startPosition.X;
		var speedRight = startSpeed.X;
		var accRightMaxTime = trajectoryAcceleration(distanceRight, speedRight, maxTime);
		Console.Error.WriteLine(string.Format("To travel {0}m to the RIGHT in {3}s, with inital speed of {1}m/s, we need to accelerate with {2} m/s2.", distanceRight, speedRight, accRightMaxTime, maxTime));

		var acceleration = new Point((int)Math.Round(accRightMaxTime), (int)Math.Round(accUpMaxTime + GRAVITY));
		Console.Error.WriteLine("Angle for this force = " + acceleration.Angle);
		return new Tuple<int, int>(-acceleration.Angle, 4);

		//var accRightMinTime = trajectoryAcceleration(distanceRight, speedRight, minTime);
		//Console.Error.WriteLine(string.Format("Horizontal from {0} to {1} in {2}s requires acc {3} m/s2 .", startPosition, endPosition, minTime, maxAX));

		//if (accRightMinTime > 4 || accRightMinTime < -4)
		//{
		//	//We are faster vertically than horizontally
		//	Console.Error.WriteLine("We can't make it horizontally in the shortest possible timeframe");
			
		//}
		//else
		//{
		//	//We are faster horizontally than vertically
		//	Console.Error.WriteLine("It's a piece of cake to make it in the shortest timeframe");

		//}


		//Console.Error.WriteLine("TODO: Letting gravity do its work INSTEAD of steering.");
		//return new Tuple<int, int>(0, 0);

	}

	private static Tuple<int, int> buildCommand_LAND(Point position, Point speed, int lzY)
	{
		Console.Error.WriteLine("Building commmand for LANDING");

		if (speed.Y <= CRITICAL_VERTICAL_SPEED)
		{
			Console.Error.WriteLine("At critical speed! Hold on tight!");
			return new Tuple<int, int>(0, 4);
		}

		var distance = position.Y - lzY;
		Console.Error.WriteLine("Vertical distance to LZ: " + distance.ToString());

		var breakingDistance = distanceFor(speed.Y, CRITICAL_VERTICAL_SPEED, 4 - GRAVITY);
		Console.Error.WriteLine(string.Format("Min breaking distance: {0}m ({1:P}%)", breakingDistance, distance/breakingDistance));

		if (distance >= breakingDistance * 0.95)
		{
			return new Tuple<int, int>(0, 4);
		}
		else if (distance >= breakingDistance * 0.9)
		{
			return new Tuple<int, int>(0, 3);
		}
		else if (distance >= breakingDistance * 0.7)
		{
			return new Tuple<int, int>(0, 2);
		}
		else if (distance >= breakingDistance * 0.6)
		{
			return new Tuple<int, int>(0, 1);
		}
		else
		{
			return new Tuple<int, int>(0, 0);
		}
	}

	//private static Tuple<int, int> buildCommand_VerticalLanding(Point position, Point speed, int currentAngle, Tuple<Point, Point> waypoint)
	//{
	//	Console.Error.WriteLine("Vertical landing");

	//	var angle = 0;
	//	var centerOfWaypoint = (waypoint.Item1.X + waypoint.Item2.X) / 2;

	//	if (speed.X > CRITICAL_HORIZONTAL_SPEED + 12 * currentAngle / 90.0 || position.X > centerOfWaypoint && speed.X > 0)
	//	{
	//		Console.Error.WriteLine("We are going too fast to the right");
	//		angle = 90;
	//	}
	//	else if (speed.X < -CRITICAL_HORIZONTAL_SPEED + 12 * currentAngle / 90.0 || position.X < centerOfWaypoint && speed.X < 0)
	//	{
	//		Console.Error.WriteLine("We are going too fast to the left");
	//		angle = -90;
	//	}
	//	else
	//	{
	//		Console.Error.WriteLine("We are good horizontally");
	//		angle = 0;
	//	}

	//	var distance = position.Y - waypoint.Item1.Y;
	//	Console.Error.WriteLine("Vertical distance to LZ: " + distance.ToString());

	//	var breakingDistance = distanceFor(speed.Y, CRITICAL_VERTICAL_SPEED, 4 - GRAVITY);
	//	Console.Error.WriteLine("Min breaking distance: " + breakingDistance.ToString());

	//	if (breakingDistance >= distance * 0.9 || distance < 50)
	//	{
	//		Console.Error.WriteLine("No room for horizontal control. Just take us down!");
	//		return new Tuple<int, int>(0, 4);
	//	}
	//	else if (breakingDistance >= distance * 0.75)
	//	{
	//		Console.Error.WriteLine("Max break thrust with sideway control.");
	//		return new Tuple<int, int>(angle / 6, 4);//Max 15 degrees angle, so we get enough vertical breaking
	//	}
	//	else if (angle == 0)
	//	{
	//		Console.Error.WriteLine("Controlled vertical descent.");
	//		if (breakingDistance >= distance * 0.5)
	//		{
	//			return new Tuple<int, int>(0, 3);
	//		}
	//		else if (breakingDistance >= distance * 0.25)
	//		{
	//			return new Tuple<int, int>(0, 2);
	//		}
	//		else
	//		{
	//			return new Tuple<int, int>(0, 0);
	//		}
	//	}
	//	else
	//	{
	//		Console.Error.WriteLine("Controlled descent with horizontal control.");
	//		if (breakingDistance >= distance * 0.5)
	//		{
	//			return new Tuple<int, int>(angle / 2, 4);
	//		}
	//		else
	//		{
	//			return new Tuple<int, int>(angle, 4);
	//		}
	//	}
	//}










	private static Tuple<Point, Point> findLandingZone(List<Point> topology)
	{
		for (int i = 1; i < topology.Count(); i++)
		{
			var p1 = topology[i - 1];
			var p2 = topology[i];
			if (p1.Y == p2.Y)
			{
				return new Tuple<Point, Point>(p1, p2);
			}
		}
		throw new ArgumentException("No possible LZ in the topology!");
	}


	static double trajectoryDistance(double v, double a, double t)
	{
		return v * t + 0.5 * a * t * t;
	}
	static double trajectoryAcceleration(double d, double v, double t)
	{
		return (d - v * t) * 2 / t / t;
	}
	static double trajectoryTime(double d, double v, double a)
	{
		// d = v*t + a/2*t^2
		// a/2 * t^2 + v * t - d = 0     => t = -v/2/(a/2) +- SQRT(v^2 - 4*(a/2)*(-d)) / (2*a/2)  => t = -v/a +- SQRT(v^2 + 2ad) / a
		//  A  * x^2 + B * x + C = 0     => x = -B/2A +- SQRT(B^2-4AC) / 2A
		// x = t, A = a/2, B = v, C = -d
		if (a == 0)
		{
			return d / v;
		}
		else
		{
			var sqrt = Math.Abs(Math.Sqrt(v * v + 2 * a * d) / a);
			var result1 = -v / a + sqrt;
			var result2 = -v / a - sqrt;
			var shortest_time = new[] { result1, result2 }.Where(x => x >= 0).Min();
			return shortest_time;
		}
	}
	static double trajectoryStartSpeed(double d, double a, double t)
	{
		return d / t - 0.5 * a * t;
	}


	private static double distanceFor(int vStart, int vEnd, double a)
	{
		// vEnd^2 = vStart^2 + 2*a*d
		return (vEnd * vEnd - vStart * vStart) / 2 / a;
	}

	private static double accelerationFor(int vStart, int vEnd, double d)
	{
		return (vEnd * vEnd - vStart * vStart) / 2 / d;
	}

}