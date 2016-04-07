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
                solve(possibility, allPositions, guess, bulls, cows)
            ).ToList();
        }

    }

    static IEnumerable<int[][]> solve(int[][] validValues, int[] positionsToSolve, int[] guess, int bulls, int cows)
    {
        if (!positionsToSolve.Any())
        {
            yield return Enumerable.Repeat(new int[0], guess.Length).ToArray();
            yield break;
        }
        else
        {
            if (bulls > 0)
            {
                foreach (var position in positionsToSolve)
                {
                    var valueForPosition = guess[position];
                    if (validValues[position].Contains(valueForPosition))
                    {
                        var otherPositionsToSolve = positionsToSolve.Except(new[] { position }).ToArray();
                        var otherSolutions = solve(validValues, otherPositionsToSolve, guess, bulls - 1, cows);
                        foreach (var otherSolution in otherSolutions)
                        {
                            otherSolution[position] = new[] { valueForPosition };
                            yield return otherSolution;
                        }
                    }
                }
            }

            /*
            While bulls>0
                Lås första och testa alla kombos kvar
                Lås andra och testa alla kombos kvar
                Lås tredje och testa alla kombos kvar
                Lås fjärde och testa alla kombos kvar

            While cows>0
                För varje unikt värde, som INTE är på min plats; lås på detta och testa alla kombos kvar

            If bulls + cows < positionsToSolve.Length, testa alla övriga giltiga värden 
            */


        }
    }

    private static void logSolutions(List<int[][]> validValues)
    {
        var count = validValues.Select(solution => solution.Select(a => a.Length).Aggregate((a, b) => a * b)).Sum();
        Console.Error.WriteLine("{0} possible solutions", count);
    }

}