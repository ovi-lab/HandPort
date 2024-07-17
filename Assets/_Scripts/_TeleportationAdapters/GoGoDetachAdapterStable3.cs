using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;


public class GoGoDetachAdapterStable3 : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public Transform xrOrigin;
    private  float minVirtDistance = 0.1f; 
    private float maxVirtDistance = 80f;

    public float scaledDistance = 0f;
    public float minDistance = 0.15f;
    public float maxDistance = 0.5f;
    public float p = 2.0f;
    public Transform rightHand;
    
    private OneEuroFilter positionFilter;
    private OneEuroFilterVector3 headsetFilter;

    public Transform rightHandScaleAnchor;
    
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
        positionFilter = new OneEuroFilter(minCutoff: 0.1f, beta: 0.1f, dCutoff: 1.0f, initialDt: Time.deltaTime);
        Vector3 initialHeadsetForward = Camera.main.transform.forward;  // Use current headset forward as initial value
        initialHeadsetForward.y = 0; // Ensure the forward vector is on the XZ plane
        initialHeadsetForward.Normalize();
        headsetFilter = new OneEuroFilterVector3(initialHeadsetForward, minCutoff: 0.01f, beta: 0.02f);
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

                // Filter headset forward direction
                Vector3 filteredForward = headsetFilter.Filter(headsetForward, Time.deltaTime);
            
                // Calculate the direction from the headset to the wrist
                Vector3 directionToWrist = worldWristPosition - headsetPosition;
                directionToWrist.y = rightHand.transform.position.y; // Ensure the direction vector is on the XZ plane
            
                // Project the directionToWrist onto the headsetForward
                float forwardDistance = Vector3.Dot(directionToWrist, filteredForward);

                // Adjust the sensitivity by changing the power or scaling factor
                scaledDistance = (forwardDistance - minDistance) / (maxDistance - minDistance);
                if (scaledDistance > 0)
                {
                    float virtualDistance = minVirtDistance +
                                            Mathf.Pow(scaledDistance, 2) * (maxVirtDistance - minVirtDistance);
                    
                    Vector3 newPosition = worldWristPosition + filteredForward  * virtualDistance;
                    newPosition.y = xrOrigin.transform.position.y; // Adjust as needed to keep the hand at desired height
                    
                    newPosition = positionFilter.FilterPosition(newPosition);

                    rightHand.transform.position = newPosition;
                    
                    // Scale hand visualisation
                    float scaleFactor = 1f + Mathf.Pow(scaledDistance, 2)*5;
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