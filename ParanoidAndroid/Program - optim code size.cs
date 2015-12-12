using System;
using System.Linq;
using System.Collections.Generic;

class P{static void Main(){
	var a = Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
	int exitFloor = a[3]; 
	int exitPos = a[4];

	var e = new Dictionary<int, int>();
	for(int i=0;i<a[7];i++){
		var b = Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
		e.Add(b[0], b[1]);
	}
	e.Add(-1, -1);
	while (true)
	{
		var r2 = Console.ReadLine().Split(' ');
		var r = r2.Take(2).Select(x => int.Parse(x)).ToArray();

		int cloneFloor = r[0]; 
		int clonePos = r[1];
		string direction = r2.Last();

		var targetPos = exitFloor == cloneFloor ? exitPos : e[cloneFloor];
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