using UnityEngine;

[DisallowMultipleComponent]
public class DungeonCameraController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float interactionRadius = 0.2f;
    [Space]
    [Header("Locomotion")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintModifier = 1.2f;
    [SerializeField] private float crouchModifier = 0.65f;
    [Space]
    [SerializeField] private float rappelSpeed;

    [HideInInspector] public bool isRappelling;
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
    [SerializeField] private bool invertFallCurve = false;
    [SerializeField] private float terminalVelocity = 55.55f;
    [SerializeField] private float terminalVelocitySpan = 3f;
    [SerializeField] private float jumpForce = 2f; //j
    [SerializeField] private float jumpDuration = 0.8f; //j
    [SerializeField] private float jumpCooldown => jumpDuration / 2; //j
    [SerializeField] private AnimationCurve jumpCurve; //j

    private float fallAirTimer = 0f;
    private float jumpAirTimer = 0f;
    private float jumpCooldownTimer = 0f;
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
    private bool IsSprinting => sprintInput && !stayCrouched && verticalInput >= 0f && IsMoving;
    private bool IsGrounded => controller.isGrounded;
    private bool CannotStand => Physics.SphereCast(transform.position + controller.center, controller.radius, Vector3.up, out RaycastHit hitInfo,
        crouchCeilingOffset + defaultHeight - controller.height / 2, ~playerLayer, QueryTriggerInteraction.Ignore);

    private bool CeilingAboveHead => Physics.Raycast(transform.position + controller.center, Vector3.up, 
        controller.height / 2 + Mathf.Abs(crouchCeilingOffset), ~playerLayer, QueryTriggerInteraction.Ignore);

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

    private bool interactInput;

    [Header("Debug")]
    [SerializeField] private Vector3 DEBUGVelocity = Vector3.zero;
    [SerializeField] private float DEBUGMoveMagnitude = 0f;
    [SerializeField] private bool DEBUGCannotStand;

    public static DungeonCameraController Instance;
    private void Awake()
    {
        Instance = this;
    }

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

        SettlePlayer();

        void SettlePlayer()
        {
            Ray downRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            Physics.Raycast(downRay, out hit, 100f, ~playerLayer, QueryTriggerInteraction.Ignore);
            controller.Move(new Vector3(0, -hit.distance, 0));
        }
    }

    void Update()
    {
        DEBUGCannotStand = CannotStand;

        InputDetection();

        Interaction();

        void InputDetection()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            mouseXInput = Input.GetAxis("Mouse X");
            mouseYInput = Input.GetAxis("Mouse Y");

            jumpInput = Input.GetButton("Jump");
            crouchInput = Input.GetButton("Crouch");
            sprintInput = Input.GetButton("Sprint");

            interactInput = Input.GetButtonDown("Interact");
        }

        void Interaction()
        {
            if(Physics.SphereCast(mainCamera.transform.position, interactionRadius, mainCamera.transform.forward, out RaycastHit hitInfo, interactionDistance, ~playerLayer, QueryTriggerInteraction.Collide))
            {
                if (hitInfo.transform.TryGetComponent(out IInteractable interactable))
                {
                    if (interactInput)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        Rappelling();

        Jumping();

        Crouching();

        Moving();

        controller.Move(new Vector3(moveDirection.x, currentFallForce, moveDirection.z) * movementSpeed * Time.deltaTime);
        DEBUGVelocity = controller.velocity;

        void Rappelling()
        {
            if (isRappelling)
            {
                //Get view angle on x rotational axis
                //if angle is above range, "w" is up and "s" is down
                //if angle is below range, the opposite is true
                //if angle is between range, nothing happens
                //apply rappel speed

                //Also implement letting go of rope with "e"
            }
        }

        void Jumping()
        {

            if (!isRappelling) jumpCooldownTimer -= Time.fixedDeltaTime;

            if (IsGrounded || isRappelling)
            {            
                currentFallForce = isRappelling ? 0f : -Mathf.Abs(groundedGravity);
                fallAirTimer = 0f;
                jumpAirTimer = 1f;
                if (jumpInput && !CeilingAboveHead && jumpCooldownTimer <= 0f)
                {
                    isRappelling = false;

                    jumpAirTimer = 0f;
                    jumpCooldownTimer = jumpCooldown;
                    currentFallForce = jumpCurve.Evaluate(jumpAirTimer);
                }
            }
            else
            {
                if (isRappelling)
                {
                    currentFallForce = 0f;
                    return;
                }

                if (jumpAirTimer < 1)
                {
                    if (CeilingAboveHead)
                    {
                        jumpAirTimer = 1f;
                    }
                    currentFallForce = jumpCurve.Evaluate(jumpAirTimer) * jumpForce;
                    jumpAirTimer = Mathf.Clamp01(jumpAirTimer + Time.fixedDeltaTime / jumpDuration);
                }
                else
                {
                    currentFallForce = -gravityFallCurve.Evaluate(invertFallCurve ? Mathf.Abs(fallAirTimer - 1) : fallAirTimer) * terminalVelocity;
                    fallAirTimer = Mathf.Clamp01(fallAirTimer + Time.fixedDeltaTime / terminalVelocitySpan);
                }
            }
            //j We may not need two different timers, Rhys recommends just using the one currentFallForce equation and accounting for jumping in it. 
            //We can do this by having an if statement that asks if the jump timer is not yet 0; if it isn't, we apply the jump curve instead of the
            //fall curve, or perhaps just use a modifier that makes the evaluate output positive.

            // Somewhere in here we'll also have to implement Sliding down surfaces. 
            // Make a ray that goes from transform downward
            // get ray info, specifically hit.normal
            // compare that vector to Vector3.up
            // If it's more severe than a determined slope limit, 
            // use a planar vector (quarternions ughhh) that is perpendicular to the slope,
            // Apply that vector to the player using Controller.Move.

            //ONCE WE HAVE THIS, WE CAN USE IT FOR ALL SLOPE FIXING, INCLUDING REGULAR WALKING! - NO MORE STATIC GRAVITY APPLICATION

        }

        void Crouching()
        {
            if (stayCrouched && CannotStand) return;

            float targetHeight;

            if (crouchInput && IsGrounded && !isRappelling)
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

        void Moving()
        {
            Vector3 forward = Vector3.Normalize(new Vector3(transform.forward.x, 0f, transform.forward.z));
            moveDirection = Vector3.Normalize(forward * verticalInput + transform.right * horizontalInput);
            if (stayCrouched) moveDirection = moveDirection * crouchModifier;
            else if (IsSprinting) moveDirection = moveDirection * sprintModifier;

            DEBUGMoveMagnitude = moveDirection.magnitude;
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