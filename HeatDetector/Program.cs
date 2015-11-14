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
	class Point
	{
		public int X { get; set; }
		public int Y { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", X, Y);
		}
	}

	static void Main(string[] args)
	{
		string[] inputs;
		inputs = Console.ReadLine().Split(' ');
		int W = int.Parse(inputs[0]); // width of the building.
		int H = int.Parse(inputs[1]); // height of the building.
		int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
		inputs = Console.ReadLine().Split(' ');
		int X0 = int.Parse(inputs[0]);
		int Y0 = int.Parse(inputs[1]);

		var searchArea = new Point[2] { new Point { X = 0, Y = 0 }, new Point { X = W - 1, Y = H - 1 } };
		//Console.Error.WriteLine("Initial area: (" + searchArea[0] + ") - (" + searchArea[1] + ")");

		var batman = new Point { X = X0, Y = Y0 };

		// game loop
		while (true)
		{
			string BOMBDIR = Console.ReadLine(); // the direction of the bombs from batman's current location (U, UR, R, DR, D, DL, L or UL)
			//Console.Error.WriteLine("Bomb dir: " + BOMBDIR);

			if (BOMBDIR.Contains("U"))
			{
				searchArea[1].Y = batman.Y - 1;
			}
			else if(BOMBDIR.Contains("D"))
			{
				searchArea[0].Y = batman.Y + 1;
			}
			else
			{
				searchArea[0].Y = searchArea[1].Y = batman.Y;
			}

			if (BOMBDIR.Contains("L"))
			{
				searchArea[1].X = batman.X - 1;
			}
			else if (BOMBDIR.Contains("R"))
			{
				searchArea[0].X = batman.X + 1;
			}
			else
			{
				searchArea[0].X = searchArea[1].X = batman.X;
			}

			//Console.Error.WriteLine("Search area: (" + searchArea[0] + ") - (" + searchArea[1] + ")");


			batman.X = (searchArea[0].X + searchArea[1].X) / 2;
			batman.Y = (searchArea[0].Y + searchArea[1].Y) / 2;
			Console.WriteLine(batman); // the location of the next window Batman should jump to.
		}
	}
}