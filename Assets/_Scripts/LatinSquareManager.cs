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
    
    private List<(int, float)> GenerateAllCombinations(int[] set1, float[] set2)
    {
        var combinations = new List<(int, float)>();

        foreach (var item1 in set1)
        {
            foreach (var item2 in set2)
            {
                combinations.Add((item1, item2));
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
    
    private List<(int, float)> GenerateBalancedCombinations(List<(int, float)> combinations)
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
        //Debug.Log(combinationsLog);

        // Generate a balanced permutation of the combinations
        var balancedCombinations = GenerateBalancedCombinations(combinations);

        // Log the balanced combinations
        string balancedCombinationsLog = "Balanced Combinations:\n";
        balancedCombinationsLog = balancedCombinations.Aggregate(balancedCombinationsLog, (current, combination) =>
            current + $"{combination.Item1}, {combination.Item2}, {combination.Item3}\n");
        //Debug.Log(balancedCombinationsLog);

        return balancedCombinations;
    }
    
    public List<(int, float)> GenerateAndShuffleCombinations(int[] set1, float[] set2, int repetition)
    {
        // Generate all combinations
        var combinations = GenerateAllCombinations(set1, set2);

        // Create a list to hold repeated combinations
        var repeatedCombinations = new List<(int, float)>();

        // Repeat each combination the specified number of times
        foreach (var combo in combinations)
        {
            for (int i = 0; i < repetition; i++)
            {
                repeatedCombinations.Add(combo);
            }
        }

        // Shuffle the combinations
        var rand = new System.Random();
        var balancedCombinations = repeatedCombinations.OrderBy(x => rand.Next()).ToList();

        // Log all combinations
        string combinationsLog = "All Combinations:\n";
        combinationsLog = combinations.Aggregate(combinationsLog, (current, combination) =>
            current + $"{combination.Item1}, {combination.Item2}\n");

        // Log the balanced combinations
        string balancedCombinationsLog = "Balanced Combinations:\n";
        balancedCombinationsLog = balancedCombinations.Aggregate(balancedCombinationsLog, (current, combination) =>
            current + $"{combination.Item1}, {combination.Item2}\n");
        //Debug.Log(balancedCombinationsLog);

        return balancedCombinations;
    }
}
