using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;


public class GoGoDetachAdapterStable : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public Transform xrOrigin;
    private  float minVirtDistance = 0.1f; 
    private float maxVirtDistance = 70f;
    private float thresholdDistance = 0.1f;

    public bool rayStabilized = false;
    public float minDistance = 0.1f;
    public float maxDistance = 0.6f;
    public float p = 2.0f;
    public XRInteractionGroup rightHand;
    
    private float previousForwardDistance = 0f;
    private const float distanceChangeThreshold = 0.0005f;
    
    void Start()
    {
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

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
            if (wristJoint.TryGetPose(out Pose pose))
            {
                Vector3 worldWristPosition = xrOrigin.transform.TransformPoint(pose.position);
                Vector3 headsetPosition = Camera.main.transform.position;
                worldWristPosition.y = headsetPosition.y;
                
                // Calculate the forward direction of the headset
                Vector3 headsetForward = Camera.main.transform.forward;
                headsetForward.y = 0; // Ensure the forward vector is on the XZ plane
                headsetForward.Normalize();
            
                // Calculate the direction from the headset to the wrist
                Vector3 directionToWrist = worldWristPosition - headsetPosition;
                directionToWrist.y = rightHand.transform.position.y; // Ensure the direction vector is on the XZ plane
            
                // Project the directionToWrist onto the headsetForward
                float forwardDistance = Vector3.Dot(directionToWrist, headsetForward);
                
                // Stabilize ray over certain threshold
                if (forwardDistance > 0.25f)
                {
                    rayStabilized = true;
                }
                else
                {
                    rayStabilized = false;
                }
                
                if (Mathf.Abs(forwardDistance - previousForwardDistance) > distanceChangeThreshold)
                {
                    previousForwardDistance = forwardDistance;

                    // Adjust the sensitivity by changing the power or scaling factor
                    float scaledDistance = (forwardDistance - minDistance) / (maxDistance - minDistance);
                    if (scaledDistance > 0)
                    {
                        float virtualDistance = minVirtDistance +
                                                Mathf.Pow(scaledDistance, 2) * (maxVirtDistance - minVirtDistance);

                        Vector3 newPosition = worldWristPosition + headsetForward * virtualDistance;
                        newPosition.y = 0; // Adjust as needed to keep the hand at desired height

                        rightHand.transform.position = newPosition;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }
}