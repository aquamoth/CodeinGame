using System;
using System.Linq;
using System.Collections.Generic;
class P{static void Main(){
var c=Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
var l=new Dictionary<int, int>();
for(int i=0;i<c[7];i++){
var e=Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
l.Add(e[0], e[1]);
}
l.Add(c[3], c[4]);
while (true)
{
var q = Console.ReadLine().Split(' ');
var r = q.Take(2).Select(x => int.Parse(x)).ToArray();
Console.WriteLine((r[0]==-1||r[1]==l[r[0]]||(q.Last()!="LEFT")^(r[1]>l[r[0]]))?"WAIT":"BLOCK");
}}}