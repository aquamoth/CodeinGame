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
		int N = int.Parse(Console.ReadLine()); // Number of elements which make up the association table.
		int Q = int.Parse(Console.ReadLine()); // Number Q of file names to be analyzed.

		var mimeTypes = new Dictionary<string, string>();
		for (int i = 0; i < N; i++)
		{
			string[] inputs = Console.ReadLine().Split(' ');
			string EXT = inputs[0]; // file extension
			string MT = inputs[1]; // MIME type.
			mimeTypes.Add(EXT.ToLowerInvariant(), MT);
		}
		for (int i = 0; i < Q; i++)
		{
			string FNAME = Console.ReadLine(); // One file name per line.

			var ext = System.IO.Path.GetExtension(FNAME);
			if (ext != "") ext = ext.Substring(1).ToLowerInvariant();
			if (!mimeTypes.ContainsKey(ext))
			{
				Console.WriteLine("UNKNOWN"); // For each of the Q filenames, display on a line the corresponding MIME type. If there is no corresponding type, then display UNKNOWN.
			}
			else
			{
				Console.WriteLine(mimeTypes[ext]);
			}
		}
	}
}