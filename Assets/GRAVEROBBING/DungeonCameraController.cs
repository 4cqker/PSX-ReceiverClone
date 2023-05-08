using System;
using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    [Header("Locomotion")]
    public float movementSpeed = 5f;
    public float sprintModifier = 1.2f;
    public float crouchModifier = 0.65f;
    [Space]
    public float crouchCameraDrop = 1f;
    public float crouchHeight = 1f;
    public float crouchCeilingOffset = 0.1f;
    public float crouchSpeed = 10f;
    private float defaultHeight;
    private LayerMask playerLayer;
    private bool stayCrouched = false;
    [Space]
    public float mouseSensitivity = 100f;
    public float minVerticalLookAngle = -80.0f;
    public float maxVerticalLookAngle = 80.0f;
    [Space]
    public float groundedGravity = 9.81f;
    public AnimationCurve gravityFallCurve;
    public bool invertFallCurve;
    public float terminalVelocity = 55.55f;
    public float terminalVelocitySpan = 3f;
    public float jumpForce = 2f;
    [Space]
    [Header("Camera")]
    public float headBobAmplitude = 1.3f;
    public float headBobFrequency = 1.3f;
    public float headBobReturnSpeed = 0.1f;
    public bool enableHeadbob = true;
    private float headBobAmount = 0f;
    private float headBobStopwatch = 0f;
    [Space]
    public float cameraTiltAngle = 15f;
    public float cameraTiltSpeed = 0.1f;
    public bool enableCameraTilt = true;
    [Space]
    public float defaultFOV = 75f;
    public float sprintFOV = 90f;
    public float FOVChangeSpeed = 0.1f;
    public bool enableFOVChange = true;

    private CharacterController controller;
    private Camera mainCamera;

    private bool IsMoving => moveDirection.magnitude > 0.001f;
    private bool IsSprinting => sprintInput && !stayCrouched && verticalInput >= 0f && IsGrounded && IsMoving;
    private bool IsCrouching => crouchInput && IsGrounded;
        
    private bool IsGrounded => controller.isGrounded;
    private bool CannotStand => Physics.SphereCast(transform.position + controller.center, controller.radius, Vector3.up, out RaycastHit hitInfo,
        crouchCeilingOffset + defaultHeight - controller.height / 2, ~playerLayer, QueryTriggerInteraction.Ignore);

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;
    private Vector3 moveDirection;
    private Transform tiltHandler;
    private Transform bobHandler;
    private Transform crouchHandler;
    private Vector3 initialCrouchHeight;
    private Vector3 initialHeadBobHeight;

    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;

    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;

    private float currentFallForce;

    [Header("Debug")]
    public Vector3 DEBUGVelocity = Vector3.zero;
    public float DEBUGMoveMagnitude = 0f;
    public float DEBUGAirTimer = 0f;
    public bool DEBUGCannotStand;


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

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

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

                DEBUGAirTimer = 0f;
            }
            else
            {              
                currentFallForce = -gravityFallCurve.Evaluate(invertFallCurve ? Mathf.Abs(DEBUGAirTimer - 1) : DEBUGAirTimer) * terminalVelocity;

                DEBUGAirTimer = Mathf.Clamp01(DEBUGAirTimer + Time.deltaTime / terminalVelocitySpan);
            }
        }

        void Crouching()
        {
            if (stayCrouched && CannotStand) return;

            float targetHeight;

            if (IsCrouching)
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
            horizontalAngle += mouseX * mouseSensitivity;
            verticalAngle -= mouseY * mouseSensitivity;
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