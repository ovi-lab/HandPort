using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool isEnabled = true;
    [SerializeField] private Vector3 angle;
    private Transform cam;

    private void Awake()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (!isEnabled) return;
        transform.LookAt(cam);
        transform.Rotate(angle);
    }
}