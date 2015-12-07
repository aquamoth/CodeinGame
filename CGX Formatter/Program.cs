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

		Console.WriteLine(dom.Print());
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
			case '(':
				element = new BLOCK(raw);
				break;

			default:
				if (lastDom != null)
					throw new ArgumentException("Primitive types does not have a left hand expression");

				element = PRIMITIVE_TYPE.From(raw);
				break;
		}

		LTrim(raw);

		if (raw.Length > 0 && raw[0] == KEY_VALUE.DELIMITER)
			element = new KEY_VALUE(raw, element);




		return element;
	}

	protected static void LTrim(StringBuilder raw)
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

	public abstract string Print(int indentation = 0);

	protected string indent(int indentation)
	{
		return string.Join("", Enumerable.Repeat(' ', indentation).ToArray());
	}
}

class KEY_VALUE : Element
{
	public const char DELIMITER = '=';

	public Element Key { get; private set; }
	public Element Value { get; private set; }

	public KEY_VALUE(StringBuilder raw, Element key)
	{
		if (key == null)
			throw new ArgumentException("KEY_VALUE expects an ELEMENT to the left of the equal sign.");
		Key = key;

		if (raw[0] != DELIMITER)
			throw new ArgumentException("KEY_VALUE expected delimiter token");
		raw.Remove(0, 1);

		Value = Element.From(raw, null);
	}

	public override string Print(int indentation = 0)
	{
		var sb = new StringBuilder();
		sb.Append(Key.Print(indentation));
		sb.Append(DELIMITER.ToString());
		if (Value is PRIMITIVE_TYPE)
		{
			sb.Append(Value.Print(0));
		}
		else
		{
			sb.AppendLine();
			sb.Append(Value.Print(indentation));
		}
		return sb.ToString();
	}

}

class BLOCK : Element
{
	const char BLOCK_START = '(';
	const char BLOCK_SEPARATOR = ';';
	const char BLOCK_END = ')';

	readonly List<Element> _elements = new List<Element>();

	public BLOCK(StringBuilder raw)
	{
		if (raw[0] != BLOCK_START)
			throw new ArgumentException("Expected block start token");

		do
		{
			raw.Remove(0, 1);
			LTrim(raw);

			//TODO: Special case, test for empty block
			if (raw[0] == BLOCK_END)
				break;

			if (raw[0] != BLOCK_SEPARATOR)
			{
				var element = Element.From(raw, null);
				_elements.Add(element);
				LTrim(raw);
			}
		}
		while (raw[0] == BLOCK_SEPARATOR);

		if (raw[0] != BLOCK_END)
			throw new ArgumentException("Expected end of block");

		raw.Remove(0, 1);
	}

	public override string Print(int indentation = 0)
	{
		var sb = new StringBuilder();
		sb.Append(indent(indentation));
		sb.AppendLine("(");

		for (int i = 0; i < _elements.Count; i++)
		{
			var element = _elements[i];
			sb.Append(element.Print(indentation + 4));
			if (i < _elements.Count - 1)
				sb.Append(BLOCK_SEPARATOR);
			sb.AppendLine();
		}

		sb.Append(indent(indentation));
		sb.Append(")");
		return sb.ToString();
	}
}


#region PRIMITIVE_TYPE

class PRIMITIVE_STRING : PRIMITIVE_TYPE
{
	public const char DELIMITER = '\'';

	public PRIMITIVE_STRING(StringBuilder raw)
	{
		var index = 1;
		while (index < raw.Length)
		{
			var nextChar = raw[index];
			if (nextChar == DELIMITER)
				break;

			index++;
		}

		if (index == raw.Length)
			throw new ApplicationException("Found End of Stream while parsing PRIMITIVE_STRING");

		index++;

		Value = raw.ToString(0, index);
		raw.Remove(0, index);
	}
}

class PRIMITIVE_NUMBER : PRIMITIVE_TYPE
{
	public PRIMITIVE_NUMBER(StringBuilder raw)
	{
		var index = 0;
		while (index < raw.Length)
		{
			var nextChar = raw[index];
			if (!Char.IsNumber(nextChar))
				break;

			index++;
		}

		if (index == raw.Length)
			throw new ApplicationException("Found End of Stream while parsing PRIMITIVE_NUMBER");

		Value = raw.ToString(0, index);
		raw.Remove(0, index);
	}
}

class PRIMITIVE_BOOL : PRIMITIVE_TYPE
{
	public const string TRUE_TOKEN = "true";
	public const string FALSE_TOKEN = "false";

	public PRIMITIVE_BOOL(StringBuilder raw)
	{
		if (tryRead(raw, TRUE_TOKEN))
			return;
		if (tryRead(raw, FALSE_TOKEN))
			return;

		throw new ArgumentException("PRIMITIVE_BOOL expected true or false");
	}

	public static bool CanParse(StringBuilder raw)
	{
		if (startsWith(raw, TRUE_TOKEN))
			return true;
		if (startsWith(raw, FALSE_TOKEN))
			return true;
		return false;
	}

	private bool tryRead(StringBuilder raw, string token)
	{
		if (startsWith(raw, token))
		{
			Value = token;
			raw.Remove(0, token.Length);
			return true;
		}
		return false;
	}

	private static bool startsWith(StringBuilder raw, string token)
	{
		return (raw.Length >= token.Length && raw.ToString(0, token.Length) == token);
	}
}

abstract class PRIMITIVE_TYPE : Element
{
	public string Value { get; protected set; }

	public static Element From(StringBuilder raw)
	{
		switch (dataTypeOf(raw))
		{
			case dataType.Number: return new PRIMITIVE_NUMBER(raw);
			case dataType.String: return new PRIMITIVE_STRING(raw);
			case dataType.Boolean: return new PRIMITIVE_BOOL(raw);
			default:
				throw new NotSupportedException();
		}
	}

	static dataType dataTypeOf(StringBuilder raw)
	{
		if (raw[0] == PRIMITIVE_STRING.DELIMITER)
			return dataType.String;
		if (Char.IsNumber(raw[0]))
			return dataType.Number;
		if (PRIMITIVE_BOOL.CanParse(raw))
			return dataType.Boolean;
		throw new ArgumentException("Unsupported data type");
	}

	enum dataType
	{
		Number,
		String,
		Boolean
	}

	public override string Print(int indentation = 0)
	{
		return indent(indentation) + Value;
	}
}

#endregion PRIMITIVE_TYPE
