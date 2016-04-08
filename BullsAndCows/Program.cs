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
        const int NUMBER_OF_POSITIONS = 4;
        Console.Error.WriteLine("Using {0} positions", NUMBER_OF_POSITIONS);

        var allPositions = Enumerable.Range(0, NUMBER_OF_POSITIONS).ToArray();

        var validValues = new List<int[][]> {
            Enumerable.Repeat(Enumerable.Range(0, 10).ToArray(), NUMBER_OF_POSITIONS).ToArray()
        };

        logSolutions(validValues);

        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int[] guess = inputs[0].Select(x => int.Parse(x.ToString())).ToArray();
            int bulls = int.Parse(inputs[1]);
            int cows = int.Parse(inputs[2]);
            Console.Error.WriteLine("guess {0} == {1} bulls + {2} cows", inputs[0], bulls, cows);

            validValues = validValues.SelectMany(possibility => 
                solve(possibility, guess, bulls, cows, NUMBER_OF_POSITIONS - 1)
            ).ToList();

            logSolutions(validValues);
        }

        var solution = string.Join("", validValues.Single().Select(x => x.Single()).ToArray());
        Console.WriteLine(solution);
        Console.ReadLine();
    }

    static IEnumerable<int[][]> solve(int[][] validValues, int[] guess, int bulls, int cows, int position)
    {
        if (position < 0)
        {
            yield return validValues;
            yield break;
        }


        var possibleBulls = unspokenGuesses(validValues, guess).Intersect(validValues[position]).ToArray();
        if (bulls > 0)
        {
            var value = guess[position];
            if (possibleBulls.Contains(value))
            {
                var solutionValues = keep(validValues, position, value);
                var solutions = solve(solutionValues, guess, bulls - 1, cows, position - 1).ToArray();
                foreach (var solution in solutions)
                    yield return solution;
            }
        }

        validValues = remove(validValues, position, new[] { guess[position] });

        var possibleCows = unspokenGuesses(validValues, guess).Intersect(validValues[position]);

        if (cows > 0)
        {
            foreach (var value in possibleCows)
            {
                var solutionValues = keep(validValues, position, value);
                var solutions = solve(solutionValues, guess, bulls, cows - 1, position - 1).ToArray();
                foreach (var solution in solutions)
                    yield return solution;
            }
        }

        validValues = remove(validValues, position, possibleCows);

        if (bulls + cows < position + 1)
        {
            var solutions = solve(validValues, guess, bulls, cows, position - 1).ToArray();
            foreach (var solution in solutions)
                yield return solution;
        }
    }

    private static int[] unspokenGuesses(int[][] validValues, int[] guess)
    {
        var guessesToTestAsCows = guess.Select(a => a).ToArray();
        foreach (var value in validValues.Where(a => a.Length == 1).Select(a => a.Single()))
        {
            var index = guessesToTestAsCows.ToList().IndexOf(value);
            if (index >= 0)
            {
                guessesToTestAsCows = guessesToTestAsCows.Take(index).Concat(guessesToTestAsCows.Skip(index + 1)).ToArray();
            }
        }

        return guessesToTestAsCows;
    }

    #region Array manipulation

    private static int[][] keep(int[][] validValues, int position, int value)
    {
        return keep(validValues, position, new[] { value });
    }

    private static int[][] keep(int[][] validValues, int position, IEnumerable<int> values)
    {
        validValues = clone(validValues);
        validValues[position] = validValues[position].Intersect(values).ToArray();
        return validValues;
    }

    private static int[][] remove(int[][] validValues, int position, IEnumerable<int> values)
    {
        validValues = clone(validValues);
        validValues[position] = validValues[position].Except(values).ToArray();
        return validValues;
    }

    private static int[][] clone(int[][] validValues)
    {
        validValues = validValues.Select(a => a).ToArray();
        return validValues;
    }

    #endregion Array manipulation

    private static void logSolutions(List<int[][]> validValues)
    {
        var count = validValues.Select(solution => solution.Select(a => a.Length).Aggregate((a, b) => a * b)).Sum();
        Console.Error.WriteLine("{0} possible solutions", count);
        if (count == 1)
            Console.Error.WriteLine(string.Join("", validValues.SelectMany(solution => solution.Select(a => a.Single())).ToArray()));
    }

}