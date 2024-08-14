using System.Collections;
using System.Collections.Generic;
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
    private bool rayCastHit = false;
    private Vector3 targetDestination;
    
    public int frames = 200;
    private Queue<(bool hit, RaycastHit hitInfo)> raycastHitQueue = new Queue<(bool, RaycastHit)>();

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
        selectAction = rightHandController.selectAction.action;
    }
    void Update()
    {
        if (rayInteractor != null)
        {
            bool rayCastHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo);

            // Add the current raycast hit status and information to the queue
            raycastHitQueue.Enqueue((rayCastHit, hitInfo));

            // Keep the queue size within the frame buffer size
            if (raycastHitQueue.Count > frames)
            {
                raycastHitQueue.Dequeue();
            }
            
            bool hitTeleportationAnchor = rayCastHit && hitInfo.collider.CompareTag("TeleportationAnchor");
            if (hitTeleportationAnchor)
            {
                
                GameObject hitObject = hitInfo.collider.gameObject;
                targetDestination = hitObject.transform.position;
                targetDestination.y += hitObject.transform.localScale.y/2;
            }

            if (!hitTeleportationAnchor && selectAction.triggered)
            {
                CheckTeleportRequest();
            }
        }
    }

    private void CheckTeleportRequest()
    {
        // Variables to track the result of the raycast hits
        bool raycastHitValid = false;
        Vector3 finalDestination = Vector3.zero;
        
        // Iterate through the stored raycast hits in the queue using a traditional for loop
        foreach (var item in raycastHitQueue)
        {
            if (item.hit)
            {
                raycastHitValid = true;
                break; 
            }
        }

        if (raycastHitValid)
        {
            // Create the teleport request
            var teleportRequest = new TeleportRequest
            {
                destinationPosition = targetDestination,
                matchOrientation = MatchOrientation.WorldSpaceUp
            };
            Debug.Log(targetDestination);
            teleportationProvider.QueueTeleportRequest(teleportRequest);
            if (gameManager.GetCurrentTarget < gameManager.GetTargetCount)
            {
                gameManager.EnableNextTarget(new SelectExitEventArgs());   
            }
            if (gameManager.GetCurrentTarget == 0)
            {
                var teleportReset = new TeleportRequest
                {
                    destinationPosition = Vector3.zero,
                    matchOrientation = MatchOrientation.WorldSpaceUp
                };
            
                teleportationProvider.QueueTeleportRequest(teleportReset);
            }
        }
    }
}
