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
        var alignmentString = Console.ReadLine();

        int N = int.Parse(Console.ReadLine());

        var lines = new string[N];
        for (int i = 0; i < N; i++)
            lines[i] = Console.ReadLine();

        var alignment = (Alignment)Enum.Parse(typeof(Alignment), alignmentString);
        var width = lines.Select(t => t.Length).Max();
        var alignedLines = alignText(lines, alignment, width);

        foreach (var text in alignedLines)
            Console.WriteLine(text);
    }

    static IEnumerable<string> alignText(IEnumerable<string> lines, Alignment alignment, int width)
    {
        switch (alignment)
        {
            case Alignment.LEFT:
                return lines;

            case Alignment.RIGHT:
                return lines.Select(t => new String(' ', width - t.Length) + t);

            case Alignment.CENTER:
                return lines.Select(t => new String(' ', (width - t.Length) / 2) + t);

            case Alignment.JUSTIFY:
                return lines.Select(t =>
                {
                    var words = t.Split();
                    var spaceRequired = width - t.Length;
                    var spacing = spaceRequired / (words.Length - 1) + 1;
                    return string.Join(new String(' ', spacing), words);
                });

            default:
                throw new NotImplementedException();
        }
    }

    enum Alignment
    {
        LEFT,
        RIGHT,
        CENTER,
        JUSTIFY
    }
}