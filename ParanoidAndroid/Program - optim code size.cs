using System;
using System.Linq;
using System.Collections.Generic;

class P{static void Main(){
	var r = Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
	int exitFloor = r[3]; 
	int exitPos = r[4]; 

	var elevators = new Dictionary<int, int>();
	int nbElevators = r[7];
	for (int i = 0; i < nbElevators; i++)
	{
		r = Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
		elevators.Add(r[0], r[1]);
	}
	elevators.Add(-1, -1);
	while (true)
	{
		var r2 = Console.ReadLine().Split(' ');
		r = r2.Take(2).Select(x => int.Parse(x)).ToArray();

		int cloneFloor = r[0]; 
		int clonePos = r[1];
		string direction = r2.Last();

		var targetPos = exitFloor == cloneFloor ? exitPos : elevators[cloneFloor];
		if (cloneFloor == -1 || clonePos == targetPos)
		{
			Console.WriteLine("WAIT");
		}
		else if ((direction == "RIGHT") ^ (clonePos > targetPos))
		{
			Console.WriteLine("WAIT");
		}
		else
		{
			Console.WriteLine("BLOCK");
		}
	}
}}