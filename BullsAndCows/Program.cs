﻿using System;
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
        const int NUMBER_OF_POSITIONS = 2;
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
            yield return Enumerable.Repeat(new int[0], guess.Length).ToArray();
            yield break;
        }


        if (bulls > 0)
        {
            var valueForPosition = new[] { guess[position] };
            if (validValues[position].Contains(valueForPosition.Single()))
            {
                var solutions = solve(validValues, guess, bulls - 1, cows, position - 1).ToArray();
                foreach (var solution in solutions)
                {
                    solution[position] = valueForPosition;
                    yield return solution;
                }
            }
        }

        validValues = remove(validValues, position, new[] { guess[position] });

        var unspokenGuesses = guess.Distinct();

        if (cows > 0)
        {
            var possibilities = unspokenGuesses.Intersect(validValues[position]);
            //TODO: Need to strip out guess-values already used as bulls and cows
            foreach (var possibility in possibilities)
            {
                var valueForPosition = new[] { possibility };
                if (validValues[position].Contains(valueForPosition.Single()))
                {
                    var solutions = solve(validValues, guess, bulls, cows - 1, position - 1).ToArray();
                    foreach (var solution in solutions)
                    {
                        solution[position] = valueForPosition;
                        yield return solution;
                    }
                }
            }
        }

        validValues = remove(validValues, position, unspokenGuesses);

        if (bulls + cows < position + 1)
        {
            var solutions = solve(validValues, guess, bulls, cows, position - 1).ToArray();
            foreach (var solution in solutions)
            {
                solution[position] = validValues[position];
                yield return solution;
            }
        }
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

    private static void logSolutions(List<int[][]> validValues)
    {
        var count = validValues.Select(solution => solution.Select(a => a.Length).Aggregate((a, b) => a * b)).Sum();
        Console.Error.WriteLine("{0} possible solutions", count);
    }

}