using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;


public class GoGoTeleportationAdapter : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public XRRayInteractor rayInteractor;
    public Transform xrOrigin;
    public  float baseVelocity = 1f; 
    public float velocityMultiplier = 150f; 

    void Start()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
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
                // Update game logic that uses hand data
                LogWristPosition();
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
                // Update visual objects that use hand data
                break;
        }
        
    }
    
    private void LogWristPosition()
    {
        if (m_HandSubsystem.rightHand.isTracked)
        {
            var wristJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.Wrist);

            if (wristJoint.TryGetPose(out Pose pose))
            {
                //Debug.Log($"{m_HandSubsystem.rightHand.handedness} palm position: {pose.position}, rotation: {pose.rotation}");

                Vector3 worldWristPosition = xrOrigin.transform.TransformPoint(pose.position);
                Vector3 headsetPosition = Camera.main.transform.position;
                worldWristPosition.y = headsetPosition.y;
                
                //Debug.Log("origin"+headsetPosition);
                
                float distance = Vector3.Distance(worldWristPosition, headsetPosition);
                
                rayInteractor.velocity = baseVelocity + velocityMultiplier * Mathf.Pow(distance, 2);
                
                Debug.Log("Distance to wrist (XZ plane): " + Mathf.Pow(distance, 2) + " meters");
                
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }
}