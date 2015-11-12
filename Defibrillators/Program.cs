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
		string LON = Console.ReadLine();
		string LAT = Console.ReadLine();
		int N = int.Parse(Console.ReadLine());

		var defibrillators = new List<string[]>();
		for (int i = 0; i < N; i++)
		{
			string DEFIB = Console.ReadLine();
			defibrillators.Add(DEFIB.Split(';'));
		}

		//Console.Error.WriteLine( defibrillators.Count().ToString() + " defibrillators.");

		var arr = defibrillators
			.Select(x => new { defibrillator = x, distance = calc(LAT, LON, x[5], x[4]) })
			.OrderBy(x => x.distance)
			.ToArray();

		foreach (var defib in arr)
		{
			//Console.Error.WriteLine(string.Join(";", defib.defibrillator[1]) + "       :" + defib.distance);
		}

		var closest = arr
			.First();

		Console.WriteLine(closest.defibrillator[1]);
	}

	static double calc(string latAstr, string lonAstr, string latBstr, string lonBstr)
	{
		//Console.Error.WriteLine("calc for " + latBstr + ", " + lonBstr);
		var culture = System.Globalization.CultureInfo.CreateSpecificCulture("sv-SE");
		var latA = Convert.ToDouble(latAstr, culture);
		var lonA = Convert.ToDouble(lonAstr, culture);
		var latB = Convert.ToDouble(latBstr, culture);
		var lonB = Convert.ToDouble(lonBstr, culture);

		//Console.WriteLine(lonB);
		//Console.WriteLine(lonBstr);

		var x = (rad(lonB) - rad(lonA)) * Math.Cos((rad(latA) + rad(latB)) / 2);
		var y = rad(latB) - rad(latA);

		const double EARTH_RADIUS = 6371.0;
		var distance = Math.Sqrt(x * x + y * y) * EARTH_RADIUS;

		Console.Error.WriteLine("Distance: " + distance.ToString());
		return distance;
	}

	static double rad(double deg)
	{
		return Math.PI / 180 * deg;
	}
}