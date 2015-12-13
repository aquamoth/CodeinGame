using System;
using System.Linq;
using System.Collections.Generic;
class P{static void Main(){
var a=Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
int exitFloor=a[3]; 
int exitPos=a[4];
var e=new Dictionary<int, int>();
for(int i=0;i<a[7];i++){
var b=Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
e.Add(b[0], b[1]);
}
e.Add(exitFloor, exitPos);
while (true)
{
var r2 = Console.ReadLine().Split(' ');
var r = r2.Take(2).Select(x => int.Parse(x)).ToArray();
Console.WriteLine((r[0]==-1||r[1]==e[r[0]]||(r2.Last()=="RIGHT")^(r[1]>e[r[0]]))?"WAIT":"BLOCK");
}}}