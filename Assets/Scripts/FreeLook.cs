using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookCamera : MonoBehaviour
{
    public Camera mainCamera; // The main camera controlled by Cinemachine
    public CinemachineFreeLook cinemachineFreeLook; // The Cinemachine camera
    public Camera freeLookCamera; // The manual free-look camera
    public float mouseSensitivity = 100f; // Mouse sensitivity

    private float xRotation;
    private float yRotation;

    private BallInput inputActions; // Reference to the input actions class
    private bool isFreeLooking;
    
    public Action<bool> SetFreeLook;

    private void Awake()
    {
        // Initialize input actions
        freeLookCamera.enabled = false;
        cinemachineFreeLook.enabled = true;
        inputActions = new BallInput();
        inputActions.PogoControls.ToggleFreeLook.canceled += ToggleFreeLook;
    }

    private void ToggleFreeLook(InputAction.CallbackContext context)
    {
        isFreeLooking = !isFreeLooking;
        SetFreeLook?.Invoke(isFreeLooking);

        if (isFreeLooking)
        {
            // Capture the current rotation of the main camera
            Quaternion mainCamRotation = mainCamera.transform.rotation;
            freeLookCamera.transform.rotation = mainCamRotation;

            // Convert rotation to Euler angles
            Vector3 eulerRotation = mainCamRotation.eulerAngles;
            xRotation = eulerRotation.x;
            yRotation = eulerRotation.y;
        }
        else
        {
            // When exiting free look, apply free look camera's rotation to main camera
            mainCamera.transform.rotation = freeLookCamera.transform.rotation;
        }

        // Enable/Disable appropriate cameras
        freeLookCamera.enabled = isFreeLooking;
        cinemachineFreeLook.enabled = !isFreeLooking;

        // Update cursor state
        Cursor.lockState = isFreeLooking ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFreeLooking;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        if (isFreeLooking)
        {
            // Read mouse delta from input
            Vector2 mouseDelta = inputActions.PogoControls.MouseDelta.ReadValue<Vector2>();

            // Adjust rotations based on mouse input
            yRotation += mouseDelta.x * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp vertical rotation

            // Apply rotation to the free look camera
            freeLookCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}
