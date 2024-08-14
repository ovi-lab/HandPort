using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayVariantText : MonoBehaviour
{
    public TextMeshProUGUI permutationText;

    private void Awake()
    {
        if (permutationText == null)
        {
            permutationText = GameObject.Find("VariantText").GetComponent<TextMeshProUGUI>();
        }
    }

    public void DisplayEndText()
    {
        string textToShow = "Thank You!";
        StartCoroutine(DisplayTextForDuration(textToShow, 10f));
    }
    
    public void DisplayVariant((int, int, int) permutation)
    {
        string textToShow = $"Camera Type: {(CameraType)permutation.Item1}\nAnchor: {(CameraAnchor)permutation.Item2}\nMapping Function: {(GoGoAlgorithm)permutation.Item3}";
        StartCoroutine(DisplayTextForDuration(textToShow, 8f));
    }
    
    private IEnumerator DisplayTextForDuration(string text, float duration)
    {
        permutationText.text = text;
        yield return new WaitForSeconds(duration);
        permutationText.text = string.Empty;
    }
    
}