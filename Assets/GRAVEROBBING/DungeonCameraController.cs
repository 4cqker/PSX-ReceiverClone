using System;
using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    [Header("Locomotion")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintModifier = 1.2f;
    [SerializeField] private float crouchModifier = 0.65f;
    [Space]
    [SerializeField] private float crouchCameraDrop = 1f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCeilingOffset = 0.1f;
    [SerializeField] private float crouchSpeed = 10f;
    
    private float defaultHeight;
    private LayerMask playerLayer;
    private bool stayCrouched = false;
    [Space]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minVerticalLookAngle = -80.0f;
    [SerializeField] private float maxVerticalLookAngle = 80.0f;
    [Space]
    [SerializeField] private float groundedGravity = 9.81f;
    [SerializeField] private AnimationCurve gravityFallCurve;
    [SerializeField] private bool invertFallCurve;
    [SerializeField] private float terminalVelocity = 55.55f;
    [SerializeField] private float terminalVelocitySpan = 3f;
    [SerializeField] private float jumpForce = 2f; //j
    [SerializeField] private float jumpDuration = 0.8f; //j
    [SerializeField] private AnimationCurve jumpCurve; //j

    private float fallAirTimer = 0f;
    private float jumpAirTimer = 0f;
    [Space]
    [Header("Camera")]
    [SerializeField] private float headBobAmplitude = 1.3f;
    [SerializeField] private float headBobFrequency = 1.3f;
    [SerializeField] private float headBobReturnSpeed = 0.1f;
    [SerializeField] private bool enableHeadbob = true;

    private float headBobAmount = 0f;
    private float headBobStopwatch = 0f;
    [Space]
    [SerializeField] private float cameraTiltAngle = 15f;
    [SerializeField] private float cameraTiltSpeed = 0.1f;
    [SerializeField] private bool enableCameraTilt = true;
    [Space]
    [SerializeField] private float defaultFOV = 75f;
    [SerializeField] private float sprintFOV = 90f;
    [SerializeField] private float FOVChangeSpeed = 0.1f;
    [SerializeField] private bool enableFOVChange = true;

    private bool IsMoving => moveDirection.magnitude > 0.001f;
    private bool IsSprinting => sprintInput && !stayCrouched && verticalInput >= 0f && IsGrounded && IsMoving;
    private bool IsGrounded => controller.isGrounded;
    private bool CannotStand => Physics.SphereCast(transform.position + controller.center, controller.radius, Vector3.up, out RaycastHit hitInfo,
        crouchCeilingOffset + defaultHeight - controller.height / 2, ~playerLayer, QueryTriggerInteraction.Ignore);

    private CharacterController controller;
    private Camera mainCamera;
    private Transform tiltHandler;
    private Transform bobHandler;
    private Transform crouchHandler;
    private Vector3 initialCrouchHeight;
    private Vector3 initialHeadBobHeight;

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;
    private float currentFallForce;
    private Vector3 moveDirection;

    private float horizontalInput;
    private float verticalInput;
    private float mouseXInput;
    private float mouseYInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;

    [Header("Debug")]
    [SerializeField] private Vector3 DEBUGVelocity = Vector3.zero;
    [SerializeField] private float DEBUGMoveMagnitude = 0f;
    [SerializeField] private bool DEBUGCannotStand;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
        mainCamera = Camera.main;
        tiltHandler = mainCamera.transform.parent;
        bobHandler = tiltHandler.parent;
        crouchHandler = bobHandler.parent;
        initialCrouchHeight = crouchHandler.transform.localPosition;
        initialHeadBobHeight = bobHandler.localPosition;

        playerLayer = LayerMask.GetMask("Player");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        DEBUGCannotStand = CannotStand;

        InputDetection();

        void InputDetection()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            mouseXInput = Input.GetAxis("Mouse X");
            mouseYInput = Input.GetAxis("Mouse Y");

            jumpInput = Input.GetButton("Jump");
            crouchInput = Input.GetButton("Crouch");
            sprintInput = Input.GetButton("Sprint");
        }
    }

    private void FixedUpdate()
    {
        Moving();

        Jumping();

        Crouching();

        controller.Move(new Vector3(moveDirection.x, currentFallForce, moveDirection.z) * movementSpeed * Time.deltaTime);
        DEBUGVelocity = controller.velocity;

        void Moving()
        {
            Vector3 forward = Vector3.Normalize(new Vector3(transform.forward.x, 0f, transform.forward.z));
            moveDirection = Vector3.Normalize(forward * verticalInput + transform.right * horizontalInput);
            if (stayCrouched) moveDirection = moveDirection * crouchModifier;
            else if (IsSprinting) moveDirection = moveDirection * sprintModifier;

            DEBUGMoveMagnitude = moveDirection.magnitude;
        }

        void Jumping()
        {
            if (IsGrounded)
            {            
                currentFallForce = -Mathf.Abs(groundedGravity);
                //jumpAirTimer = jumpInput ? jumpDuration : 0f; //j
                fallAirTimer = 0f;
            }
            /*else if (jumpAirTimer > 0.02f) //j
            { 
                currentFallForce = jumpCurve.Evaluate(jumpAirTimer) * jumpForce; //j
                jumpAirTimer -= Time.fixedDeltaTime; //j
                if (IsGrounded) jumpAirTimer = 0f; //j
            }*/
            else
            {
                currentFallForce = -gravityFallCurve.Evaluate(invertFallCurve ? Mathf.Abs(fallAirTimer - 1) : fallAirTimer) * terminalVelocity;
                fallAirTimer = Mathf.Clamp01(fallAirTimer + Time.deltaTime / terminalVelocitySpan);
            }
            //j We may not need two different timers, Rhys recommends just using the one currentFallForce equation and accounting for jumping in it. 
            //We can do this by having an if statement that asks if the jump timer is not yet 0; if it isn't, we apply the jump curve instead of the
            //fall curve, or perhaps just use a modifier that makes the evaluate output positive.
        }

        void Crouching()
        {
            if (stayCrouched && CannotStand) return;

            float targetHeight;

            if (crouchInput && IsGrounded)
            {
                targetHeight = crouchHeight;
                stayCrouched = true;
            }
            else
            {
                targetHeight = defaultHeight;
                stayCrouched = false;
            }

            controller.height = targetHeight;
            controller.center = Vector3.up * targetHeight / 2;

            controller.Move(Vector3.down * controller.minMoveDistance);
        }
    }

    private void LateUpdate()
    {
        MouseLooking();

        HeadBobbing();

        CameraTilt();

        CrouchCamera();

        FieldOfView();

        void MouseLooking()
        {
            horizontalAngle += mouseXInput * mouseSensitivity;
            verticalAngle -= mouseYInput * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalLookAngle, maxVerticalLookAngle);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, horizontalAngle, transform.localRotation.z);
            bobHandler.localRotation = Quaternion.Euler(verticalAngle, bobHandler.localRotation.y, bobHandler.localRotation.z);
        }

        void HeadBobbing()
        {
            if (!enableHeadbob)
            {
                bobHandler.localPosition = initialHeadBobHeight;
                return;
            }

            if (IsGrounded && IsMoving)
            {
                headBobStopwatch += Time.deltaTime * moveDirection.magnitude;
                float phaseOffset = 2f * Mathf.PI * headBobFrequency * headBobStopwatch;
                headBobAmount = Mathf.Cos(phaseOffset) * headBobAmplitude - headBobAmplitude;
            }
            else if (headBobAmount < -0.001f)
            {
                headBobStopwatch = 0f;
                headBobAmount = Mathf.Lerp(headBobAmount, 0f, Time.deltaTime / headBobReturnSpeed);
            }
            else
            {
                headBobStopwatch = 0f;
                headBobAmount = 0f;
            }

            bobHandler.localPosition = new Vector3(0f, headBobAmount, 0f) + initialHeadBobHeight;
        }

        void CameraTilt()
        {
            if (!enableCameraTilt)
            {
                tiltHandler.rotation = bobHandler.rotation;
                return;
            }

            Quaternion targetTilt = Quaternion.Euler(new Vector3(
                tiltHandler.eulerAngles.x,
                tiltHandler.eulerAngles.y,
                Input.GetAxisRaw("Horizontal") * -cameraTiltAngle));

            tiltHandler.rotation = Quaternion.Lerp(tiltHandler.rotation, targetTilt, Time.deltaTime / cameraTiltSpeed);
        }

        void CrouchCamera()
        {
            Vector3 targetPosition;
            if (stayCrouched)
            {
                targetPosition = initialCrouchHeight - (crouchCameraDrop * Vector3.down);
            }
            else
            {
                targetPosition = initialCrouchHeight;
            }
            
            crouchHandler.localPosition = Vector3.Lerp(crouchHandler.localPosition, targetPosition, crouchSpeed * Time.deltaTime);

            if (Vector3.Distance(crouchHandler.localPosition, targetPosition) < 0.001f)
                crouchHandler.localPosition = targetPosition;
        }

        void FieldOfView()
        {
            if (!enableFOVChange)
            {
                mainCamera.fieldOfView = defaultFOV;
                return;
            }

            float targetFOV = IsSprinting ? sprintFOV : defaultFOV;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime / FOVChangeSpeed);
        }
    }   
}