using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Hands;

public class GoGoTeleportationAdapter : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public XRRayInteractor rayInteractor;
    public Transform xrOrigin;

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
                Debug.Log("Hand update (dynamic)");
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
                // Update visual objects that use hand data
                Debug.Log("Hand update (BeforeRender)");
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
                Debug.Log($"{m_HandSubsystem.rightHand.handedness} wrist position: {pose.position}, rotation: {pose.rotation}");
                // var localWristPos = pose.position;
                // var globalWristPos = transform.TransformPoint(localWristPos);
                // var globalXrOriginPos = xrOrigin.position;

                // var adjustedVec = new Vector3(globalXrOriginPos.x, globalXrOriginPos.y, globalWristPos.z);

                // Debug.Log("posePos "+localWristPos);
                // Debug.Log("globalWristPos "+globalWristPos);
                // Debug.Log("globalXrOriginPos "+globalXrOriginPos);
                

                // float distance = Vector3.Distance(adjustedVec, globalXrOriginPos);
                // float velocity = Mathf.Clamp(5f + (distance) * 50, 5f, 40f);
                // rayInteractor.velocity = velocity;
                // Debug.Log("distance "+distance);
                // Debug.Log("velocity "+velocity);
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }
}