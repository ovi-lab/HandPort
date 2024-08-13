using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LatinSquareManager
{
    // Generates all possible combinations of the provided sets
    private List<(int, int, int)> GenerateAllCombinations(int[] set1, int[] set2, int[] set3)
    {
        var combinations = new List<(int, int, int)>();

        foreach (var item1 in set1)
        {
            foreach (var item2 in set2)
            {
                foreach (var item3 in set3)
                {
                    combinations.Add((item1, item2, item3));
                }
            }
        }

        return combinations;
    }

    // Generates a balanced permutation of the combinations
    private List<(int, int, int)> GenerateBalancedCombinations(List<(int, int, int)> combinations)
    {
        // Ensure the list is shuffled in a way to cover all possible combinations
        var rand = new System.Random();
        return combinations.OrderBy(x => rand.Next()).ToList();
    }

    // Generates and shuffles the combinations
    public List<(int, int, int)> GenerateAndShuffleCombinations(int[] set1, int[] set2, int[] set3)
    {
        // Generate all combinations
        var combinations = GenerateAllCombinations(set1, set2, set3);

        // Log all combinations
        string combinationsLog = "All Combinations:\n";
        combinationsLog = combinations.Aggregate(combinationsLog, (current, combination) =>
            current + $"{combination.Item1}, {combination.Item2}, {combination.Item3}\n");
        Debug.Log(combinationsLog);

        // Generate a balanced permutation of the combinations
        var balancedCombinations = GenerateBalancedCombinations(combinations);

        // Log the balanced combinations
        string balancedCombinationsLog = "Balanced Combinations:\n";
        balancedCombinationsLog = balancedCombinations.Aggregate(balancedCombinationsLog, (current, combination) =>
            current + $"{combination.Item1}, {combination.Item2}, {combination.Item3}\n");
        Debug.Log(balancedCombinationsLog);

        return balancedCombinations;
    }
}
