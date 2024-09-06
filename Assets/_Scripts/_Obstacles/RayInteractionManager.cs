using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayInteractorManager : MonoBehaviour
{
    public XRRayInteractor rayInteractor1; // First XRRayInteractor
    public XRRayInteractor rayInteractor2; // Second XRRayInteractor
    private HighlightOnHover currentHighlightedObject;

    void Update()
    {
        // Variables to store the hit results for each ray interactor
        RaycastHit hit1;
        RaycastHit hit2;
        bool hitDetected = false;
        HighlightOnHover newHighlightedObject = null;
        XRRayInteractor activeRayInteractor = null;

        // Check for the first ray interactor
        if (rayInteractor1 != null && rayInteractor1.TryGetCurrent3DRaycastHit(out hit1))
        {
            hitDetected = true;
            HighlightOnHover highlightable = hit1.collider.GetComponent<HighlightOnHover>();
            Vector3 hitNormal = hit1.normal;
            if (highlightable != null)
            {
                newHighlightedObject = highlightable;
                activeRayInteractor = rayInteractor1;
                if (Vector3.Angle(Vector3.up, hitNormal) > 20)
                {
                    Debug.LogWarning($"{Vector3.Angle(Vector3.up, hitNormal)}");
                    return;
                }
            }
        }

        // Check for the second ray interactor
        if (rayInteractor2 != null && rayInteractor2.TryGetCurrent3DRaycastHit(out hit2))
        {
            hitDetected = true;
            HighlightOnHover highlightable = hit2.collider.GetComponent<HighlightOnHover>();
            Vector3 hitNormal = hit2.normal;

            if (highlightable != null)
            {
                if (Vector3.Angle(Vector3.up, hitNormal) > 20)
                {
                    Debug.LogWarning($"{Vector3.Angle(Vector3.up, hitNormal)}");
                    return;
                }
                // Use the second ray interactor's hit if it is the most recent or the same as the first one
                if (newHighlightedObject == null || activeRayInteractor == rayInteractor1)
                {
                    newHighlightedObject = highlightable;
                    activeRayInteractor = rayInteractor2;
                }
            }
        }


        // Manage highlighting based on the new highlighted object
        if (newHighlightedObject != null)
        {
            if (currentHighlightedObject != newHighlightedObject)
            {
                if (currentHighlightedObject != null)
                {
                    currentHighlightedObject.Unhighlight();
                }
                currentHighlightedObject = newHighlightedObject;
                currentHighlightedObject.Highlight();
            }
        }
        else if (currentHighlightedObject != null)
        {
            currentHighlightedObject.Unhighlight();
            currentHighlightedObject = null;
        }
    }
}
