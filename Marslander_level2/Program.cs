using System;
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
	const int CRITICAL_VERTICAL_SPEED = -40;
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

			var waypoint = nextWaypoint(position, lz, topology);
			var command = buildCommand(position, speed, waypoint);

			Console.WriteLine(string.Format("{0} {1}", command.Item1, command.Item2));
		}
	}

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

	private static Tuple<Point, Point> nextWaypoint(Point position, Tuple<Point,Point> lz, List<Point> topology)
	{
		//TODO: Naive waypoint!
		return lz;
	}

	private static Tuple<int, int> buildCommand(Point position, Point speed, Tuple<Point, Point> waypoint)
	{
		//Do we need to go upwards to get to the next waypoint?
		if (position.Y < waypoint.Item1.Y)
		{
			throw new NotSupportedException("The lander does not handle going upwards to land");
		}

		//Do we need to go horizontally to get to the next waypoint?
		var needToGoLeft = position.X > waypoint.Item2.X;
		if (needToGoLeft)
			return buildCommand_GoLeft(position, speed, waypoint);

		var needToGoRight = position.X < waypoint.Item1.X;
		if (needToGoRight)
			return buildCommand_GoRight(position, speed, waypoint);




		Console.Error.WriteLine("Not sure what command to send..");
		return new Tuple<int, int>(0, 0);

		//var angle = 0;
		//if (needToGoLeft)
		//{
		//	Console.Error.WriteLine("We need to go left");
		//	angle = (hSpeed >= -CRITICAL_HORIZONTAL_SPEED * 1.25) ? 90 : 0;
		//}
		//else if (needToGoRight)
		//{
		//	Console.Error.WriteLine("We need to go right");
		//	angle = (hSpeed <= CRITICAL_HORIZONTAL_SPEED * 1.25) ? -90 : 0;
		//}
		//else if (hSpeed > CRITICAL_HORIZONTAL_SPEED)
		//{
		//	Console.Error.WriteLine("We are going too fast to the right");
		//	angle = 90;
		//}
		//else if (hSpeed < -CRITICAL_HORIZONTAL_SPEED)
		//{
		//	Console.Error.WriteLine("We are going too fast to the left");
		//	angle = -90;
		//}
		//else
		//{
		//	Console.Error.WriteLine("We are good horizontally");
		//	angle = 0;
		//}

		//var distance = Y - lzY;
		//Console.Error.WriteLine("Vertical distance to LZ: " + distance.ToString());

		//var breakingDistance = (vSpeed * vSpeed - CRITICAL_VERTICAL_SPEED * CRITICAL_VERTICAL_SPEED) / 2 / (4 - GRAVITY);
		//Console.Error.WriteLine("Min breaking distance: " + breakingDistance.ToString());
		//if (breakingDistance >= distance * 0.75)
		//{
		//	angle /= 6; //Max 15 degrees angle, so we get enough vertical breaking
		//	Console.WriteLine(angle.ToString() + " 4");
		//}
		//else if (angle == 0)
		//{
		//	if (breakingDistance >= distance * 0.5)
		//	{
		//		Console.WriteLine("0 3");
		//	}
		//	else if (breakingDistance >= distance * 0.25 || needToGoLeft || needToGoRight)
		//	{
		//		Console.WriteLine("0 2");
		//	}
		//	else
		//	{
		//		Console.WriteLine("0 0");
		//	}
		//}
		//else
		//{
		//	if (breakingDistance >= distance * 0.5)
		//	{
		//		angle /= 2;
		//	}
		//	Console.WriteLine(angle.ToString() + " 4");
		//}

	}

	private static Tuple<int, int> buildCommand_GoRight(Point position, Point speed, Tuple<Point, Point> waypoint)
	{
		//TODO: What if we are already decending? Need to stop decent first?
		Console.Error.Write("Going to the right");

		var desiredFinalSpeed = CRITICAL_HORIZONTAL_SPEED; // Determine based on the next waypoint on the way
		var possibleAcceleration = Math.Sin(Math.PI * MAX_ANGLE_KEEP_VSPEED / 180);
		var breakingDistance = distanceFor(speed.X, desiredFinalSpeed, possibleAcceleration);

		var centerOfWaypoint = (waypoint.Item1.X + waypoint.Item2.X) / 2;
	
		if (position.X + breakingDistance > centerOfWaypoint)
		{
			Console.Error.WriteLine(", breaking.");
			return new Tuple<int, int>(MAX_ANGLE_KEEP_VSPEED, 4);
		}
		else if (position.X + breakingDistance > waypoint.Item1.X)
		{
			Console.Error.WriteLine(" with good speed.");
			return new Tuple<int, int>(0, speed.Y > 0 ? 3 : 4);
		}
		else
		{
			Console.Error.WriteLine(", accelerating.");
			return new Tuple<int, int>(-MAX_ANGLE_KEEP_VSPEED, 4);
		}
	}

	private static Tuple<int, int> buildCommand_GoLeft(Point position, Point speed, Tuple<Point, Point> waypoint)
	{
		throw new NotImplementedException();
		////TODO: What if we are already decending? Need to stop decent first?
		//Console.Error.Write("Going to the left");
		//if (speed.X < -CRITICAL_HORIZONTAL_SPEED)
		//{
		//	Console.Error.WriteLine(" with good speed.");
		//	return new Tuple<int, int>(0, 4);//TODO: Should vary speed to maintain height?!
		//}
		//else
		//{
		//	Console.Error.WriteLine(", accelerating.");
		//	return new Tuple<int, int>(MAX_ANGLE_KEEP_VSPEED, 4);
		//}
	}

	private static double distanceFor(int initialSpeed, int finalSpeed, double acceleration)
	{
		return (initialSpeed * initialSpeed - finalSpeed * finalSpeed) / 2 / acceleration;
	}
}