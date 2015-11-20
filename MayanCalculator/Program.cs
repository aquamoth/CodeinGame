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



		long number1 = 0;
		int S1 = int.Parse(Console.ReadLine());
		for (int x = 0; x < S1; x += H)
		{
			var digit = "";
			for (int i = 0; i < H; i++)
			{
				string num1Line = Console.ReadLine();
				digit += num1Line;
			}
			number1 = number1 * digits.Count + digits.IndexOf(digit);
		}
		Console.Error.WriteLine("Number 1: " + number1);


		long number2 = 0;
		int S2 = int.Parse(Console.ReadLine());
		for (int i = 0; i < S2; i+=H)
		{
			var digit = "";
			for (int x = 0; x < H; x++)
			{
				string num2Line = Console.ReadLine();
				digit += num2Line;
			}
			number2 = number2 * digits.Count + digits.IndexOf(digit);
		}
		Console.Error.WriteLine("Number 2: " + number2);


		string operation = Console.ReadLine();

		long result;
		switch (operation)
		{
			case "+": result = number1 + number2; break;
			case "-": result = number1 - number2; break;
			case "*": result = number1 * number2; break;
			case "/": result = number1 / number2; break;

			default:
				throw new NotImplementedException("Operation: " + operation);
		}

		Console.Error.WriteLine(number1 + " " + operation + " " + number2 + " = " + result);



		var resultString = toMayan(result, digits, L).ToArray();
		for (int y = 0; y < resultString.Length; y++)
		{
			Console.WriteLine(resultString[y]);
		}
	}

	private static IEnumerable<string> toMayan(long number, List<string> mayanDigits, int L)
	{
		//if (number == 0)
		//{
		//	return new string[0];
		//}
		//else
		//{
			var firstNumber = number / mayanDigits.Count;
			var first = firstNumber > 0 ? toMayan(firstNumber, mayanDigits, L) : new string[0];

			var nextDigit = (int)(number % mayanDigits.Count);
			Console.Error.WriteLine(nextDigit);
			var nextMayanDigit = mayanDigits[nextDigit];

			var nextMayanDigitRows = nextMayanDigit
				.Select((c, index) => new { character = c, part = index / L })
				.GroupBy(x => x.part, x => x.character)
				.Select(x => string.Join("", x))
				.ToArray();

			return first.Concat(nextMayanDigitRows);
		//}
	}
}