using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    [Header("Locomotion")]
    public float movementSpeed = 5f;
    public float sprintModifier = 1.2f;
    [Space]
    public float crouchHeight = 0.5f;
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
    public bool enableHeadbob = true;
    private float headBobAmount = 0f;
    [Space]
    public float cameraTiltAngle = 15f;
    public float cameraTiltSpeed = 0.1f;
    public bool enableCameraTilt = true;
    [Space]
    public float defaultCamFOV = 75f;
    public float sprintingCamFOV = 90f;
    public bool enableFOVChange = true;

    private CharacterController controller;
    private Transform camTransform;

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;
    private Vector3 moveDirection;
    private Transform tiltHandler;
    private Transform bobHandler;

    private bool isCrouching = false;
    private float defaultHeight;
    private float verticalVelocity = 0f;
    private float rollVelocity = 0f;


    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;

    private bool jumpInput;
    private bool sprintInput;

    private float currentFallForce;
    private Vector3 verticalVector;

    [Header("Debug")]
    public float DEBUGFallVelocity = 0f;
    public float DEBUGAirTimer = 0f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        camTransform = GetComponentInChildren<Camera>().transform;
        defaultHeight = controller.height;
        tiltHandler = camTransform.parent;
        bobHandler = tiltHandler.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        InputDetection();

        void InputDetection()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            jumpInput = Input.GetButton("Jump");

            sprintInput = Input.GetButton("Sprint");
        }
    }

    private void FixedUpdate()
    {
        Moving();

        Jumping();

        void Moving()
        {
                Vector3 forward = Vector3.Normalize(new Vector3(transform.forward.x, 0f, transform.forward.z));
                moveDirection = Vector3.Normalize(forward * verticalInput + transform.right * horizontalInput);
                if (sprintInput) moveDirection = moveDirection * sprintModifier;
        }

        void Jumping()
        {
            if (controller.isGrounded)
            {
                DEBUGAirTimer = 0f;
                currentFallForce = -Mathf.Abs(groundedGravity);
            }
            else
            {
                DEBUGAirTimer = Mathf.Clamp01(DEBUGAirTimer + Time.deltaTime / terminalVelocitySpan);
                currentFallForce = -gravityFallCurve.Evaluate(invertFallCurve ? Mathf.Abs(DEBUGAirTimer - 1) : DEBUGAirTimer) * terminalVelocity;
            }
        }

        controller.Move(new Vector3(moveDirection.x, currentFallForce, moveDirection.z) * movementSpeed * Time.deltaTime);
        DEBUGFallVelocity = verticalVector.y;
    }

    private void LateUpdate()
    {
        MouseLooking();

        if (enableHeadbob) HeadBobbing();

        if (enableCameraTilt) CameraTilt();

        void MouseLooking()
        {
            horizontalAngle += mouseX * mouseSensitivity;
            verticalAngle -= mouseY * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalLookAngle, maxVerticalLookAngle);
            transform.localRotation = Quaternion.Euler(verticalAngle, horizontalAngle, transform.localRotation.z);
        }

        void HeadBobbing()
        {
            if (!enableHeadbob)
            {
                bobHandler.localPosition = Vector3.zero;
                return;
            }

            if (controller.isGrounded && moveDirection.magnitude > Mathf.Epsilon || headBobAmount < -headBobAmplitude / 100f)
            {
                headBobAmount = Mathf.Sin(Time.time * headBobFrequency) * headBobAmplitude - headBobAmplitude;
            }
            else
            {
                headBobAmount = 0f;
            }

            bobHandler.localPosition = new Vector3(0f, headBobAmount, 0f);
        }

        void CameraTilt()
        {
            if (!enableCameraTilt)
            {
                tiltHandler.rotation = Quaternion.identity;
                return;
            }

            Quaternion targetTilt = Quaternion.Euler(new Vector3(
                tiltHandler.eulerAngles.x,
                tiltHandler.eulerAngles.y,
                Input.GetAxisRaw("Horizontal") * -cameraTiltAngle));

            tiltHandler.rotation = Quaternion.Lerp(tiltHandler.rotation, targetTilt, Time.deltaTime / cameraTiltSpeed);
        }
    }
}