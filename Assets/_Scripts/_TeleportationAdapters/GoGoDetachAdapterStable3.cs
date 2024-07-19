using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class GoGoDetachAdapterStable3 : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public Transform xrOrigin;
    private  float minVirtDistance = 0f; 
    private float maxVirtDistance = 60f;

    public float scaledDistance = 0f;
    private float minDistance;
    private float maxDistance;
    
    private float originShoulderDistance;
    private float ellbowWristDistance;
    private float shoulderEllbowDistance;
    
    public float p = 2.0f;
    public Transform rightHand;
    
    private OneEuroFilter positionFilter;
    public Transform rightHandScaleAnchor;
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
                Debug.Log("Found and using XRHandSubsystem.");
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
        positionFilter = new OneEuroFilter(minCutoff: 0.1f, beta: 0.1f, dCutoff: 1.0f, initialDt: Time.deltaTime);
    }
    
    public void SetArmMeasurements(float oSD, float sED, float eWD)
    {
        originShoulderDistance = oSD;
        ellbowWristDistance = sED;
        shoulderEllbowDistance = eWD;
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
            var middleTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.MiddleTip);
            var wristJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.Wrist);
            if (wristJoint.TryGetPose(out Pose wristPose) && (middleTipJoint.TryGetPose(out Pose middlePose)))
            {
                Vector3 worldWristPosition = xrOrigin.transform.TransformPoint(wristPose.position);
                worldWristPosition.y = 0;
                
                // Calculate the right direction from the headset's forward direction
                Vector3 headsetForward = Camera.main.transform.forward;
                headsetForward.y = 0;
                Vector3 rightDirection = Vector3.Cross(Vector3.up, headsetForward).normalized;

                // Adjust xrOrigin position to the right
                Vector3 adjustedXROriginPosition = xrOrigin.transform.position;
                adjustedXROriginPosition.y = 0;
                Vector3 forwardDirection = worldWristPosition - (adjustedXROriginPosition + rightDirection * originShoulderDistance);
                
                // Project the direction onto the XZ plane
                forwardDirection.y = 0;
                forwardDirection.Normalize();
            
                // Calculate the direction from the adjusted xrOrigin to the wrist
                Vector3 directionToWrist = worldWristPosition - adjustedXROriginPosition;
                directionToWrist.y = 0;

                // Project the directionToWrist onto the forwardDirection
                float forwardDistance = Vector3.Dot(directionToWrist, forwardDirection);

                // Adjust the sensitivity by changing the power or scaling factor
                scaledDistance = (forwardDistance - minDistance) / (maxDistance - minDistance);
                if (scaledDistance > 0)
                {
                    float virtualDistance = minVirtDistance +
                                            Mathf.Pow(scaledDistance, 2) * (maxVirtDistance - minVirtDistance);
                    
                    Vector3 newPosition = worldWristPosition + forwardDirection  * virtualDistance;
                    newPosition.y = xrOrigin.transform.position.y; // Adjust as needed to keep the hand at desired height

                    rightHand.transform.position = positionFilter.FilterPosition(newPosition);
                    
                    // Scale hand visualisation
                    float scaleFactor = 1f + Mathf.Pow(scaledDistance, 2)*10;
                    rightHandScaleAnchor.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                }
                else
                {
                    Vector3 resetPosition = xrOrigin.position;
                    resetPosition = positionFilter.FilterPosition(resetPosition);

                    rightHand.transform.position = resetPosition;
                }
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }
}