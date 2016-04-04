﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
	static void Main(string[] args)
	{
		string[] inputs = Console.ReadLine().Split(' ');

		int W = int.Parse(inputs[0]);
		int H = int.Parse(inputs[1]);
		string IMAGE = Console.ReadLine();
		var scores = decodeDWE(IMAGE, W, H);
		log(scores.Content.Length);

		var notes = enumerate(scores);
		Console.WriteLine(string.Join(" ", notes.Select(note => note.ToString())));



		Console.ReadLine();
	}

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
		//log("Skipping to start of staffs");
		var x = findStartOfStaffs(bitmap);

		var outsideStaffIndexes = staffPositions(bitmap.Column(x).ToArray());
		log("Staff indexes: {0}", string.Join(", ", outsideStaffIndexes));
		eraseStaffs(bitmap, outsideStaffIndexes);

		var staffThickness = outsideStaffIndexes[1] - outsideStaffIndexes[0];
		var staffDistance = outsideStaffIndexes[2] - outsideStaffIndexes[0];
		log("Staff heights: {0}", staffDistance);
		var notePositions = getNotePositions(outsideStaffIndexes).ToArray();
		while (x < bitmap.Width)
		{
			//log("x={0}", x);
			var column = bitmap.Column(x).ToArray();
			var centerY = verticalCenterOfNote(column, staffDistance);
			if (centerY.HasValue)
			{
				var centerX = x + staffDistance / 2;
				log("Found note at: ({0}, {1})", centerX, centerY);
				var duration = durationAt(bitmap, centerX, centerY.Value + staffThickness);
				var pitch = pitchAt(centerY.Value, notePositions);
				yield return new Note { Pitch = pitch, Duration = duration };

				//Step out of note
				x += staffDistance;
			}
			x++;
		}
	}

	private static IEnumerable<int> getNotePositions(int[] outsideStaffIndexes)
	{
		return outsideStaffIndexes
			.Skip(1)
			.Select((y, index) => (y + outsideStaffIndexes[index]) / 2);
	}

	private static void eraseStaffs(Bitmap bitmap, int[] outsideStaffIndexes)
	{
		for (int i = 0; i < outsideStaffIndexes.Length; i += 2)
		{
			var y1 = outsideStaffIndexes[i];
			var y2 = outsideStaffIndexes[i + 1];
			log("Erasing staff between {0} - {1}", y1, y2);
			for (var x = 0; x < bitmap.Width; x++)
			{
				for (var y = y1; y < y2; y++)
				{
					bitmap[x, y] = false;
				}
			}
		}
	}

	private static int findStartOfStaffs(Bitmap bitmap)
	{
		var x = 0;
		while (!bitmap.Column(x).Any(pixel => pixel == true))
		{
			x++;
		}
		log("Staffs found at {0}.", x);
		return x;
	}

	private static NoteDuration durationAt(Bitmap bitmap, int centerX, int centerY)
	{
		var isBlackNote = bitmap[centerX, centerY];
		log("Black note? at ({0}, {1})", centerX, centerY);
		return isBlackNote ? NoteDuration.Q : NoteDuration.H;
	}

	private static char pitchAt(int centerY, int[] notePositions)
	{
		var noteIndex = notePositions
			.Select((y, index) => new { index = index, distance = Math.Abs(y - centerY) })
			.OrderBy(x => x.distance)
			.Select(x => x.index)
			.First();
		return new[] { 'F', 'E', 'D', 'C', 'B', 'A', 'G', 'F', 'E', 'D', 'C' }[noteIndex];
	}

	private static int heighsOf(int[] staffCenters)
	{
		log("HeightOf: {0}", string.Join(", ", staffCenters));
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

	private static int? verticalCenterOfNote(bool[] column, int staffHeights)
	{
		var numberOfPixelsSet = column.Where(px => px == true).Count();
		if (numberOfPixelsSet == 0) //Skip empty columns
			return null;
		log("Found {0} pixels", numberOfPixelsSet);
		if (numberOfPixelsSet > staffHeights) // note tails
			return null;

		log("Identifying center of note");
		var setPixelsInColumn = column.Select((set, y) => new { set = set, y = y }).Where(x => x.set).Select(x => x.y);
		var centerY = setPixelsInColumn.Average();
		return (int)centerY;
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

	static void log(object arg)
	{
		log("{0}", arg);
	}
	static void log(string format, params object[] args)
	{
		if (logTimer == null)
		{
			logTimer = new Stopwatch();
			logTimer.Start();
		}
		Console.Error.Write(logTimer.ElapsedMilliseconds);
		Console.Error.Write(" ms: ");
		Console.Error.WriteLine(format, args);
	}
	static Stopwatch logTimer = null;
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
