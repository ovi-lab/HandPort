using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LatinSquareManager
{
    public List<(int, int, int)> GenerateAndApplyLatinSquare(int[] set1, int[] set2, int[] set3)
    {
        var combinations = GenerateAllCombinations(set1, set2, set3);
        var latinSquare = GenerateBalancedLatinSquare(combinations.Count);
        return ApplyLatinSquare(combinations, latinSquare);
    }

    private List<(int, int, int)> GenerateAllCombinations(int[] set1, int[] set2, int[] set3)
    {
        var combinations = from value1 in set1
                           from value2 in set2
                           from value3 in set3
                           select (value1, value2, value3);

        return combinations.ToList();
    }

    private List<int[]> GenerateBalancedLatinSquare(int n)
    {
        var latinSquare = new List<int[]>();

        // Generate permutations lazily
        var lazyPermutations = GeneratePermutationsLazy(Enumerable.Range(0, n).ToArray()).Take(n).ToList();

        // Ensure the square is balanced
        foreach (var perm in lazyPermutations)
        {
            if (IsValidLatinSquare(latinSquare, perm, n))
            {
                latinSquare.Add(perm);
            }
            if (latinSquare.Count == n)
            {
                break;
            }
        }

        return latinSquare;
    }

    // Lazy permutation generator
    private IEnumerable<int[]> GeneratePermutationsLazy(int[] arr, int index = 0)
    {
        if (index == arr.Length - 1)
        {
            yield return arr.Clone() as int[];
        }
        else
        {
            for (int i = index; i < arr.Length; i++)
            {
                Swap(ref arr[index], ref arr[i]);
                foreach (var perm in GeneratePermutationsLazy(arr, index + 1))
                {
                    yield return perm;
                }
                Swap(ref arr[index], ref arr[i]);
            }
        }
    }

    private void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    private bool IsValidLatinSquare(List<int[]> latinSquare, int[] candidate, int n)
    {
        for (int i = 0; i < latinSquare.Count; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (latinSquare[i][j] == candidate[j])
                {
                    return false;
                }
            }
        }
        return true;
    }

    private List<(int, int, int)> ApplyLatinSquare(List<(int, int, int)> combinations, List<int[]> latinSquare)
    {
        var shuffledCombinations = new List<(int, int, int)>();

        foreach (var row in latinSquare)
        {
            foreach (var index in row)
            {
                if (index < combinations.Count)
                {
                    shuffledCombinations.Add(combinations[index]);
                }
            }
        }

        return shuffledCombinations;
    }
}
