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
		int L = int.Parse(Console.ReadLine());
		int H = int.Parse(Console.ReadLine());
		string T = Console.ReadLine();


		var alphabeth = new List<string>();
		for (int i = 0; i < H; i++)
		{
			alphabeth.Add(Console.ReadLine());
		}

		var numberOfCharacters = alphabeth[0].Length / L;

		for (var y = 0; y < H; y++)
		{
			foreach (var character in T.ToUpper())
			{
				int characterIndex = character - 'A';
				if (characterIndex < 0 || characterIndex >= numberOfCharacters)
					characterIndex = numberOfCharacters - 1;

				var line = string.Join("", alphabeth[y].Skip(characterIndex * L).Take(L).ToArray());
				Console.Write(line);
			}
			Console.WriteLine();
		}
	}
}