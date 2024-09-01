using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public enum CameraType
{
    TopDownRay = 0,
    PerspectiveRay = 1,
    PerspectiveHand = 2
}

public enum Resolution
{
    Res43 = 0,
    Res169 = 1
}

public enum CameraAnchor
{
    HMD = 0,
    Hand = 1,
}
public class CameraManager : MonoBehaviour
{
    public readonly Dictionary<string, CameraType> stringToEnumConverter = new Dictionary<string, CameraType>()
    {
        { "TopDownRay", CameraType.TopDownRay },
        { "RayView", CameraType.PerspectiveRay },
        { "HandView", CameraType.PerspectiveHand }
    };
    public readonly Dictionary<string, Resolution> stringToEnumConverter1 = new Dictionary<string, Resolution>()
    {
        { "4:3", Resolution.Res43 },
        { "16:9", Resolution.Res169 }
    };
    public readonly Dictionary<string, CameraAnchor> stringToEnumConverter2 = new Dictionary<string, CameraAnchor>()
    {
        { "HMDAnchor", CameraAnchor.HMD },
        { "HandAnchor", CameraAnchor.Hand },

    };
    public static Vector3 ParabolaPosition => parabolaPosition;
    [SerializeField] public CameraType cameraDisplayType;
    [SerializeField] public Resolution resolution;
    [SerializeField] public CameraAnchor anchor;
    [SerializeField] private List<CameraPlacement> cameras;
    [SerializeField] private XRRayInteractor[] rayInteractors;

    [SerializeField] private RenderTexture renderTexture43;
    [SerializeField] private RenderTexture renderTexture169;
    
    private static Vector3 parabolaPosition;
    private Camera mainCamera;
    private static GameObject currentCameraGameObject;
    private WIMWidget wimWidget; 
    private bool wimInitialized = false;
    
    private void Start()
    {
        // choose camera
        foreach (CameraPlacement cam in cameras)
        { 
            cam.gameObject.SetActive(false);
        }
        cameras[(int)cameraDisplayType].gameObject.SetActive(true);
        
        // choose resolution texture
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
        
        // choose anchor
        InitializeWIMWidget();
        wimWidget.anchorType = ((int)anchor);
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
        UpdateCameraTypeAndAnchor();
            
        // change resolution texture
        if (resolution == Resolution.Res43)
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture43;
            }
        } else if (resolution == Resolution.Res169) 
        {
            foreach (CameraPlacement cam in cameras)
            {
                Camera cameraComponent = cam.GetComponent<Camera>();
                cameraComponent.targetTexture = renderTexture169;
            }
        }
    }

    private void InitializeWIMWidget()
    {
        wimWidget = FindObjectOfType<WIMWidget>();
        wimInitialized = true;
    }

    public void UpdateCameraTypeAndAnchor()
    {
        // change camera
        foreach (var cam in cameras)
        { 
            cam.gameObject.SetActive(false);
        }
        cameras[(int)cameraDisplayType].gameObject.SetActive(true);
        
        // change resolution plane & anchor
        if (!wimInitialized) return;
        wimWidget.display.SetParent(wimWidget.transform);
        wimWidget.anchorType = ((int)anchor);
        wimWidget.InitializeDisplay();
    }
}
