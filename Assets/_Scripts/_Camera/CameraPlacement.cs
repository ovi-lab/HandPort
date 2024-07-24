using UnityEngine;

public class CameraPlacement : MonoBehaviour
{
    [SerializeField] protected float heightOffset;
    [SerializeField] protected float horizontalOffset;
    [SerializeField] protected float cameraLookAngle;
    protected Vector3 targetPosition;
    protected Camera mainCamera;
    
    protected void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        targetPosition = CameraManager.ParabolaPosition;
        PlaceCamera(targetPosition);
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
    // for handviewcamera -> direction of hand, heightoffset = height of hand
    // for rayviewcamera -> direction of ray, heightoffset 
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
