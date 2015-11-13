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
		string[] inputs;
		int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

		var lzIndex = -1;
		var topology = new List<Tuple<int, int>>();
		for (int i = 0; i < surfaceN; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
			int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
			topology.Add(new Tuple<int, int>(landX, landY));

			if (i > 0 && landY == topology[i - 1].Item2)
				lzIndex = i - 1;
		}

		var lzX1 = topology[lzIndex].Item1;
		var lzX2 = topology[lzIndex + 1].Item1;
		var lzY = topology[lzIndex].Item2;

		Console.Error.WriteLine(string.Format("LZ at x = {0} - {1}, y = {2}", lzX1, lzX2, lzY));

		const double GRAVITY = 3.711;
		const double CRITICAL_VERTICAL_SPEED = -39.0;
		const double CRITICAL_HORIZONTAL_SPEED = 20.0;

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

			//var lzY = topology.Where(x => x.Item1 < X).Last().Item2;



			var angle = 0;
			if (X > lzX2)
			{
				Console.Error.WriteLine("We need to go left");
				angle = 90;
			}
			else if (X < lzX1)
			{
				Console.Error.WriteLine("We need to go right");
				angle = -90;
			}
			else if (hSpeed > CRITICAL_HORIZONTAL_SPEED)
			{
				Console.Error.WriteLine("We are going too fast to the right");
				angle = 90;
			}
			else if (hSpeed < -CRITICAL_HORIZONTAL_SPEED)
			{
				Console.Error.WriteLine("We are going too fast to the left");
				angle = -90;
			}
			else
			{
				Console.Error.WriteLine("We are good horizontally");
				angle = 0;
			}

			var distance = Y - lzY;
			Console.Error.WriteLine("Vertical distance to LZ: " + distance.ToString());

			var breakingDistance = (vSpeed * vSpeed - CRITICAL_VERTICAL_SPEED * CRITICAL_VERTICAL_SPEED) / 2 / (4 - GRAVITY);
			Console.Error.WriteLine("Min breaking distance: " + breakingDistance.ToString());
			if (breakingDistance >= distance * 0.75)
			{
				angle /= 6; //Max 15 degrees angle, so we get enough vertical breaking
				Console.WriteLine(angle.ToString() + " 4");
			}
			else if (angle == 0)
			{
				if (breakingDistance >= distance * 0.5)
				{
					Console.WriteLine("0 3");
				}
				else if (breakingDistance >= distance * 0.25)
				{
					Console.WriteLine("0 2");
				}
				else
				{
					Console.WriteLine("0 0");
				}
			}
			else
			{
				angle /= 2;
				Console.WriteLine(angle.ToString() + " 4");
			}
		}
	}
}