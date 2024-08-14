using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class AccountForPinchGestureTeleport : MonoBehaviour
{
    
    public TeleportationProvider teleportationProvider;
    public XRRayInteractor rayInteractor;
    public GameManager gameManager;
    
    private CustomActionBasedControllerStable3 rightHandStableController;
    private ActionBasedController rightHandController;
    private InputAction selectAction; 
    private bool rayCastHit = false;
    private Vector3 targetDestination;
    
    public int frames = 200;
    private Queue<(bool hit, RaycastHit hitInfo)> raycastHitQueue = new Queue<(bool, RaycastHit)>();

    void Awake()
    {
        teleportationProvider = FindObjectOfType<TeleportationProvider>();
        gameManager = FindObjectOfType<GameManager>();
        if (SceneManager.GetActiveScene().name == "Baseline")
        {
            ActionBasedController [] controllerArray =
                ActionBasedController.FindObjectsOfType<ActionBasedController>(true);
            foreach (var controller in controllerArray)
            {
                if (controller.name.Equals("Teleport Interactor"))
                {
                    rightHandController = controller;
                }
            }
            selectAction = rightHandController.selectAction.action;
        }
        else
        {
            CustomActionBasedControllerStable3[] controllerArray = CustomActionBasedControllerStable3.FindObjectsOfType<CustomActionBasedControllerStable3>(true);
            foreach (var controller in controllerArray)
            {
                if (controller.name.Equals("Teleport Interactor"))
                {
                    rightHandStableController = controller;
                }
            }
            selectAction = rightHandStableController.selectAction.action;
        }
    }
    void Update()
    {
        if (rayInteractor != null)
        {
            bool rayCastHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo);
            bool hitTeleportationAnchor = rayCastHit && hitInfo.collider.CompareTag("TeleportationAnchor");

            // when successful teleportation -> dont store raycast
            if (selectAction.triggered && hitTeleportationAnchor)
            {
                return;
            }

            // when unsuccessful teleportation -> store anchor position
            if (hitTeleportationAnchor)
            {
            
                GameObject hitObject = hitInfo.collider.gameObject;
                targetDestination = hitObject.transform.position;
                targetDestination.y += hitObject.transform.localScale.y/2;
            }
        
            // when unsuccessful teleportation -> store raycast
            raycastHitQueue.Enqueue((rayCastHit, hitInfo));
            if (raycastHitQueue.Count > frames)
            {
                raycastHitQueue.Dequeue();
            }
            
            // when unsuccessful teleportation -> check if there was prior teleportationanchorhit
            if (!hitTeleportationAnchor && selectAction.triggered)
            {
                CheckTeleportRequest();
            }
        
        }
    }

    private void CheckTeleportRequest()
    {
        bool raycastHitValid = false;
        
        // Iterate through queue to check if anchor was stored
        foreach (var item in raycastHitQueue)
        {
            if (item.hit && item.hitInfo.collider.CompareTag("TeleportationAnchor"))
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
