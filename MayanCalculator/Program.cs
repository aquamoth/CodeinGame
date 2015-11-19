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
		const int NUMBER_OF_DIGITS = 20;

	
		var digits = Enumerable.Repeat("",NUMBER_OF_DIGITS).ToList();

		string[] inputs = Console.ReadLine().Split(' ');
		int L = int.Parse(inputs[0]);
		int H = int.Parse(inputs[1]);
		for (int i = 0; i < H; i++)
		{
			string numeral = Console.ReadLine();
			for (int l = 0; l < NUMBER_OF_DIGITS; l++)
			{
				var line = numeral.Substring(l * L, L);
				digits[l] = digits[l] + line;
			}
		}


		var digit1 = "";
		int S1 = int.Parse(Console.ReadLine());
		for (int i = 0; i < S1; i++)
		{
			string num1Line = Console.ReadLine();
			digit1 += num1Line;
		}

		var digit2 = "";
		int S2 = int.Parse(Console.ReadLine());
		for (int i = 0; i < S2; i++)
		{
			string num2Line = Console.ReadLine();
			digit2 += num2Line;
		}


		var number1 = digits.IndexOf(digit1);
		var number2 = digits.IndexOf(digit2);




		string operation = Console.ReadLine();

		int result;

		switch (operation)
		{
			case "+":
				result = number1 + number2;
				break;

			default:
				throw new NotImplementedException("Operation: " + operation);
		}




		var resultString = digits[result]
			.Select((c, index) => new { character = c, part = index / L })
			.GroupBy(x => x.part, x => x.character)
			.Select(x => string.Join("", x))
			.ToArray();

		for (int y = 0; y < resultString.Length; y++)
		{
			Console.WriteLine(resultString[y]);
		}

	}
}