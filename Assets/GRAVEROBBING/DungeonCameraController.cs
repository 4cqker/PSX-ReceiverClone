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
    public float grav = -4f;
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
    public float velocity;

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
        if (controller.isGrounded)
        {
            airTimer = 0f;
            if (Input.GetButtonDown("Jump"))
            {
                //Jump
            }
        }
        else
        {
            airTimer += 0.1f;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            //crouch
        }

        Movement();

        MouseLooking();

        HeadBobbing();

        CameraRoll();

        velocity = controller.velocity.magnitude;

        void Movement()
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 forward = Vector3.Normalize(new Vector3(transform.forward.x, 0f, transform.forward.z));
            moveDirection = Vector3.Normalize(forward * verticalInput + transform.right * horizontalInput) * movementSpeed * Time.deltaTime;
            controller.Move(moveDirection);
        }

        void MouseLooking()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            horizontalAngle += mouseX * mouseSensitivity;
            verticalAngle -= mouseY * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalLookAngle, maxVerticalLookAngle);
            transform.localRotation = Quaternion.Euler(verticalAngle, horizontalAngle, transform.localRotation.z);
        }

        void HeadBobbing()
        {
            if (moveDirection.magnitude > Mathf.Epsilon)
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
        //todo I don't think this is working properly
        controller.Move(new Vector3(0, grav * airTimer * Time.deltaTime, 0));
    }
}