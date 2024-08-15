
using UnityEngine;

public class WIMWidget : MonoBehaviour
{
    [SerializeField] public int anchorType = 0;
    [SerializeField] public Transform[] anchorPoint;

    public Transform display;
    [SerializeField] public Transform[] planes;
    private bool isInitialized = false;
    private CameraManager camManager;

    private void Start()
    {
        InitializeDisplay();
        isInitialized = true;
    }

    private void OnValidate()
    {
        if (!isInitialized) return;
        InitializeDisplay();
    }

    public void InitializeDisplay()
    {
        foreach (Transform child  in transform)
        { 
            child.gameObject.SetActive(false);
        }
        camManager = transform.parent.GetComponent<CameraManager>();
        if ((int)camManager.resolution < planes.Length)
        {
            display = planes[(int)camManager.resolution];
            display.gameObject.SetActive(true);   
            display.transform.rotation = Quaternion.Euler(90, -90, 90);
            if (anchorType == (int)CameraAnchor.HMD)
            {
                display.transform.localPosition = new Vector3(0f,0.2f,0.5f);
            }
            else if(anchorType == (int)CameraAnchor.Hand)
            {
                display.transform.localPosition = new Vector3(0, -0.08f, 0.25f);
            }
        }
        if (anchorPoint.Length > 0)
        {
            SetDisplayParent(anchorPoint[anchorType]);
        }
    }

    public void SetDisplayParent(Transform newParent)
    {
        if (display != null)
        {
            display.SetParent(newParent, false);
        }
        else
        {
            Debug.LogWarning("Display is not set.");
        }
    }
}