using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class AccountForPinchGestureTeleport : MonoBehaviour
{
    
    public TeleportationProvider teleportationProvider;
    public XRRayInteractor rayInteractor;
    public GameManager gameManager;
    
    private CustomActionBasedControllerStable3 rightHandController;
    private InputAction selectAction; 
    private bool requestQueued = false;
    private bool rayCastHit = false;
    
    public int frames = 50;
    void Awake()
    {
        teleportationProvider = FindObjectOfType<TeleportationProvider>();
        gameManager = FindObjectOfType<GameManager>();
        CustomActionBasedControllerStable3[] controllerArray = CustomActionBasedControllerStable3.FindObjectsOfType<CustomActionBasedControllerStable3>(true);
        
        foreach (var controller in controllerArray)
        {
            if (controller.name.Equals("Teleport Interactor"))
            {
                rightHandController = controller;
            }
        }
    }
    void Update()
    {
        if (rayInteractor != null)
        {
            bool rayCastHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo);
            if (rayCastHit && !hitInfo.collider.CompareTag("TeleportationAnchor"))
            {
                selectAction = rightHandController.selectAction.action;
                if (selectAction.triggered && !requestQueued )
                {
                    StartCoroutine(CheckTeleportRequest());
                } 
            }
        }
    }

    private IEnumerator CheckTeleportRequest()
    {
        requestQueued = true; 
        // Variables to track the result of the raycast hits
        bool raycastHitValid = false;
        Vector3 finalDestination = Vector3.zero;
        
        for (int i = 0; i < frames; i++)
        {
            yield return null;
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo))
            {
                // Check if the hit object is a teleportation anchor
                if (hitInfo.collider.CompareTag("TeleportationAnchor"))
                {
                    raycastHitValid = true;
                    finalDestination = hitInfo.point;
                    break; 
                }
            }
        }

        if (raycastHitValid)
        {
            // Create the teleport request
            var teleportRequest = new TeleportRequest
            {
                destinationPosition = finalDestination,
                matchOrientation = MatchOrientation.WorldSpaceUp
            };
            
            teleportationProvider.QueueTeleportRequest(teleportRequest);
            requestQueued = false; 
            
            gameManager.EnableNextTarget(new SelectExitEventArgs());
            if (gameManager.GetCurrentTarget == 0)
            {
                var teleportReset = new TeleportRequest
                {
                    destinationPosition = Vector3.zero,
                    matchOrientation = MatchOrientation.WorldSpaceUp
                };
            
                teleportationProvider.QueueTeleportRequest(teleportReset);
                requestQueued = false; 
            }
        }
        else
        {
            requestQueued = false;
        }
    }
}
