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
	const string ALPHABETH = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	const int NUMBER_OF_ZONES = 30;

	static void Main(string[] args)
	{
		string magicPhrase = Console.ReadLine();

		var requiredLetters = magicPhrase.Distinct().ToList();

		var commands = new StringBuilder();
		var currentPosition = 0;
		var createdLetters = 0;

		for (int i = 0; i < magicPhrase.Length; i++)
		{
			var letter = magicPhrase[i];
			var positionOfLetter = requiredLetters.IndexOf(letter);

			var steps = (positionOfLetter - currentPosition + NUMBER_OF_ZONES) % NUMBER_OF_ZONES;
			if (steps != 0)
			{
				if (steps < NUMBER_OF_ZONES / 2)
				{
					//Step "steps" steps to the right
					commands.Append(Enumerable.Repeat('>', steps).ToArray());
				}
				else
				{
					//Step NUMBER_OF_ZONES - "steps" steps to the left
					commands.Append(Enumerable.Repeat('<', NUMBER_OF_ZONES - steps).ToArray());
				}
				currentPosition = (currentPosition + steps) % NUMBER_OF_ZONES;
			}

			//Create the letter if it hasn't been already
			if (currentPosition+1>createdLetters)
			{
				var index = ALPHABETH.IndexOf(letter);
				if (index > ALPHABETH.Length / 2)
				{
					commands.Append(Enumerable.Repeat('-', ALPHABETH.Length - index).ToArray());
				}
				else
				{
					commands.Append(Enumerable.Repeat('+', index).ToArray());
				}
				createdLetters = currentPosition + 1;
			}

			//Register the letter
			commands.Append('.');
		}

		Console.WriteLine(commands.ToString());
		//Console.WriteLine("+.>-.");
	}
}