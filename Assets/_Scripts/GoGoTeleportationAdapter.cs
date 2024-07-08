using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;


public class GoGoTeleportationAdapter : MonoBehaviour
{
    private XRHandSubsystem m_HandSubsystem;
    public XRRayInteractor rayInteractor;
    public Transform xrOrigin;
    public  float minVelocity = 5f; 
    public float maxVelocity = 200f;

    public float minDistance = 0.1f;
    public float maxDistance = 0.6f;
    public float p = 4.0f;

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
                LogWristPosition();
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
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
                Vector3 worldWristPosition = xrOrigin.transform.TransformPoint(pose.position);
                Vector3 headsetPosition = Camera.main.transform.position;
                worldWristPosition.y = headsetPosition.y;
                
                float distance = Vector3.Distance(worldWristPosition, headsetPosition);


                float scaledDistance = (distance - minDistance) / (maxDistance - minDistance);
                //float virtualDistance = minDistance + Mathf.Pow(scaledDistance, p) * (maxDistance - minDistance); 

                float velocity = minVelocity + Mathf.Pow(scaledDistance, 4) * (maxVelocity - minVelocity);
                rayInteractor.velocity = velocity;

                Debug.Log("Distance headset to wrist: " + distance);

            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }

}