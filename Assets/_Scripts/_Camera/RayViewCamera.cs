using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using Unity.Mathematics;

public class RayViewCamera : CameraPlacement
{
    public XRRayInteractor rayInteractor;
    private Vector3 hitPoint;
    private LineRenderer lineRenderer;
    
    private float velocity;
    private float acceleration;
    private float additionalGroundHeight;
    private float additionalFlightTime;

    private Vector3 pointAt80Percent;
    
    
    void Start()
    {
        lineRenderer = rayInteractor.GetComponent<LineRenderer>();
        if (rayInteractor != null)
        {
            velocity = rayInteractor.velocity;
            acceleration = rayInteractor.acceleration;
            additionalGroundHeight = rayInteractor.additionalGroundHeight;
            additionalFlightTime = rayInteractor.additionalFlightTime;
        }
    }

    void Update()
    {
        if (rayInteractor == null)
        {
            Debug.LogError("XRRayInteractor is not assigned.");
            return;
        }
        
        // Get the current raycast hit
        RaycastHit hitInfo;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hitInfo))
        {
            // Use hitInfo.point for the hit point and hitInfo.normal for the surface normal
            hitPoint = hitInfo.point;
        }
        if (lineRenderer != null)
        {
            Vector3[] points = GetLineRendererPoints();

            if (points.Length > 0)
            {
                pointAt80Percent = GetPointAtPercent(points, 0.7f);
                pointAt80Percent.y += 4;
                PlaceCamera(pointAt80Percent, hitPoint - pointAt80Percent);
            }
        }
    }
    Vector3 GetPointAtPercent(Vector3[] points, float percent)
    {
        if (points.Length == 0)
        {
            return Vector3.zero;
        }
        float totalLength = CalculateTotalLength(points);
        float targetDistance = totalLength * percent;
        return GetPointAtDistance(points, targetDistance);
    }
    
    float CalculateTotalLength(Vector3[] points)
    {
        float totalLength = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            totalLength += Vector3.Distance(points[i], points[i + 1]);
        }
        return totalLength;
    }

    Vector3 GetPointAtDistance(Vector3[] points, float distance)
    {
        float accumulatedDistance = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            float segmentDistance = Vector3.Distance(points[i], points[i + 1]);
            if (accumulatedDistance + segmentDistance >= distance)
            {
                float remainingDistance = distance - accumulatedDistance;
                float t = remainingDistance / segmentDistance;
                return Vector3.Lerp(points[i], points[i + 1], t);
            }
            accumulatedDistance += segmentDistance;
        }
        return points[points.Length - 1];
    }
    
    Vector3[] GetLineRendererPoints()
    {
        int count = lineRenderer.positionCount;
        Vector3[] points = new Vector3[count];
        lineRenderer.GetPositions(points);
        return points;
    }
}