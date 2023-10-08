using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] [Tooltip("Player mouse look sensitivity")]
    private float lookSensitivity = 0.1f;
    
    [SerializeField] [Tooltip("Movement speed in m/s")]
    private float movementSpeed = 3.0f;

    [SerializeField] [Tooltip("Acceleration due to gravity in m/s2")]
    private float gravity = 9.81f;

    [SerializeField] [Tooltip("Maximum fall speed in m/s")]
    private float maxFallSpeed = 100.0f;

    [SerializeField] [Tooltip("Jump velocity, in m/s")]
    private float jumpVelocity = 3.0f;


    [SerializeField] private CameraPhone phone;
    
    private CharacterController _controller;
    private Camera _camera;

    private Vector2 _lookInput;
    private float _camPitch;
    private Vector2 _movementInput;
    private float _yVelocity;
    private bool _hasJustJumped;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
        _camPitch = _camera.transform.eulerAngles.x;
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void LateUpdate()
    {
        UpdateLook();
        phone.UpdateTransform();
    }

    private void UpdateLook()
    {
        transform.Rotate(Vector3.up, _lookInput.x * lookSensitivity);
        _camPitch -= _lookInput.y * lookSensitivity;
        _camPitch = Mathf.Clamp(_camPitch, -90.0f, 90.0f);
        Vector3 camEulerAngles = _camera.transform.eulerAngles;
        _camera.transform.eulerAngles = new Vector3(_camPitch, camEulerAngles.y, camEulerAngles.z);
    }
    
    private void UpdateMovement()
    {
        bool jumpedThisFrame = false;
        if (_hasJustJumped)
        {
            jumpedThisFrame = true;
            _hasJustJumped = false;
        }
        
        if (_controller.isGrounded && jumpedThisFrame)
        {
            _yVelocity = jumpVelocity;
        }
        else
        {
            _yVelocity = Mathf.Max(_yVelocity - gravity * Time.deltaTime, -maxFallSpeed);
        }
        Vector3 velocity = Time.deltaTime * movementSpeed * (_camera.transform.forward * _movementInput.y + _camera.transform.right * _movementInput.x) + Time.deltaTime * _yVelocity * Vector3.up;
        _controller.Move(velocity);
        if (_controller.isGrounded)
        {
            _yVelocity = 0;
        }
    }

    public void OnInputLook(InputAction.CallbackContext ctx)
    {
        _lookInput = ctx.ReadValue<Vector2>();
    }
    
    public void OnInputMove(InputAction.CallbackContext ctx)
    {

            _movementInput = ctx.ReadValue<Vector2>();
        
    }

    public void OnInputMove(Vector2 moveOverride)
    {
         _movementInput = moveOverride;
    }

    public void OnInputJump(InputAction.CallbackContext ctx)
    {
        _hasJustJumped = ctx.performed;
    }

    public void OnInputJump(bool value)
    {
        _hasJustJumped = value;
    }

    public void OnInputZoom(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            phone.ToggleZoomAnim();
        }
    }

    public void OnInputZoom(bool value)
    {
        if (value)
        {
            phone.ToggleZoomAnim();
        }
    }

    public void OnTakePicture(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            phone.TakePicture();
            phone.ToggleButtonIcon(true);
        }
        else if (ctx.canceled)
        {
            phone.ToggleButtonIcon(false);
        }
    }

    public void OnTakePicture(bool value)
    {
        if (value)
        {
            phone.TakePicture();
            phone.ToggleButtonIcon(true);
        }
    }

    public void OnTakePictureReleased(bool value)
    {
        if (value)
        {
            phone.ToggleButtonIcon(false);
        }
    }

    public void OnPrevPicture(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            phone.MoveCursor(true);
        }
    }

    public void OnNextPicture(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            phone.MoveCursor(false);
        }
    }

    public void OnPrevPicture(bool value)
    {
        if (value)
        {
            phone.MoveCursor(true);
        }
    }

    public void OnNextPicture(bool value)
    {
        if (value)
        {
            phone.MoveCursor(false);
        }
    }

    public void OnDeletePic(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            phone.TryDeleteImage();
        }
    }

    public void OnDeletePic(bool value)
    {
        if (value)
        {
            phone.TryDeleteImage();
        }
    }

}
