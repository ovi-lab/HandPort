using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public enum CameraType
{
    TopDownTarget = 0,
    TopDownRay = 1,
    RayView = 2,
    HandView = 3
}
public class CameraManager : MonoBehaviour
{

    public readonly Dictionary<string, CameraType> stringToEnumConverter = new Dictionary<string, CameraType>()
    {
        { "TopDownTarget", CameraType.TopDownTarget },
        { "TopDownRay", CameraType.TopDownRay },
        { "RayView", CameraType.RayView },
        { "HandView", CameraType.HandView }

    };

    public static Vector3 ParabolaPosition => parabolaPosition;
    [SerializeField] private CameraType cameraDisplayType;
    [SerializeField] private List<CameraPlacement> cameras;
    [SerializeField] private XRRayInteractor[] rayInteractors;
    
    private static Vector3 parabolaPosition;
    private Camera mainCamera;
    private static GameObject currentCameraGameObject;


    private void Start()
    {
        foreach (CameraPlacement cam in cameras)
        { 
            cam.gameObject.SetActive(false);
        }
        cameras[(int)cameraDisplayType].gameObject.SetActive(true);
        rayInteractors = FindObjectsOfType<XRRayInteractor>();
    }

    private void Update()
    {
        parabolaPosition = GetCurrentPoint();
    }

    public void ChangeCamera(string minimapCameraType)
    {
        cameraDisplayType = stringToEnumConverter[minimapCameraType];
        foreach (CameraPlacement cam in cameras)
        { 
            cam.gameObject.SetActive(false);
        }
        cameras[(int)cameraDisplayType].gameObject.SetActive(true);
    }
    
    private Vector3 GetCurrentPoint()
    {
        foreach (var ray in rayInteractors)
        {
            if(ray.TryGetCurrent3DRaycastHit(out RaycastHit raycastHitPoint))
            {
                return raycastHitPoint.point;
            }
        }
        return Vector3.zero;
    }
    private void OnValidate()
    {
        foreach (var cam in cameras)
        { 
            cam.gameObject.SetActive(false);
        }
        cameras[(int)cameraDisplayType].gameObject.SetActive(true);
    }
}
