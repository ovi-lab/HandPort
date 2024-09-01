using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public enum GoGoAlgorithm
{
    Power = 0,
    Sigmoid = 1,
    Root = 2,
    Linear = 3,
}
public class GoGoDetachAdapterStable3 : MonoBehaviour
{
    public GoGoAlgorithm goGoAlgorithm;


    [Header("Reference Transforms")]
    public Transform xrOrigin;
    public Transform rightHand;
    public Transform rightHandScaleAnchor;

    [Header("Go Go Parameters")]
    public float normalizedDeltaForward = 0f;
    [SerializeField] float handMovementThreshold = 0.01f;
    [SerializeField] private float minVirtDistance = 0f;
    [SerializeField] private float maxVirtDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    [Header("One Euro Filter")]
    [SerializeField] private float minCufoff =0.3f;
    [SerializeField] private float beta = 3f;
    [SerializeField] private float dCutoff = 1f;
    private OneEuroFilter positionFilter;

    [Header("Participant Measurements")]
    [SerializeField] private float originShoulderDistance;
    [SerializeField] private float elbowWristDistance;
    [SerializeField] private float shoulderElbowDistance;
    [SerializeField] public Vector3 shoulderToWristDirection;

    private XRHandSubsystem m_HandSubsystem;
    private float previousNormDeltaForward = 0f;
    private Vector3 currentWristPos;
    private Vector3 targetWristPos;
    private int interpolationFramesCount = 45;
    private int elapsedFrames = 0;
    private Vector3 previousTargetWristPos;

    void Start()
    {
        currentWristPos = Vector3.zero;
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        minDistance =  elbowWristDistance;
        maxDistance = elbowWristDistance + shoulderElbowDistance;

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
        positionFilter = new OneEuroFilter(minCutoff: minCufoff, beta: beta, dCutoff: dCutoff, initialDt: Time.deltaTime);
    }

    public void SetInitialAdapterValues(float oSD, float sED, float eWD, float mVD)
    {
        originShoulderDistance = oSD-0.05f;
        shoulderElbowDistance = sED+0.04f;
        elbowWristDistance = eWD-0.04f;
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

                //Debug.Log(Math.Abs(normalizedDeltaForward - previousNormDeltaForward) );

                if (Math.Abs(normalizedDeltaForward - previousNormDeltaForward)  > handMovementThreshold)
                {
                    // Project NormalizedDeltaForward onto virtualDistance
                    float virtualDistance = CalculateVirtDistance();

                    // Calculate new position to hand
                    targetWristPos = worldWristPosition + shoulderToWristDirection  * virtualDistance;
                    targetWristPos.y = xrOrigin.transform.position.y;

                    previousNormDeltaForward = normalizedDeltaForward;

                    // Scale hand visualisation
                    float scaleFactor = 1+ virtualDistance / 10;
                    rightHandScaleAnchor.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
                }
                else if (normalizedDeltaForward <= 0.01)
                {
                    // // Reset position of hand to original position
                    targetWristPos = xrOrigin.position;
                    //rightHand.transform.position = resetPosition;
                    //
                    // // Reset scale of hand
                    rightHandScaleAnchor.transform.localScale = Vector3.one;
                }
            }
            else
            {
                Debug.LogWarning($"{m_HandSubsystem.rightHand.handedness} wrist joint pose data is not available.");
            }
        }
    }

    void Update()
    {

        if (targetWristPos != previousTargetWristPos)
        {
            currentWristPos = rightHand.transform.position;

            elapsedFrames = 0;

            // Update previousTargetWristPos to the new target
            previousTargetWristPos = targetWristPos;
        }

        if (targetWristPos != currentWristPos)
        {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentWristPos, targetWristPos, interpolationRatio);
            rightHand.transform.position = interpolatedPosition;
            elapsedFrames++;

            currentWristPos = interpolatedPosition;

            if (elapsedFrames >= interpolationFramesCount)
            {
                rightHand.transform.position = targetWristPos;
                elapsedFrames = 0;
                currentWristPos = targetWristPos;
            }
        }
    }

    private float CalculateVirtDistance()
    {
        switch (goGoAlgorithm)
        {

            case GoGoAlgorithm.Root:
                // ROOT
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, Mathf.Pow(normalizedDeltaForward, 1f/2f));

            case GoGoAlgorithm.Sigmoid:
                // SIGMOID
                float sigmoidValue = 1f / (1f + Mathf.Exp(6-12*normalizedDeltaForward));
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, sigmoidValue);

            case GoGoAlgorithm.Power:
                // QUADRATIC
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, Mathf.Pow(normalizedDeltaForward, 2f));

            case GoGoAlgorithm.Linear:
                // LINEAR
                return Mathf.Lerp(minVirtDistance, maxVirtDistance, normalizedDeltaForward);

            default:
                Debug.LogWarning("Unknown GoGoAlgorithm value");
                break;
        }
        return 0;
    }
}