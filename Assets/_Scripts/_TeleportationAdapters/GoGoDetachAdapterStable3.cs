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
    private OneEuroFilterVector3 forwardDirectionFilter;

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
        forwardDirectionFilter = new OneEuroFilterVector3((Vector3.forward), minCutoff: 0.01f, beta: 0.02f);
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
                Vector3 worldMiddleTipPosition = xrOrigin.transform.TransformPoint(middlePose.position);
                Vector3 headsetPosition = Camera.main.transform.position;
                worldWristPosition.y = headsetPosition.y;
                worldMiddleTipPosition.y = headsetPosition.y;
                
                // Calculate the forward direction vector from wrist to middle tip
                Vector3 forwardDirection = worldMiddleTipPosition - worldWristPosition;
                forwardDirection = forwardDirectionFilter.Filter(forwardDirection, Time.deltaTime);
                
                // Project the direction onto the XZ plane
                forwardDirection.y = 0;
                forwardDirection.Normalize();
                
                // Calculate the forward direction of the headset
                Vector3 headsetForward = Camera.main.transform.forward;
                headsetForward.y = 0; // Ensure the forward vector is on the XZ plane
                headsetForward.Normalize();
            
                // Calculate the direction from the headset to the wrist
                Vector3 directionToWrist = worldWristPosition - headsetPosition;
                directionToWrist.y = rightHand.transform.position.y; // Ensure the direction vector is on the XZ plane
            
                // Project the directionToWrist onto the headsetForward
                float forwardDistance = Vector3.Dot(directionToWrist, headsetForward);

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