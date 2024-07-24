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

public enum Resolution
{
    Res43 = 0,
    Res169 = 1
}

public enum CameraAnchor
{
    HMDAnchor = 0,
    HandAnchor = 1,
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
    public readonly Dictionary<string, Resolution> stringToEnumConverter1 = new Dictionary<string, Resolution>()
    {
        { "4:3", Resolution.Res43 },
        { "16:9", Resolution.Res169 }

    };
    public readonly Dictionary<string, CameraAnchor> stringToEnumConverter2 = new Dictionary<string, CameraAnchor>()
    {
        { "HMDAnchor", CameraAnchor.HMDAnchor },
        { "HandAnchor", CameraAnchor.HandAnchor },

    };
    

    public static Vector3 ParabolaPosition => parabolaPosition;
    [SerializeField] private CameraType cameraDisplayType;
    [SerializeField] public Resolution resolution;
    [SerializeField] private CameraAnchor anchor;
    [SerializeField] private List<CameraPlacement> cameras;
    [SerializeField] private XRRayInteractor[] rayInteractors;

    [SerializeField] private RenderTexture renderTexture43;
    [SerializeField] private RenderTexture renderTexture169;

    
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

        if ((int)resolution == 0)
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture43;
            }
        }
        else
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture169;
            }
        }
        
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
        
        if ((int)resolution == 0)
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture43;
            }
        }
        else
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture169;
            }
        }
    }
}
