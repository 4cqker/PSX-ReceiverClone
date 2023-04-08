using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    public float movementSpeed = 5f;
    
    public float crouchHeight = 0.5f;
    public float mouseSensitivity = 100f;
    public float minimumVerticalAngle = -80.0f;
    public float maximumVerticalAngle = 80.0f;

    public float headBobModifier = 5f;
    public float headBobAmplifier = 1.3f;
    public float headBobSpeed = 1.3f;
    public float bobResetSpeed = 0.2f;
    public float grav = -4f;
    public float jumpForce = 2f;

    private CharacterController controller;

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;
    private Vector3 originalPosition;
    private GameObject rollHandler;

    private bool isCrouching = false;
    private float defaultHeight;
    private float verticalVelocity = 0f;
    private float rollVelocity = 0f;
    private float airTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
        originalPosition = Camera.main.transform.localPosition;
        rollHandler = Camera.main.transform.parent.gameObject;
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

        //WASD Movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");


        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward = forward.normalized;
        Vector3 moveDirection = forward * verticalInput + transform.right * horizontalInput;
        moveDirection = moveDirection.normalized * movementSpeed * Time.deltaTime;
        controller.Move(moveDirection);

        //Mouse Looking
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        horizontalAngle += mouseX * mouseSensitivity;
        verticalAngle -= mouseY * mouseSensitivity;
        verticalAngle = Mathf.Clamp(verticalAngle, minimumVerticalAngle, maximumVerticalAngle);
        transform.localRotation = Quaternion.Euler(verticalAngle, horizontalAngle, transform.localRotation.z);

        //Headbobbing
        HeadBobbing(headBobSpeed, headBobAmplifier, moveDirection.magnitude * headBobModifier, Camera.main.transform);
        if (moveDirection.magnitude < 0.0001f)
        {
            //Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, originalPosition, Time.deltaTime * bobResetSpeed);


            Vector3 currentVelocity = Vector3.zero;
            Camera.main.transform.localPosition = Vector3.SmoothDamp(Camera.main.transform.localPosition, originalPosition, ref currentVelocity, bobResetSpeed);
        }
        //Camera Tilting is broken when turning Left. Bug fix if you want the feature in @RHYS
        //UpdateCameraRoll(rollHandler, 6f, 0.2f);

    }
    
    private void FixedUpdate()
    {
        controller.Move(new Vector3(0, grav * airTimer * Time.deltaTime, 0));
    }

    void HeadBobbing(float bobbingFrequency, float bobbingAmplitude, float movementSpeed, Transform cam)
    {
        float bobbingAmount = Mathf.Sin(Time.time * 2 * Mathf.PI * bobbingFrequency) * bobbingAmplitude;
        Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y + bobbingAmount, originalPosition.z);
        cam.localPosition = Vector3.Lerp(cam.localPosition, newPosition, Time.deltaTime * movementSpeed);
    }

    void UpdateCameraRoll(GameObject rollHandler, float maxRollAngle, float smoothTime)
    {
        float targetRoll = Mathf.Clamp(Input.GetAxis("Mouse X") * 2f, -maxRollAngle, maxRollAngle);
        float currentRoll = Mathf.SmoothDamp(rollHandler.transform.eulerAngles.z, targetRoll, ref rollVelocity, smoothTime);
        rollHandler.transform.eulerAngles = new Vector3(rollHandler.transform.eulerAngles.x, rollHandler.transform.eulerAngles.y, currentRoll);
    }
}