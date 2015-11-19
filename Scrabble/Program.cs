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
	public class Word
	{
		public string Name { get; private set; }
		public int Index { get; private set; }

		public Word(string name, int index)
		{
			Name = name;
			Index = index;
		}

		public string Chars
		{
			get
			{
				return string.Join("", this.Name.OrderBy(x => x));
			}
		}

		public int Value
		{
			get
			{
				return this.Name.Sum(c => valueOf(c));
			}
		}

		private int valueOf(char c)
		{
			switch (c)
			{
				case 'e':
				case 'a':
				case 'i':
				case 'o':
				case 'n':
				case 'r':
				case 't':
				case 'l':
				case 's':
				case 'u':
					return 1;
				case 'd':
				case 'g':
					return 2;
				case 'b':
				case 'c':
				case 'm':
				case 'p':
					return 3;
				case 'f':
				case 'h':
				case 'v':
				case 'w':
				case 'y':
					return 4;
				case 'k':
					return 5;
				case 'j':
				case 'x':
					return 8;
				case 'q':
				case 'z':
					return 10;

				default:
					throw new NotSupportedException("Unknown char: " + c);
			}
		}


		public override int GetHashCode()
		{
			return this.Chars.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as Word;
			return other != null && other.Chars == this.Chars;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}

	
	static void Main(string[] args)
	{
		var dictionary = new Dictionary<int, HashSet<Word>>();

		int N = int.Parse(Console.ReadLine());
		for (int i = 0; i < N; i++)
		{
			string W = Console.ReadLine();

			//Filtrera bort alla dict. ord > 7 texten
			if (W.Length <= 7)
			{
				var word = new Word(W, i);

				//Gruppera dict. efter poäng
				HashSet<Word> hashSet;
				if (!dictionary.TryGetValue(word.Value, out hashSet))
				{
					hashSet = new HashSet<Word>();
					dictionary.Add(word.Value, hashSet);
				}
				hashSet.Add(word);
			}
		}


		string LETTERS = Console.ReadLine();

		LETTERS = string.Join("", LETTERS.OrderBy(x => x));

		var allUniqueVariants = LETTERS.Variants()
			.Distinct()
			.Select(x => new Word(x, 0))
			.OrderByDescending(x => x.Value)
			.ToArray();

		foreach (var word in allUniqueVariants)
		{
			HashSet<Word> hashSet;
			if (dictionary.TryGetValue(word.Value, out hashSet))
			{
				var found = hashSet.Contains(word);

				if (found)
				{
					var lookupTable = hashSet.ToLookup(x => x.Chars, x => x);
					var wordToFind = lookupTable[word.Chars].OrderBy(x => x.Index).First();

					Console.WriteLine(wordToFind);
					return;

				}

			}
		}


		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.WriteLine("invalid word");
	}
}

public static class Extensions
{
	public static IEnumerable<string> Variants(this string s)
	{
		if (s != "")
			yield return s;

		//var chars = s.ToArray();
		for (var i = 0; i < s.Length; i++)
		{
			var start = i == 0 ? "" : s.Substring(0, i);
			var end = s.Substring(i + 1);
			var variant = start + end;

			foreach (var result in variant.Variants())
			{
				yield return result;
			}
		}
	}
}

/*
 * //Filtrera bort alla dict. ord > 7 texten
 * //Gruppera dict. efter poäng
 * //Sortera varje ord i dict. i bokstavsordning och spara som hash
 * 
 * Ta alla egna tecken och sortera i bokstavsordning
 * Skapa varje unik kombination av bokstäver
 * 
 * För varje kombo, testa vilka högsta poäng-ord som matchar i hashen
 * Om flera ord ger korrekt lösning, hitta vilket ord som låg överst i dict.
 *
 * 
*/