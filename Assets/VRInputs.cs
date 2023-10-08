using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputs : MonoBehaviour
{
    PlayerController controller;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {


        controller.OnInputMove(UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.LeftHand));

        controller.OnInputJump(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.LeftHand));


        controller.OnInputZoom(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.RightHand));

        controller.OnTakePicture(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.RightHand));
        controller.OnTakePictureReleased(UnityXRInputBridge.instance.GetButtonUp(XRButtonMasks.primaryButton, XRHandSide.RightHand));


        controller.OnDeletePic(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.secondaryButton, XRHandSide.RightHand));

        controller.OnNextPicture(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primary2DAxisUp, XRHandSide.RightHand));
        controller.OnPrevPicture(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primary2DAxisDown, XRHandSide.RightHand));

        if(UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primary2DAxisRight, XRHandSide.RightHand))
        {
            transform.Rotate(new Vector3(0, 30, 0));
        }

        if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primary2DAxisLeft, XRHandSide.RightHand))
        {
            transform.Rotate(new Vector3(0, -30, 0));
        }
    }
}
