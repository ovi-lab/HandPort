using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraPlacement : MonoBehaviour
{
    [SerializeField] protected float heightOffset;
    [SerializeField] protected float horizontalOffset;
    [SerializeField] protected float cameraLookAngle;
    [SerializeField] protected XRRayInteractor rayInteractor;
    protected Vector3 targetPosition;
    protected Camera mainCamera;
    [SerializeField] protected Vector3 hitPoint;
    
    protected void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {

        RaycastHit hitInfo;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hitInfo))
        {
            hitPoint = hitInfo.point;
        }
        
        PlaceCamera(hitPoint);
    }

    protected void PlaceCamera(Vector3 target)
    {
        target.y += heightOffset;
        target.z -= horizontalOffset;
        Vector3 cameraLook = target - mainCamera.transform.position;
        transform.forward = cameraLook;
        transform.rotation = Quaternion.Euler(cameraLookAngle,0,0);
        transform.position = target;
    }
    protected void PlaceCamera(Vector3 target, Vector3 forwardDirection)
    {
        forwardDirection = forwardDirection.normalized;
        target.y += heightOffset; // Apply heightOffset
        target.z -= horizontalOffset; // Apply horizontalOffset

        transform.position = target;

        // Ensure the camera looks directly at the forwardDirection
        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
        transform.rotation = targetRotation;
    }
}
