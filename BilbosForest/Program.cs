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

		var currentZones = Enumerable.Repeat(' ', NUMBER_OF_ZONES).ToArray();
		var currentPosition = 0;
		for (int i = 0; i < magicPhrase.Length; i++)
		{
			var letter = magicPhrase[i];
			//Console.Error.WriteLine("Solving letter: " + letter);

			var bestMove = 0;
			var bestTurn = 0;
			var bestCost = int.MaxValue;
			for (int positionOfLetter = 0; positionOfLetter < NUMBER_OF_ZONES; positionOfLetter++)
			{
				var zoneLetter = currentZones[positionOfLetter];
				var turningCost = (ALPHABETH.IndexOf(letter) - ALPHABETH.IndexOf(zoneLetter) + ALPHABETH.Length + ALPHABETH.Length / 2) % ALPHABETH.Length - ALPHABETH.Length / 2;
				var movingCost = (positionOfLetter - currentPosition + NUMBER_OF_ZONES + NUMBER_OF_ZONES / 2) % NUMBER_OF_ZONES - NUMBER_OF_ZONES / 2;
				var totalCost = Math.Abs(turningCost) + Math.Abs(movingCost);
				if (totalCost < bestCost || (totalCost == bestCost && zoneLetter == ' '))
				{
					bestMove = movingCost;
					bestTurn = turningCost;
					bestCost = totalCost;
					//Console.Error.WriteLine("..so far best is to turn #" + positionOfLetter + " from '" + zoneLetter + "' in " + turningCost + " turns. It takes " + movingCost + " steps to go there.");
				}
			}

			currentPosition = stepToZone(currentPosition, bestMove);

			//Create the letter if it hasn't been already
			if (bestTurn < 0)
			{
				Console.Write(Enumerable.Repeat('-', Math.Abs(bestTurn)).ToArray());
			}
			else if (bestTurn > 0)
			{
				Console.Write(Enumerable.Repeat('+', Math.Abs(bestTurn)).ToArray());
			}
			currentZones[currentPosition] = letter;


			//Register the letter
			Console.Write('.');
		}

		Console.WriteLine("");
	}

	private static int stepToZone(int currentPosition, int bestStep)
	{
		if (bestStep > 0)
		{
			Console.Write(Enumerable.Repeat('>', bestStep).ToArray());
		}
		else if (bestStep < 0)
		{
			Console.Write(Enumerable.Repeat('<', Math.Abs(bestStep)).ToArray());
		}
		currentPosition = (currentPosition + bestStep + NUMBER_OF_ZONES) % NUMBER_OF_ZONES;
		return currentPosition;
	}
}