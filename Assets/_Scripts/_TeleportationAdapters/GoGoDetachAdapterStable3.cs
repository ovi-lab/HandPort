using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;

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
    public InputActionReference middleFingerPinch;
    public InputActionReference middleFingerPinchStrength;
    [Header("Reference Transforms")]
    public Transform xrOrigin;
    public Transform rightHand;
    public Transform rightHandInPlace;
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
    [SerializeField] private float minCufoff2 =0.3f;
    [SerializeField] private float beta2 = 3f;
    [SerializeField] private float dCutoff2 = 1f;
    private OneEuroFilter positionFilter;
    private OneEuroFilter gogoHandFilter;

    [Header("Participant Measurements")]
    [SerializeField] private float originShoulderDistance;
    [SerializeField] private float elbowWristDistance;
    [SerializeField] private float shoulderElbowDistance;
    [SerializeField] public Vector3 shoulderToWristDirection;

    private float previousNormDeltaForward = 0f;
    private bool granularMode;
    private Camera mainCamera;
    private CustomActionBasedControllerStable3 controller;
    private Vector3 currentWristPos;
    private bool wasLastFramePressed;
    private Vector3 targetWristPos;
    private Vector3 previousTargetWristPos;
    private Vector3 previousHandPos;
    private Vector3 currentHandPos;
    private Vector3 granularModeAnchor;
    private XRHandSubsystem m_HandSubsystem;

    void Start()
    {
        mainCamera = Camera.main;
        currentWristPos = Vector3.zero;
        controller = GetComponent<CustomActionBasedControllerStable3>();


        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        minDistance =  elbowWristDistance;
        maxDistance = elbowWristDistance + shoulderElbowDistance;

        foreach (XRHandSubsystem handSubsystem in handSubsystems)
        {
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
        gogoHandFilter =
            new OneEuroFilter(minCutoff: minCufoff2, beta: beta2, dCutoff: dCutoff2, initialDt: Time.deltaTime);
    }

    private void OnValidate()
    {
        gogoHandFilter =
            new OneEuroFilter(minCutoff: minCufoff2, beta: beta2, dCutoff: dCutoff2, initialDt: Time.deltaTime);
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
            XRHandJoint wristJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.Wrist);
            if (wristJoint.TryGetPose(out Pose wristPose))
            {
                Vector3 worldWristPosition = positionFilter.FilterPosition(xrOrigin.transform.TransformPoint(wristPose.position));
                worldWristPosition.y = 0;

                // Calculate shoulderToWristDirection
                Vector3 XZOriginPos = new Vector3(xrOrigin.position.x, 0f, xrOrigin.position.z);
                shoulderToWristDirection = worldWristPosition - (XZOriginPos + mainCamera.transform.right * originShoulderDistance);
                shoulderToWristDirection.y = 0;
                shoulderToWristDirection.Normalize();

                // Calculate the direction from the adjusted xrOrigin to the wrist
                Vector3 xrOriginToWristDirection = worldWristPosition - XZOriginPos;
                xrOriginToWristDirection.y = 0;

                // Project xrToWristDirection onto shoulderToWristDirection
                float deltaForward = Vector3.Dot(xrOriginToWristDirection, shoulderToWristDirection);
                float clampedDeltaForward = Mathf.Clamp(deltaForward, minDistance, maxDistance);

                // Normalize DeltaForward
                normalizedDeltaForward = Mathf.InverseLerp(minDistance, maxDistance, clampedDeltaForward);
                normalizedDeltaForward = Mathf.Clamp(normalizedDeltaForward, 0f, 1f);

                if (normalizedDeltaForward > 0.01)
                {
                    // Project NormalizedDeltaForward onto virtualDistance
                    float virtualDistance = CalculateVirtDistance();
                    float posX = Mathf.Lerp(minVirtDistance, maxVirtDistance, normalizedDeltaForward);
                    float posY = Mathf.Lerp(minVirtDistance, maxVirtDistance, normalizedDeltaForward);
                    shoulderToWristDirection.Scale(new Vector3(virtualDistance, virtualDistance, virtualDistance));

                    if (granularMode)
                    {
                        targetWristPos = granularModeAnchor + (rightHandInPlace.position - xrOrigin.position);
                        targetWristPos.y = xrOrigin.transform.position.y;
                    }
                    else
                    {
                        targetWristPos = positionFilter.FilterPosition(worldWristPosition) + shoulderToWristDirection;
                        targetWristPos.y = xrOrigin.transform.position.y;
                        float scaleFactor = 1 + virtualDistance / 10;
                        rightHandScaleAnchor.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
                    }

                    previousNormDeltaForward = normalizedDeltaForward;

                    // Scale hand visualisation

                }
                else if (normalizedDeltaForward <= 0.01)
                {
                    // // Reset position of hand to original position
                    targetWristPos = xrOrigin.position;
                    //rightHand.transform.position = resetPosition;

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

    void Update()
    {

        float leftPinchPressedValue = controller.selectAction.action.ReadValue<float>();
        bool leftPinchPressed = controller.selectAction.action.IsPressed();

        granularMode = leftPinchPressedValue > 0.85f;

        if (leftPinchPressed && !wasLastFramePressed)
        {
            granularModeAnchor = rightHand.transform.position + (rightHandInPlace.position - xrOrigin.position);
        }

        if (!leftPinchPressed && wasLastFramePressed)
        {
            StartCoroutine(GranularModeBuffer());
        }

        wasLastFramePressed = leftPinchPressed;
        targetWristPos = gogoHandFilter.FilterPosition(targetWristPos);
        rightHand.transform.position = targetWristPos;
    }

    private IEnumerator GranularModeBuffer()
    {
        yield return new WaitForSeconds(0.3f);
        granularMode = false;
    }
    private float CalculateVirtDistance()
    {
        float lerpValue = 0;
        switch (goGoAlgorithm)
        {
            case GoGoAlgorithm.Root:
                lerpValue = Mathf.Pow(normalizedDeltaForward, 1f / 2f);
                break;
            case GoGoAlgorithm.Sigmoid:
                lerpValue = 1f / (1f + Mathf.Exp(6 - 12 * normalizedDeltaForward));
                break;
            case GoGoAlgorithm.Power:
                lerpValue = Mathf.Pow(normalizedDeltaForward, 2f);
                break;
            case GoGoAlgorithm.Linear:
                lerpValue = normalizedDeltaForward;
                break;
            default:
                Debug.LogWarning("Unknown GoGoAlgorithm value");
                break;
        }
        return Mathf.Lerp(minVirtDistance, maxVirtDistance, lerpValue);
    }

}