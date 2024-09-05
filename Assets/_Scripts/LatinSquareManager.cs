
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LatinSquareManager
{
    // public List<List<(int, int, int)>> GenerateCombinations(int[] cameraTypes, int[] panelAnchors,
    //     int[] mappingFunctions)
    // {
    //     var patterns = GeneratePattern();
    //     var extendedPatterns = new List<List<(int, int, int)>>();
    //     var rand = new System.Random();
    //
    //     // Generate extended patterns
    //     foreach (var row in patterns)
    //     {
    //         var extendedRow = new List<(int, int, int)>();
    //         var mappingFunctionList = mappingFunctions.ToList();
    //
    //         foreach (var pair in row)
    //         {
    //             var shuffledMappingFunctions = mappingFunctionList.OrderBy(x => rand.Next()).ToArray();
    //             foreach (var mappingFunction in shuffledMappingFunctions)
    //             {
    //                 extendedRow.Add((pair.Item1, pair.Item2, mappingFunction));
    //             }
    //         }
    //
    //         extendedPatterns.Add(extendedRow);
    //     }
    //
    //     // Debugging output
    //     foreach (var row in extendedPatterns)
    //     {
    //         string rowString = string.Join("\t", row.Select(t => $"({t.Item1}, {t.Item2}, {t.Item3})"));
    //         Debug.Log(rowString);
    //     }
    //
    //     return extendedPatterns;
    // }

    public List<List<(int, int, int)>> GenerateCombinations(int[] cameraTypes, int[] panelAnchors,
        int[] mappingFunctions)
    {
        var patterns = GeneratePattern();
        var extendedPatterns = new List<List<(int, int, int)>>();
        var rand = new System.Random();

        // Generate extended patterns
        foreach (var row in patterns)
        {
            var extendedRow = new List<(int, int, int)>();
            var mappingFunctionList = mappingFunctions.ToList();

            foreach (var pair in row)
            {
                var shuffledMappingFunctions = mappingFunctionList.OrderBy(x => rand.Next()).ToArray();
                extendedRow.Add((pair.Item1, pair.Item2, 3));
            }

            extendedPatterns.Add(extendedRow);
        }

        // Debugging output
        foreach (var row in extendedPatterns)
        {
            string rowString = string.Join("\t", row.Select(t => $"({t.Item1}, {t.Item2}, {t.Item3})"));
            Debug.Log(rowString);
        }

        return extendedPatterns;
    }

    public List<List<int>> GenerateMappingCombinations(
        int[] mappingFunctions)
    {
        var pattern = GenerateMappingPattern();
        foreach (var row in pattern)
        {
            // Convert the row into a string representation, formatting each integer.
            string rowString = string.Join("\t", row.Select(num => num.ToString()));
            Debug.Log(rowString);
        }

        return pattern;
    }

    private List<List<(int, int)>> GeneratePattern()
    {
        var pattern = new List<List<(int, int)>>
        {
            new List<(int, int)>
            {
                (0,0),(1,0),(2,0),(0,1),(1,1),(2,1)
            },
            new List<(int, int)>
            {
                (0,1),(2,1),(1,1),(0,0),(2,0),(1,0)

            },
            new List<(int, int)>
            {
                (2,0),(0,0),(1,0),(2,1),(0,1),(1,1)

            },
            new List<(int, int)>
            {
                (2,1),(1,1),(0,1),(2,0),(1,0),(0,0)
            },
            new List<(int, int)>
            {
                (1,0),(2,0),(0,0),(1,1),(2,1),(0,1)
            },
            new List<(int, int)>
            {
                (1,1),(0,1),(2,1),(1,0),(0,0),(2,0)
            }
        };

        return pattern;
    }

    private List<List<int>> GenerateMappingPattern()
    {
        // Initialize a list of lists with integer elements
        var pattern = new List<List<int>>
        {
            // -1 = BaseLine, 0 = Power, 1 = Sigmoid,  2 = Root, 3 = Linear
            new List<int> { 0, 1, 3, -1, 2 },
            new List<int> { 3, 2, 0, -1, 1 },
            new List<int> { -1, 2, 1, 3, 0 },
            new List<int> { 1, 0, -1, 3, 2 },
            new List<int> { 3, 0, 2, 1, -1 },
            new List<int> { 2, -1, 3, 1, 0 },
            new List<int> { 1, -1, 0, 2, 3 },
            new List<int> { 0, 3, 1, 2, -1 },
            new List<int> { 2, 3, -1, 0, 1 },
            new List<int> { -1, 1, 2, 0, 3 }
        };

        return pattern; // Return the generated list of lists
    }


    // private List<List<(int, int)>> GeneratePattern()
    // {
    //     var pattern = new List<List<(int, int)>>
    //     {
    //         new List<(int, int)>
    //         {
    //             (0, 0), (0, 1), (1, 0), (1, 1), (2, 0), (2, 1)
    //         },
    //         new List<(int, int)>
    //         {
    //             (0, 1), (0, 0), (2, 1), (2, 0), (1, 1), (1, 0)
    //         },
    //         new List<(int, int)>
    //         {
    //             (1, 0), (1, 1), (0, 0), (0, 1), (2, 0), (2, 1)
    //         },
    //         new List<(int, int)>
    //         {
    //             (1, 1), (1, 0), (2, 1), (2, 0), (0, 1), (0, 0)
    //         },
    //         new List<(int, int)>
    //         {
    //             (2, 0), (2, 1), (0, 0), (0, 1), (1, 0), (1, 1)
    //         },
    //         new List<(int, int)>
    //         {
    //             (2, 1), (2, 0), (1, 1), (1, 0), (0, 1), (0, 0)
    //         }
    //     };
    //
    //     return pattern;
    // }

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
