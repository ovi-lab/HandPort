using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public enum CameraType
{
    TopDown = 0,
    MiniMap = 1,
    Inclined = 2
}
public class MiniMapCameraManager : MonoBehaviour
{

    public readonly Dictionary<string, CameraType> stringToEnumConverter = new Dictionary<string, CameraType>()
    {
        { "TopDown", CameraType.TopDown },
        { "MiniMap", CameraType.MiniMap },
        { "Inclined", CameraType.Inclined }

    };

    public static Vector3 ParabolaPosition => parabolaPosition;
    [SerializeField] private CameraType cameraDisplayType;
    [SerializeField] private List<MiniMapCameraPlacement> cameras;
    [SerializeField] private XRRayInteractor[] rayInteractors;
    
    private static Vector3 parabolaPosition;
    private Camera mainCamera;
    private static GameObject currentCameraGameObject;


    private void Start()
    {
        foreach (MiniMapCameraPlacement cam in cameras)
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
        foreach (MiniMapCameraPlacement cam in cameras)
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
