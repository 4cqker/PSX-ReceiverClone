using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    public float movementSpeed = 5f;
    [Space]
    public float crouchHeight = 0.5f;
    [Space]
    public float mouseSensitivity = 100f;
    public float minVerticalLookAngle = -80.0f;
    public float maxVerticalLookAngle = 80.0f;
    [Space]
    public float gravity = 9.81f;
    public float terminalVelocity = 55.55f;
    public float jumpForce = 2f;
    [Space]
    [Header("Game Feel")]
    public float headBobAmplitude = 1.3f;
    public float headBobFrequency = 1.3f;
    private float headBobAmount = 0f;
    [Space]
    public float cameraTiltAngle = 15f;
    public float cameraTiltSpeed = 0.1f;

    private CharacterController controller;
    private Transform camTransform;

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;
    private Vector3 originalPosition;
    private Vector3 moveDirection;
    private Transform tiltHandler;
    private Transform bobHandler;

    private bool isCrouching = false;
    private float defaultHeight;
    private float verticalVelocity = 0f;
    private float rollVelocity = 0f;
    private float airTimer = 0f;

    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;
    private bool jumpInput;

    private float currentFallForce;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camTransform = GetComponentInChildren<Camera>().transform;
        defaultHeight = controller.height;
        originalPosition = camTransform.localPosition;
        tiltHandler = camTransform.parent;
        bobHandler = tiltHandler.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        InputDetection();

        MouseLooking();

        HeadBobbing();

        CameraRoll();

        void InputDetection()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            jumpInput = Input.GetButton("Jump");
        }

        void MouseLooking()
        {
            horizontalAngle += mouseX * mouseSensitivity;
            verticalAngle -= mouseY * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalLookAngle, maxVerticalLookAngle);
            transform.localRotation = Quaternion.Euler(verticalAngle, horizontalAngle, transform.localRotation.z);
        }

        void HeadBobbing()
        {
            if (controller.isGrounded && moveDirection.magnitude > Mathf.Epsilon)
                headBobAmount = Mathf.Sin(Time.time * headBobFrequency) * headBobAmplitude - headBobAmplitude;
            bobHandler.localPosition = new Vector3(0f, headBobAmount, 0f);
        }

        void CameraRoll()
        {
            Quaternion targetRoll = Quaternion.Euler(new Vector3(
                tiltHandler.eulerAngles.x,
                tiltHandler.eulerAngles.y,
                Input.GetAxisRaw("Horizontal") * -cameraTiltAngle));

            tiltHandler.rotation = Quaternion.Lerp(tiltHandler.rotation, targetRoll, Time.deltaTime / cameraTiltSpeed);
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
        }

        void Jumping()
        {
            currentFallForce = controller.isGrounded ? 0f : Mathf.Clamp(currentFallForce - Mathf.Abs(gravity) * Time.deltaTime, -Mathf.Abs(terminalVelocity), 0f);
        }

        controller.Move(new Vector3(moveDirection.x, currentFallForce, moveDirection.z) * movementSpeed * Time.deltaTime);
    }
}