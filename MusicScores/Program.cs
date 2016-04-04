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
		string[] inputs = Console.ReadLine().Split(' ');
		//Console.Error.WriteLine(string.Join(" ", inputs));

		int W = int.Parse(inputs[0]);
		int H = int.Parse(inputs[1]);
		string IMAGE = Console.ReadLine();
		//Console.Error.WriteLine(IMAGE.Length);
		//Console.Error.WriteLine(IMAGE);

		var scores = decodeDWE(IMAGE, W, H);
		Console.Error.WriteLine(scores.Content.Length);

		var notes = enumerate(scores);
		Console.WriteLine(string.Join(" ", notes.Select(note => note.ToString())));

		// Write an action using Console.WriteLine()
		// To debug: Console.Error.WriteLine("Debug messages...");

		Console.ReadLine();
	}

	//public static void print(Bitmap bitmap)
	//{
	//	for (var y = 0; y < bitmap.Height; y++)
	//	{
	//		var row = bitmap.Content.Skip(y * bitmap.Width).Take(bitmap.Width);
	//		var bitsAsChar = row.Select(x => x ? "*" : ".");
	//		Console.Error.WriteLine(string.Join("", bitsAsChar));
	//	}
	//}

	public static Bitmap decodeDWE(string image, int width, int height)
	{
		var bits = AllBitsIn(image).ToArray();
		return new Bitmap(width, height, bits);
	}

	public static IEnumerable<bool> AllBitsIn(string image)
	{
		var parts = image.Split(' ');
		for (var index = 0; index < parts.Length; index += 2)
		{
			var isBitSet = parts[index] == "B";
			var count = int.Parse(parts[index + 1]);
			for (var i = 0; i < count; i++)
				yield return isBitSet;
		}
	}

	public static IEnumerable<Note> enumerate(Bitmap bitmap)
	{
		var x = 0;
		//Console.Error.WriteLine("Skipping to start of staffs");
		while (!bitmap.Column(x).Any(pixel => pixel == true))
			x++;

		Console.Error.WriteLine("Staffs found at {0}.", x);

		var outsideStaffIndexes = staffPositions(bitmap.Column(x).ToArray());
		Console.Error.WriteLine("Staff indexes: {0}", string.Join(", ", outsideStaffIndexes));
		var staffCenters = centersOf(outsideStaffIndexes);
		var staffHeights = heighsOf(staffCenters.ToArray());

		while (x < bitmap.Width)
		{
			var column = bitmap.Column(x).ToArray();
			var noteIndexes = startOfNote(column, outsideStaffIndexes);
			if (noteIndexes != null)
			{
				var noteIsBetweenLines = outsideStaffIndexes.ToList().IndexOf(noteIndexes.First()) % 2 == 1;
				Console.Error.WriteLine("Found note at: {0}. Between lines: {1}", string.Join(", ", noteIndexes), noteIsBetweenLines);

				var centerX = false ? x : x + staffHeights / 2;
				var centerY = noteIndexes.Sum() / 2;
				var duration = durationAt(bitmap, centerX, centerY);
				var pitch = pitchAt(centerY, outsideStaffIndexes);
				yield return new Note { Pitch = pitch, Duration = duration };

				//Step out of note
				x += staffHeights;
			}
			x++;
		}
	}

	private static NoteDuration durationAt(Bitmap bitmap, int centerX, int centerY)
	{
		var isBlackNote = bitmap[centerX, centerY];
		Console.Error.WriteLine("Black note? at ({0}, {1})", centerX, centerY);
		return isBlackNote ? NoteDuration.Q : NoteDuration.H;
	}

	private static char pitchAt(int centerY, int[] outsideStaffIndexes)
	{
		var staffIndex = outsideStaffIndexes
			.Select((y, index) => new { y, index })
			.Where(grp => grp.y > centerY)
			.Select(grp => grp.index)
			.First();
		return new[] { '?', 'F', 'E', 'D', 'C', 'B', 'A', 'G', 'F', 'E', 'D', 'C' }[staffIndex];
	}

	private static int heighsOf(int[] staffCenters)
	{
		Console.Error.WriteLine("HeightOf: {0}", string.Join(", ", staffCenters));
		var heights = staffCenters.Skip(1).Select((y, index) => y - staffCenters[index]).ToArray();
		return heights.Sum() / heights.Length;
	}

	private static IEnumerable<int> centersOf(int[] outsideStaffIndexes)
	{
		for (var i = 0; i < outsideStaffIndexes.Length; i+=2)
		{
			yield return (outsideStaffIndexes[i] + outsideStaffIndexes[i + 1]) / 2;
		}
	}

	private static int[] startOfNote(bool[] column, int[] staffIndexes)
	{
		var noteIndexes = staffIndexes.Select(index => column[index] ? index : -1).Where(index => index >= 0).ToArray();
		if (noteIndexes.Length != 2)
			return null;
		return noteIndexes;
	}

	private static int[] staffPositions(bool[] column)
	{
		var outsideStaffIndexes = new List<int>();
		var isInsideStaff = false;
		for (var y = 0; y < column.Length; y++)
		{
			if (column[y] ^ isInsideStaff)
			{
				isInsideStaff = column[y];
				outsideStaffIndexes.Add(isInsideStaff ? y - 1 : y);
			}
		}

		//TODO: Add virtual staff for C

		return outsideStaffIndexes.ToArray();
	}
}

class Note
{
	public char Pitch { get; set; }
	public NoteDuration Duration { get; set; }

	public override string ToString()
	{
		return string.Format("{0}{1}", Pitch, Duration);
	}
}

enum NoteDuration
{
	H,	//Half
	Q	//Quarter
}

class Bitmap
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	public bool[] Content { get; private set; }

	public Bitmap(int width, int height, bool[] content)
	{
		if (content.Length != width * height)
			throw new ApplicationException("Content length invalid");
		Width = width;
		Height = height;
		Content = content;
	}

	public bool this[int width, int height] { 
		get { return Content[width + height * Width]; } 
		set { Content[width + height * Width] = value; } 
	}

	public IEnumerable<bool> Column(int x)
	{
		for (var y = 0; y < Height; y++)
			yield return this[x, y];
	}
}
