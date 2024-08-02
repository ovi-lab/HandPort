﻿using UnityEngine.InputSystem;
using System.Collections.Generic;


namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interprets feature values on a tracked input controller device using actions from the Input System
    /// into XR Interaction states, such as Select. Additionally, it applies the current Pose value
    /// of a tracked device to the transform of the GameObject.
    /// </summary>
    /// <remarks>
    /// This behavior requires that the Input System is enabled in the <b>Active Input Handling</b>
    /// setting in <b>Edit &gt; Project Settings &gt; Player</b> for input values to be read.
    /// Each input action must also be enabled to read the current value of the action. Referenced
    /// input actions in an Input Action Asset are not enabled by default.
    /// </remarks>
    /// <seealso cref="XRBaseController"/>
    [AddComponentMenu("XR/XR Controller (Action-based)", 11)]
    // [HelpURL(XRHelpURLConstants.k_ActionBasedController)]
    public class CustomActionBasedControllerStable3 : CustomActionBasedController
    {
        private GoGoDetachAdapterStable3 adaper;
        private float stableEulerX = 0;
        private float stableEulerY = 0;
        private float stableEulerZ = 0;
        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            EnableAllDirectActions();

            // Find and reference GoGoDetachAdapterStable3
            adaper = GetComponent<GoGoDetachAdapterStable3>();
            if (adaper == null)
            {
                Debug.LogError("GoGoDetachAdapterStable3 component not found.");
            }
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            DisableAllDirectActions();
        }

        /// <inheritdoc />
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            if (controllerState == null)
                return;

            var posAction = m_PositionAction.action;
            var rotAction = m_RotationAction.action;
            var isTrackedInputAction = m_IsTrackedAction.action;
            var trackingStateInputAction = m_TrackingStateAction.action;


            // Warn the user if using referenced actions and they are disabled
            if (!m_HasCheckedDisabledTrackingInputReferenceActions && (posAction != null || rotAction != null))
            {
                if (IsDisabledReferenceAction(m_PositionAction) || IsDisabledReferenceAction(m_RotationAction))
                {
                    Debug.LogWarning(
                        "'Enable Input Tracking' is enabled, but Position and/or Rotation Action is disabled." +
                        " The pose of the controller will not be updated correctly until the Input Actions are enabled." +
                        " Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action." +
                        " The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.",
                        this);
                }

                m_HasCheckedDisabledTrackingInputReferenceActions = true;
            }

            // Update isTracked and inputTrackingState
            controllerState.isTracked = false;
            controllerState.inputTrackingState = InputTrackingState.None;

            // Actions without bindings are considered empty and will fallback
            if (isTrackedInputAction != null && isTrackedInputAction.bindings.Count > 0)
            {
                controllerState.isTracked = IsPressed(isTrackedInputAction);
            }
            else
            {
                // Fallback: Tracking State > {Position or Rotation when same device, combine otherwise}
                if (trackingStateInputAction?.activeControl?.device is TrackedDevice trackingStateTrackedDevice)
                {
                    controllerState.isTracked = trackingStateTrackedDevice.isTracked.isPressed;
                }
                else
                {
                    var positionTrackedDevice = posAction?.activeControl?.device as TrackedDevice;
                    var rotationTrackedDevice = rotAction?.activeControl?.device as TrackedDevice;

                    var positionIsTracked = positionTrackedDevice?.isTracked.isPressed ?? false;

                    // If the tracking devices are different, their Is Tracked values will be ANDed together
                    if (positionTrackedDevice != rotationTrackedDevice)
                    {
                        var rotationIsTracked = rotationTrackedDevice?.isTracked.isPressed ?? false;
                        controllerState.isTracked = positionIsTracked && rotationIsTracked;
                    }
                    else
                    {
                        controllerState.isTracked = positionIsTracked;
                    }
                }
            }

            // Actions without bindings are considered empty and will fallback
            if (trackingStateInputAction != null && trackingStateInputAction.bindings.Count > 0)
            {
                controllerState.inputTrackingState = (InputTrackingState)trackingStateInputAction.ReadValue<int>();
            }
            else
            {
                // Fallback: Is Tracked > {Position or Rotation when same device, combine otherwise}
                if (isTrackedInputAction?.activeControl?.device is TrackedDevice isTrackedDevice)
                {
                    controllerState.inputTrackingState = (InputTrackingState)isTrackedDevice.trackingState.ReadValue();
                }
                else
                {
                    var positionTrackedDevice = posAction?.activeControl?.device as TrackedDevice;
                    var rotationTrackedDevice = rotAction?.activeControl?.device as TrackedDevice;

                    var positionTrackingState = positionTrackedDevice != null
                        ? (InputTrackingState)positionTrackedDevice.trackingState.ReadValue()
                        : InputTrackingState.None;

                    // If the tracking devices are different only the InputTrackingState.Position and InputTrackingState.Rotation flags will be considered
                    if (positionTrackedDevice != rotationTrackedDevice)
                    {
                        var rotationTrackingState = rotationTrackedDevice != null
                            ? (InputTrackingState)rotationTrackedDevice.trackingState.ReadValue()
                            : InputTrackingState.None;

                        controllerState.inputTrackingState =
                            (positionTrackingState & InputTrackingState.Position) |
                            (rotationTrackingState & InputTrackingState.Rotation);
                    }
                    else
                    {
                        controllerState.inputTrackingState = positionTrackingState;
                    }
                }
            }

            // Update position when valid
            if (posAction != null && (controllerState.inputTrackingState & InputTrackingState.Position) != 0)
            {
                controllerState.position = posAction.ReadValue<Vector3>();
            }

            // Update rotation when valid
            if (rotAction != null && (controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
            {
                Quaternion rotation = rotAction.ReadValue<Quaternion>();
                Vector3 euler = rotation.eulerAngles;

                if (adaper != null)
                {
                    float stabilityFactor = 1.0f - adaper.normalizedDeltaForward;
                    //adaper.shoulderToWristDirection;
                    
                    Debug.Log(adaper.normalizedDeltaForward);

                    stableEulerX = Mathf.Lerp(stableEulerX, euler.x, stabilityFactor);
                    stableEulerY = Mathf.Lerp(stableEulerY, euler.y, stabilityFactor);
                    stableEulerZ = Mathf.Lerp(stableEulerZ, euler.z, stabilityFactor);
                    
                    euler.x = stableEulerX;
                    euler.y = stableEulerY;
                    euler.z = stableEulerZ;
                    
                    Debug.Log(euler);
                }

                controllerState.rotation = Quaternion.Euler(euler);
            }
        }
    }
}
