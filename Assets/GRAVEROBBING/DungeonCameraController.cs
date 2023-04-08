using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float jumpHeight = 2f;
    public float crouchHeight = 0.5f;
    public float mouseSensitivity = 100f;
    public float minimumVerticalAngle = -85.0f;
    public float maximumVerticalAngle = 85.0f;

    private CharacterController controller;

    private float verticalAngle = 0.0f;
    private float horizontalAngle = 0.0f;

    private bool isCrouching = false;
    private float defaultHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
    }

    void Update()
    {
        
        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                //jump
            }

            if (Input.GetButtonDown("Crouch"))
            {
                //crouch
            }
        }

        //WASD Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movementVector = new Vector3(horizontalInput, 0, verticalInput) * movementSpeed * Time.deltaTime;
        controller.Move(movementVector);

        //Mouse Looking
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        horizontalAngle += mouseX * mouseSensitivity;
        verticalAngle -= mouseY * mouseSensitivity;
        verticalAngle = Mathf.Clamp(verticalAngle, minimumVerticalAngle, maximumVerticalAngle);

        transform.localRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0.0f);

    }
}