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
		string MESSAGE = Console.ReadLine();
		var messageBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(MESSAGE);

		var byteIndex = 0;
		var bitValue = 64;
		var isStart = true;
		var lastWasSet = false;
		while (byteIndex < messageBytes.Length)
		{
			var isSet = (messageBytes[byteIndex] & bitValue) != 0;
			//Console.Error.WriteLine("Byte: " + byteIndex.ToString());
			//Console.Error.WriteLine("Bit: " + bitValue.ToString());

			if (isStart || isSet != lastWasSet)
			{
				if (!isStart)
					Console.Write(" ");
				else
					isStart = false;

				Console.Write(isSet ? "0 " : "00 ");
			}

			Console.Write("0");


			lastWasSet = isSet;

			bitValue /= 2;
			if (bitValue == 0)
			{
				bitValue = 64;
				byteIndex++;
			}
		}
	}
}