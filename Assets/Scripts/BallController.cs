using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [Header("Ball Movement Settings")]
    public float moveForce = 10f;
    public float maxSquishTime = 2f;
    public float jumpForce = 10f;
    public float squishFactor = 0.5f;
    public Transform cameraTransform;
    public float scaleRecoverySpeed = 2f;
    public float airControlFactor = 0.5f;
    public float fallMultiplier = 2.5f;
    public float minJumpForce = 30f;
    public float maxJumpForce = 100f;
    public Vector3 originalScale;
    public float ropeHangForce = 10f;
    public float jumpRadiusFromCenter = 10f;

    private bool isFreeLooking = false;
    public FreeLookCamera freeLookCamera;

    [Header("Rope")]
    private bool isAttachedToRope = false;
    public RopeVisual ropeVisual;
    
    private Rigidbody rb;
    private bool isChargingJump;
    private float squishStartTime;
    private BallInput ballInput;
    private Vector2 movementInput;
    private bool isRecoveringScale;
    private List<RaycastHit> groundHits = new List<RaycastHit>();
    
    [Header("Ground Check")]
    public LayerMask groundLayers; 
    public float checkDistance = 0.2f;
    public Vector3 checkOrigin = Vector3.zero;
    private string currentSurfaceTag = "Untagged";
    private string previousSurfaceTag = "Untagged";

    [Header("Water Stuff")] 
    public ParticleSystem enterWater;
    public ParticleSystem enterWater2;
    public ParticleSystem enterWater3;
    
    public ParticleSystem rollingInWater;
    
    private Vector3 initialPosition;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 100f;
        originalScale = transform.localScale;
        ballInput = new BallInput();

        ballInput.PogoControls.Jump.started += OnJumpStarted;
        ballInput.PogoControls.Jump.canceled += OnJumpCanceled;
        ballInput.PogoControls.Move.performed += OnMovementPerformed;
        ballInput.PogoControls.Move.canceled += OnMovementCanceled;
        ballInput.PogoControls.Restart.canceled += OnRestart;
    }

    private void Start()
    {
        freeLookCamera.SetFreeLook += b => isFreeLooking = b;
        initialPosition = transform.position;
    }

    private void OnEnable() => ballInput.Enable();
    private void OnDisable() => ballInput.Disable();

    private void Update()
    {
        HandleMovement();
        HandleRopeMovement();
        HandleScaleSquish();
        HandleScaleRecovery();
        HandleAirborne();
        GetObjectBallStandingOn();
        WaterEffects();
    }

    private void HandleMovement()
    {
        if (isFreeLooking)
        {
            return;
        }
        
        if (isAttachedToRope) return;
        
        Vector3 direction = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        if (direction.magnitude >= 0.1f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDir = direction.z * cameraForward + direction.x * cameraRight;
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, moveDir);  
            float controlFactor = IsGrounded() ? 1f : airControlFactor;
            rb.AddTorque(torqueAxis * (moveForce * controlFactor), ForceMode.Impulse);
        }
    }

    private void HandleRopeMovement()
    {
        if (isFreeLooking)
        {
            return;
        }
        
        if(!isAttachedToRope) return;

        Vector3 direction = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        if (direction.magnitude >= 0.1f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDir = direction.z * cameraForward + direction.x * cameraRight;
            rb.AddForce(moveDir * ropeHangForce, ForceMode.Force);
        }
    }
    private void HandleScaleSquish()
    {
        if (isChargingJump)
        {
            float squishTime = Mathf.Clamp(Time.time - squishStartTime, 0, maxSquishTime);
            float squishAmount = 1 - (squishTime / maxSquishTime) * (1 - squishFactor);
            transform.localScale = originalScale * squishAmount;
        }
    }
    private void HandleScaleRecovery()
    {
        if (isRecoveringScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleRecoverySpeed);
            if (Vector3.Distance(transform.localScale, originalScale) < 0.01f)
            {
                transform.localScale = originalScale;
                isRecoveringScale = false;
            }
        }
    }
    private void HandleAirborne()
    {
        if (!IsGrounded())
        {
            rb.velocity += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
        }
    }
    private void GetObjectBallStandingOn()
    {
        RaycastHit hit;
        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y, transform.position.z) - checkOrigin;
        Vector3 rayDirection = Vector3.down;

        // Draw a black debug ray in Scene View
        Debug.DrawRay(rayOrigin, rayDirection * checkDistance, Color.black);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, checkDistance, groundLayers))
        {
            previousSurfaceTag = currentSurfaceTag;
            currentSurfaceTag = hit.collider.tag;
        }
        else
        {
            currentSurfaceTag = "Untagged";
        }
    }
    private void WaterEffects()
    {
        if (currentSurfaceTag == "Water" && previousSurfaceTag != "Water")
        {
            if (enterWater.isPlaying)
            {
                if (enterWater2.isPlaying)
                {
                    if (!enterWater3.isPlaying)
                    {
                        enterWater3.Play();
                    }
                }
                else
                {
                    enterWater2.Play();
                }
            }
            else
            {
                enterWater.Play();
            }
        }

        if (currentSurfaceTag == "Water")
        {
            Vector3 movement = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
            if (movement.magnitude > 0.1f)
            {
                if (!rollingInWater.isPlaying)
                {
                    rollingInWater.Play();
                }
            }
            else
            {
                if (rollingInWater.isPlaying)
                {
                    rollingInWater.Stop();
                }
            }
        }
        else
        {
            rollingInWater.Stop();
        }
    }
    
    private void Jump()
    {
        if (isFreeLooking) return;
        
        if (IsGrounded())
        {
            Vector3 combinedNormal = Vector3.zero;
            foreach (var hit in groundHits)
            {
                combinedNormal += hit.normal;
            }
            combinedNormal.Normalize();

            float squishTime = Mathf.Clamp(Time.time - squishStartTime, 0, maxSquishTime);
            float appliedJumpForce = Mathf.Clamp(jumpForce * (squishTime / maxSquishTime), minJumpForce, maxJumpForce);

            Vector3 jumpDirection = combinedNormal * appliedJumpForce;
            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }
    }
    private bool IsGrounded()
    {
        groundHits.Clear();
        int rayCount = 40; // Antall stråler
        float radius = jumpRadiusFromCenter;

        // Fibonacci-sfærisk fordeling for jevnt fordelte punkter på en kule
        for (int i = 0; i < rayCount; i++)
        {
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / rayCount); // Høydevinkel
            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * i; // Azimuthal vinkel

            Vector3 direction = new Vector3(
                Mathf.Sin(phi) * Mathf.Cos(theta),
                Mathf.Cos(phi),
                Mathf.Sin(phi) * Mathf.Sin(theta)
            );

            Debug.DrawRay(transform.position, direction * radius, Color.green);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, radius))
            {
                if (!groundHits.Exists(h => h.collider == hit.collider))
                {
                    groundHits.Add(hit);
                }
            }
        }

        return groundHits.Count > 0;
    }
    
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (isFreeLooking) return;
        if (isAttachedToRope) return;
        isChargingJump = true;
        squishStartTime = Time.time;
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (isFreeLooking) return;
        if (isAttachedToRope)
        {
            isAttachedToRope = !isAttachedToRope;
            if (gameObject.GetComponent<SpringJoint>() != null)
            {
                Destroy(gameObject.GetComponent<SpringJoint>());
            }
            
            ropeVisual.RemovePlayerFromRopeVisual();
        }
        
        if (isChargingJump)
        {
            if (IsGrounded())
            {
                Jump();
            }
            isChargingJump = false;
            isRecoveringScale = true;
        }
    }
    private void OnRestart(InputAction.CallbackContext context)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = initialPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LastRopeModule"))
        {
            if(gameObject.GetComponent<SpringJoint>() != null) return;
            
            ropeVisual.AddPlayerToRopeVisual();
            isAttachedToRope = true;
            // Get the Spring Joint component from the last module
            SpringJoint lastModuleSpringJoint = other.GetComponent<SpringJoint>();

            // Add a Spring Joint component to the ball
            SpringJoint ballSpringJoint = gameObject.AddComponent<SpringJoint>();
            ballSpringJoint.connectedBody = other.gameObject.GetComponent<Rigidbody>();

            // Copy all the settings from the last module's Spring Joint to the ball's Spring Joint
            if (lastModuleSpringJoint != null)
            {
                ballSpringJoint.autoConfigureConnectedAnchor = false;
                ballSpringJoint.anchor = lastModuleSpringJoint.anchor;
                ballSpringJoint.connectedAnchor = lastModuleSpringJoint.connectedAnchor;
                ballSpringJoint.spring = lastModuleSpringJoint.spring;
                ballSpringJoint.damper = lastModuleSpringJoint.damper;
                ballSpringJoint.minDistance = lastModuleSpringJoint.minDistance;
                ballSpringJoint.maxDistance = lastModuleSpringJoint.maxDistance;
                ballSpringJoint.tolerance = lastModuleSpringJoint.tolerance;
                ballSpringJoint.breakForce = lastModuleSpringJoint.breakForce;
                ballSpringJoint.breakTorque = lastModuleSpringJoint.breakTorque;
                ballSpringJoint.enableCollision = lastModuleSpringJoint.enableCollision;
                ballSpringJoint.connectedMassScale = lastModuleSpringJoint.connectedMassScale;
                ballSpringJoint.massScale = lastModuleSpringJoint.massScale;
                ballSpringJoint.enablePreprocessing = lastModuleSpringJoint.enablePreprocessing;
            }
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext context) => movementInput = context.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext context) => movementInput = Vector2.zero;
} 
