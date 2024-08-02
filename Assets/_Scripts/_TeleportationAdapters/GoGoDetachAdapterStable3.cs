using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public enum GoGoAlgorithm
{
    FastForLowDistance = 0,
    FastForMedDistance = 1,
    FastForHighDistance = 2,
}
public class GoGoDetachAdapterStable3 : MonoBehaviour
{
    public GoGoAlgorithm goGoAlgorithm;
    
    private XRHandSubsystem m_HandSubsystem;
    public Transform xrOrigin;
    private  float minVirtDistance = 0f; 
    private float maxVirtDistance;

    public float normalizedDeltaForward = 0f;
    private float minDistance;
    private float maxDistance;
    
    private float originShoulderDistance;
    private float ellbowWristDistance;
    private float shoulderEllbowDistance;
    
    private float p = 2.0f;
    public float sigmoidGradient = 8f;
    public Transform rightHand;
    
    private OneEuroFilter positionFilter;
    public Transform rightHandScaleAnchor;
    public Vector3 shoulderToWristDirection;
    
    void Start()
    {
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        minDistance = originShoulderDistance + ellbowWristDistance;
        maxDistance = originShoulderDistance + ellbowWristDistance + shoulderEllbowDistance;

        for (int i = 0; i < handSubsystems.Count; ++i)
        {
            var handSubsystem = handSubsystems[i];
            if (handSubsystem.running)
            {
                m_HandSubsystem = handSubsystem;
                break;
            }
        }

        if (m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands += OnUpdatedHands;
        }
        else
        {
            Debug.LogWarning("No running XRHandSubsystem found.");
        }
        positionFilter = new OneEuroFilter(minCutoff: 0.2f, beta: 0.02f, dCutoff: 0.7f, initialDt: Time.deltaTime);
    }
    
    public void SetInitialAdapterValues(float oSD, float sED, float eWD, float mVD)
    {
        originShoulderDistance = oSD;
        ellbowWristDistance = sED;
        shoulderEllbowDistance = eWD;
        maxVirtDistance = mVD;
    }

    void OnUpdatedHands(XRHandSubsystem subsystem,
        XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
        XRHandSubsystem.UpdateType updateType)
    {
        switch (updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
                ScaleUpWristPos();
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
                break;
        }
    }
    
    private void ScaleUpWristPos()
    {
        if (m_HandSubsystem.rightHand.isTracked)
        {
            var wristJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.Wrist);
            if (wristJoint.TryGetPose(out Pose wristPose))
            {
                Vector3 worldWristPosition = xrOrigin.transform.TransformPoint(wristPose.position);
                worldWristPosition.y = 0;
                
                // Calculate shoulderToWristDirection
                Vector3 XZOriginPos = new Vector3(xrOrigin.position.x, 0f, xrOrigin.position.z);
                shoulderToWristDirection = worldWristPosition - (XZOriginPos + Camera.main.transform.right * originShoulderDistance);
                shoulderToWristDirection.y = 0;
                shoulderToWristDirection.Normalize();
            
                // Calculate the direction from the adjusted xrOrigin to the wrist
                Vector3 xrToWristDirection = worldWristPosition - XZOriginPos;
                xrToWristDirection.y = 0;

                // Project xrToWristDirection onto shoulderToWristDirection
                float deltaForward = Vector3.Dot(xrToWristDirection, shoulderToWristDirection);
                float clampedDeltaForward = Mathf.Clamp(deltaForward, minDistance, maxDistance);

                // Normalize DeltaForward
                normalizedDeltaForward = Mathf.InverseLerp(minDistance, maxDistance, clampedDeltaForward);
                normalizedDeltaForward = Mathf.Clamp(normalizedDeltaForward, 0f, 1f);
                
                if (normalizedDeltaForward > 0)
                {
                    // Project NormalizedDeltaForward onto virtualDistance 
                    float virtualDistance = CalculateVirtDistance();
                    
                    // Calculate & apply new position to hand
                    Vector3 newPosition = worldWristPosition + shoulderToWristDirection  * virtualDistance;
                    newPosition.y = xrOrigin.transform.position.y; 
                    rightHand.transform.position = positionFilter.FilterPosition(newPosition);

                    // // Adjust filter parameters based on virtual distance
                    // float sD = Mathf.Round(scaledDistance * 10f) / 10f;
                    // float minCutoff = Mathf.Lerp(0.2f, 0.05f, sD); // More stable further away
                    // float beta = Mathf.Lerp(0.05f, 0.01f, sD); // Less reactive further away
                    // float dCutoff = Mathf.Lerp(1.0f, 0.7f, sD);
                    //
                    // positionFilter.minCutoff = minCutoff;
                    // positionFilter.beta = beta;
                    // positionFilter.dCutoff = dCutoff;
                    
                    
                    // Scale hand visualisation
                    float scaleFactor = 1+ virtualDistance / 10;
                    rightHandScaleAnchor.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
                }
                else
                {
                    // Reset position of hand to original position
                    Vector3 resetPosition = xrOrigin.position;
                    resetPosition = positionFilter.FilterPosition(resetPosition);
                    rightHand.transform.position = resetPosition;
                    
                    // Reset scale of hand
                    rightHandScaleAnchor.transform.localScale = Vector3.one;
                }
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }


    private float CalculateVirtDistance()
    {
        switch (goGoAlgorithm)
        {
            case GoGoAlgorithm.FastForLowDistance:
                // SIGMOID - GRADIENT 2 SUITABLE
                 return Mathf.Lerp(minVirtDistance, maxVirtDistance, (1-1/(1+sigmoidGradient*normalizedDeltaForward))); 
                
                // EXPONENTIAL FUNCTION
                //return Mathf.Lerp(minVirtDistance, maxVirtDistance, 1 - Mathf.Exp(-sigmoidGradient * normalizedDeltaForward));
            
            case GoGoAlgorithm.FastForMedDistance:
                // SIGMOID - GRADIENT 8 SUITABLE
                float sigmoidInput = (normalizedDeltaForward - 0.5f) * sigmoidGradient;
                float sigmoidValue = 1f / (1f + Mathf.Exp(-sigmoidInput));
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, sigmoidValue);
                
                // TANH
                // float tanhInput = (normalizedDeltaForward - 0.5f) * sigmoidGradient;
                // float normalizedValue = ((float)Math.Tanh(tanhInput) + 1f) / 2f;
                // return Mathf.Lerp(minVirtDistance, maxVirtDistance, normalizedValue);
            
            case GoGoAlgorithm.FastForHighDistance:
                // QUADRATIC
                float quadraticValue = Mathf.Pow(normalizedDeltaForward, p);
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, quadraticValue);
            
            default:
                Debug.LogWarning("Unknown GoGoAlgorithm value");
                break;
        }
        return 0;
    }
}