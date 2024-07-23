using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class RayInteractorManager : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    private HighlightOnHover currentHighlightedObject = null;

    void Update()
    {
        if (rayInteractor == null) return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            HighlightOnHover highlightable = hit.collider.GetComponent<HighlightOnHover>();

            if (highlightable != null)
            {
                if (currentHighlightedObject != highlightable)
                {
                    if (currentHighlightedObject != null)
                    {
                        currentHighlightedObject.Unhighlight();
                    }
                    currentHighlightedObject = highlightable;
                    currentHighlightedObject.Highlight();
                }
            }
            else if (currentHighlightedObject != null)
            {
                currentHighlightedObject.Unhighlight();
                currentHighlightedObject = null;
            }
        }
        else if (currentHighlightedObject != null)
        {
            currentHighlightedObject.Unhighlight();
            currentHighlightedObject = null;
        }
    }
}