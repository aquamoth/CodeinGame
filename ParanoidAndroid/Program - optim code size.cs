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
	e.Add(exitFloor, exitPos);
	while (true)
	{
		var r2 = Console.ReadLine().Split(' ');
		var r = r2.Take(2).Select(x => int.Parse(x)).ToArray();
		int f = r[0];
		int p = r[1];
		string direction = r2.Last();
		Console.WriteLine((f == -1 || p == e[f] || (direction == "RIGHT") ^ (p > e[f])) ? "WAIT" : "BLOCK");
	}
}}