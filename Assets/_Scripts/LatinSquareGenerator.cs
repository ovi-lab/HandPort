using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatinSquareGenerator : MonoBehaviour
{
    public int numConditions;

    private void Start()
    {
        int[,] latinSquare = GenerateLatinSquare(numConditions);
        PrintLatinSquare(latinSquare);
    }

    private int[,] GenerateLatinSquare(int n)
    {
        int[,] square = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                square[i, j] = (i + j) % n;
            }
        }

        return square;
    }

    private void PrintLatinSquare(int[,] square)
    {
        int n = square.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            string row = "";
            for (int j = 0; j < n; j++)
            {
                row += square[i, j] + " ";
            }
            Debug.Log(row);
        }
    }
}
