using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum XRButtonMasks
{
    primaryButton,
    secondaryButton,
    triggerButton,
    gripButton,
    menuButton,
    primary2DAxisClick,
    secondary2DAxisClick,
    primary2DAxisUp,
    primary2DAxisDown,
    primary2DAxisLeft,
    primary2DAxisRight,
    secondary2DAxisUp,
    secondary2DAxisDown,
    secondary2DAxisLeft,
    secondary2DAxisRight,
    primaryButtonTouch,
    secondaryButtonTouch
}

public enum XRAxisMasks
{
    triggerAxis,
    gripAxis
}

public enum XRHandFinger
{
    IndexFinger,
    MiddleFinger,
    PinkyFinger,
    RingFinger
}

public enum XR2DAxisMasks
{
    primary2DAxis,
    secondary2DAxis
}

public enum XRHandSide
{
    LeftHand,
    RightHand
}

public class UnityXRInputBridge : MonoBehaviour
{
    public static UnityXRInputBridge instance;

    InputDevice LHand, RHand;

    private XRButtonMasks[] availableButtons = new XRButtonMasks[15];

    public bool Initialized;

    private void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        if (InputDevices.GetDeviceAtXRNode(XRNode.Head).subsystem != null)
            InputDevices.GetDeviceAtXRNode(XRNode.Head).subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

        availableButtons[0] = XRButtonMasks.primaryButton;
        availableButtons[1] = XRButtonMasks.secondaryButton;
        availableButtons[2] = XRButtonMasks.triggerButton;
        availableButtons[3] = XRButtonMasks.gripButton;
        availableButtons[4] = XRButtonMasks.menuButton;
        availableButtons[5] = XRButtonMasks.primary2DAxisClick;
        availableButtons[6] = XRButtonMasks.secondary2DAxisClick;
        availableButtons[7] = XRButtonMasks.primary2DAxisUp;
        availableButtons[8] = XRButtonMasks.primary2DAxisDown;
        availableButtons[9] = XRButtonMasks.primary2DAxisLeft;
        availableButtons[10] = XRButtonMasks.primary2DAxisRight;
        availableButtons[11] = XRButtonMasks.secondary2DAxisUp;
        availableButtons[12] = XRButtonMasks.secondary2DAxisDown;
        availableButtons[13] = XRButtonMasks.secondary2DAxisLeft;
        availableButtons[14] = XRButtonMasks.secondary2DAxisRight;

        while (devices.Count < 3)
        {
            yield return new WaitForSeconds(0.5f);
            InputDevices.GetDevices(devices);
        }

        LHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        RHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        foreach (var button in availableButtons)
        {
            LbuttonLastStates.Add(button, false);
            LbuttonDownStates.Add(button, false);
            LbuttonUpStates.Add(button, false);

            RbuttonLastStates.Add(button, false);
            RbuttonDownStates.Add(button, false);
            RbuttonUpStates.Add(button, false);
        }

        Initialized = true;
    }

    public bool GetButton(XRButtonMasks buttonMask, XRHandSide handside)
    {
        InputFeatureUsage<bool> button = CommonUsages.primaryButton;

        switch (buttonMask)
        {
            case XRButtonMasks.primaryButton:
                button = CommonUsages.primaryButton;
                break;
            case XRButtonMasks.secondaryButton:
                button = CommonUsages.secondaryButton;
                break;
            case XRButtonMasks.primaryButtonTouch:
                button = CommonUsages.primaryTouch;
                break;
            case XRButtonMasks.secondaryButtonTouch:
                button = CommonUsages.secondaryTouch;
                break;
            case XRButtonMasks.triggerButton:
                button = CommonUsages.triggerButton;
                break;
            case XRButtonMasks.gripButton:
                button = CommonUsages.gripButton;
                break;
            case XRButtonMasks.menuButton:
                button = CommonUsages.menuButton;
                break;
            case XRButtonMasks.primary2DAxisClick:
                button = CommonUsages.primary2DAxisClick;
                break;
            case XRButtonMasks.secondary2DAxisClick:
                button = CommonUsages.secondary2DAxisClick;
                break;
            case XRButtonMasks.primary2DAxisUp:
                return GetVec2(XR2DAxisMasks.primary2DAxis, handside).y > 0.25;

            case XRButtonMasks.primary2DAxisDown:
                return GetVec2(XR2DAxisMasks.primary2DAxis, handside).y < -0.25;

            case XRButtonMasks.primary2DAxisLeft:
                return GetVec2(XR2DAxisMasks.primary2DAxis, handside).x < -0.25;

            case XRButtonMasks.primary2DAxisRight:
                return GetVec2(XR2DAxisMasks.primary2DAxis, handside).x > 0.25;

            case XRButtonMasks.secondary2DAxisUp:
                return GetVec2(XR2DAxisMasks.secondary2DAxis, handside).y > 0.25;

            case XRButtonMasks.secondary2DAxisDown:
                return GetVec2(XR2DAxisMasks.secondary2DAxis, handside).y < -0.25;

            case XRButtonMasks.secondary2DAxisLeft:
                return GetVec2(XR2DAxisMasks.secondary2DAxis, handside).x < -0.25;

            case XRButtonMasks.secondary2DAxisRight:
                return GetVec2(XR2DAxisMasks.secondary2DAxis, handside).x > 0.25;

            default:
                break;
        }

        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        bool value;

        hand.TryGetFeatureValue(button, out value);

        return value;
    }

    public Vector3 GetVel(XRHandSide handside)
    {
        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        Vector3 value;

        hand.TryGetFeatureValue(CommonUsages.deviceVelocity, out value);

        return value;
    }

    public Vector3 GetAngVel(XRHandSide handside)
    {
        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        Vector3 value;

        hand.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out value);

        return value;
    }

    public Dictionary<XRButtonMasks, bool> LbuttonLastStates = new Dictionary<XRButtonMasks, bool>();

    public Dictionary<XRButtonMasks, bool> LbuttonDownStates = new Dictionary<XRButtonMasks, bool>();

    public Dictionary<XRButtonMasks, bool> LbuttonUpStates = new Dictionary<XRButtonMasks, bool>();

    public Dictionary<XRButtonMasks, bool> RbuttonLastStates = new Dictionary<XRButtonMasks, bool>();

    public Dictionary<XRButtonMasks, bool> RbuttonDownStates = new Dictionary<XRButtonMasks, bool>();

    public Dictionary<XRButtonMasks, bool> RbuttonUpStates = new Dictionary<XRButtonMasks, bool>();

    private void FixedUpdate()
    {

        if (!Initialized)
        {
            return;
        }

        foreach (var button in availableButtons)
        {
            LbuttonDownStates[button] = false;
            LbuttonUpStates[button] = false;
            RbuttonDownStates[button] = false;
            RbuttonUpStates[button] = false;
        }

        foreach (var button in availableButtons)
        {
            bool buttonState = GetButton(button, XRHandSide.LeftHand);
            if (LbuttonLastStates.ContainsKey(button))
                if (buttonState != LbuttonLastStates[button])
                {

                    if (buttonState)
                    {
                        LbuttonDownStates[button] = true;
                    }
                    else
                    {
                        LbuttonUpStates[button] = true;
                    }

                    LbuttonLastStates[button] = buttonState;
                }
				
			buttonState = GetButton(button, XRHandSide.RightHand);
            if (RbuttonLastStates.ContainsKey(button))
                if (buttonState != RbuttonLastStates[button])
                {

                    if (buttonState)
                    {
                        RbuttonDownStates[button] = true;
                    }
                    else
                    {
                        RbuttonUpStates[button] = true;
                    }

                    RbuttonLastStates[button] = buttonState;
                }
        }

    }

    public bool GetButtonDown(XRButtonMasks button, XRHandSide hand)
    {
        switch (hand)
        {
            case XRHandSide.LeftHand:
                if (LbuttonDownStates.ContainsKey(button))
                {
                    return LbuttonDownStates[button];
                }
                else
                {
                    return false;
                }
            case XRHandSide.RightHand:
                if (RbuttonDownStates.ContainsKey(button))
                {
                    return RbuttonDownStates[button];
                }
                else
                {
                    return false;
                }

            default:
                return false;
        }
    }

    public bool GetButtonUp(XRButtonMasks button, XRHandSide hand)
    {
        switch (hand)
        {
            case XRHandSide.LeftHand:
                if (LbuttonUpStates.ContainsKey(button))
                {
                    return LbuttonUpStates[button];
                }
                else
                {
                    return false;
                }

            case XRHandSide.RightHand:
                if (RbuttonUpStates.ContainsKey(button))
                {
                    return RbuttonUpStates[button];
                }
                else
                {
                    return false;
                }

            default:
                return false;
        }
    }

    public float GetFloat(XRAxisMasks axis, XRHandSide handside)
    {
        InputFeatureUsage<float> button = CommonUsages.trigger;

        switch (axis)
        {
            case XRAxisMasks.triggerAxis:
                button = CommonUsages.trigger;
                break;
            case XRAxisMasks.gripAxis:
                button = CommonUsages.grip;
                break;
            default:
                break;
        }

        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        float value;

        hand.TryGetFeatureValue(button, out value);

        return value;
    }

    public List<float> GetHandData(XRHandSide handside)
    {
        InputFeatureUsage<Hand> data = CommonUsages.handData;

        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        List<float> FingerRotations = new List<float>();

        hand.TryGetFeatureValue(data, out Hand value);

        List<Bone> bones = new List<Bone>();

        value.TryGetFingerBones(HandFinger.Index, bones);

        bones[0].TryGetRotation(out Quaternion indexrot);
        FingerRotations.Add(indexrot.eulerAngles.x);

        value.TryGetFingerBones(HandFinger.Middle, bones);

        bones[0].TryGetRotation(out Quaternion middlerot);
        FingerRotations.Add(middlerot.eulerAngles.x);

        value.TryGetFingerBones(HandFinger.Ring, bones);

        bones[0].TryGetRotation(out Quaternion ringrot);
        FingerRotations.Add(ringrot.eulerAngles.x);

        value.TryGetFingerBones(HandFinger.Pinky, bones);

        bones[0].TryGetRotation(out Quaternion pinkyrot);
        FingerRotations.Add(pinkyrot.eulerAngles.x);

        value.TryGetFingerBones(HandFinger.Thumb, bones);

        bones[0].TryGetRotation(out Quaternion thumbrot);
        FingerRotations.Add(thumbrot.eulerAngles.x);

        return FingerRotations;
    }

    public List<float> GetCalibratedFingerRotations(XRHandSide handside)
    {
        InputFeatureUsage<Hand> data = CommonUsages.handData;

        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        List<float> CalibratedFingerRotations = new List<float>();

        hand.TryGetFeatureValue(data, out Hand value);

        foreach (HandFinger finger in Enum.GetValues(typeof(HandFinger)))
        {
            List<Bone> bones = new List<Bone>();
            value.TryGetFingerBones(finger, bones);

            if (bones.Count > 0)
            {
                bones[0].TryGetRotation(out Quaternion fingerRot);

                // Calibrate the rotation to be within the range of 0-1
                float calibratedRotation = Mathf.Clamp01(Mathf.InverseLerp(0f, 360f, fingerRot.eulerAngles.x));

                CalibratedFingerRotations.Add(calibratedRotation);
            }
            else
            {
                // If no bone data is available, add 0
                CalibratedFingerRotations.Add(0f);
            }
        }

        return CalibratedFingerRotations;
    }

    public Vector2 GetVec2(XR2DAxisMasks axis, XRHandSide handside)
    {
        InputFeatureUsage<Vector2> button = CommonUsages.primary2DAxis;

        switch (axis)
        {
            case XR2DAxisMasks.primary2DAxis:
                button = CommonUsages.primary2DAxis;
                break;
            case XR2DAxisMasks.secondary2DAxis:
                button = CommonUsages.secondary2DAxis;
                break;
            default:
                break;
        }

        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        Vector2 value;

        hand.TryGetFeatureValue(button, out value);

        return value;
    }

    public void SetHaptics(float intensity, XRHandSide handside)
    {
        InputDevice hand = LHand;

        switch (handside)
        {
            case XRHandSide.LeftHand:
                hand = LHand;
                break;
            case XRHandSide.RightHand:
                hand = RHand;
                break;
            default:
                break;
        }

        hand.SendHapticImpulse(0, intensity, Time.deltaTime);
    }
}
