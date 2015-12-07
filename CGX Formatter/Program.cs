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
		var raw = new StringBuilder();
		int N = int.Parse(Console.ReadLine());
		for (int i = 0; i < N; i++)
		{
			string cGXLine = Console.ReadLine();
			raw.AppendLine(cGXLine);
		}

		Element temp = null;
		Element dom = null;
		do
		{
			temp = Element.From(raw, dom);
			if (temp != null)
				dom = temp;
		} while (temp != null);




		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine(dom.ToString());
	}
}


abstract class Element
{
	public static Element From(StringBuilder raw, Element lastDom)
	{
		LTrim(raw);

		if (raw.Length == 0)
			return null;

		Element element;
		var nextChar = raw[0];
		switch (nextChar)
		{


			default:
				if (lastDom != null)
					throw new ArgumentException("Primitive types does not have a left hand expression");

				element = new PRIMITIVE_TYPE(raw);
				break;
		}

		return element;
	}

	private static void LTrim(StringBuilder raw)
	{
		int index = 0;
		while (index < raw.Length)
		{
			var nextChar = raw[index];
			if (!Char.IsWhiteSpace(nextChar))
				break;
			index++;
		}
		if (index > 0)
			raw.Remove(0, index);
	}

}

class PRIMITIVE_TYPE : Element
{
	const char STRING_DELIMITER = '\'';


	public string Value { get; protected set; }

	public PRIMITIVE_TYPE(StringBuilder raw)
	{
		bool isInString = raw[0] == STRING_DELIMITER;
		var index = isInString ? 1 : 0;

		while (index < raw.Length)
		{
			var c = raw[index];

			if (isEndOfElement(c, isInString))
				break;

			index++;
		}

		if (index == raw.Length)
			throw new ApplicationException("Found End of Stream while parsing PRIMITIVE_TYPE");

		if (isInString)
			index++;

		Value = raw.ToString(0, index);
		raw.Remove(0, index);
	}

	public override string ToString()
	{
		return Value;
	}

	private bool isEndOfElement(char nextChar, bool isInString)
	{
		if (isInString)
		{
			return nextChar == STRING_DELIMITER;
		}
		else
		{
			if (Char.IsWhiteSpace(nextChar) || nextChar == ';')
				return true;
			return false;
		}
	}
}
