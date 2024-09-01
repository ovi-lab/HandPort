using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class HighlightOnHover : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public Color highlightColor;
    private bool isHighlighted = false;
    private TeleportationAnchor teleAnchor;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }
    
    public void Highlight()
    {
        if (objectRenderer != null && !isHighlighted)
        {
            objectRenderer.material.color = highlightColor;
            isHighlighted = true;
        }
    }

    public void Unhighlight()
    {
        if (objectRenderer != null && isHighlighted)
        {
            objectRenderer.material.color = originalColor;
            isHighlighted = false;
        }
    }
    
}