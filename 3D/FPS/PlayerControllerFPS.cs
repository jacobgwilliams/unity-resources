using UnityEngine;

namespace UnityTemplates.FPS3D
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerControllerFPS : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = 1;
        
        [Header("Mouse Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private bool invertY = false;
        
        [Header("Crouching")]
        [SerializeField] private bool canCrouch = true;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float crouchTransitionSpeed = 5f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private AudioClip[] jumpSounds;
        [SerializeField] private AudioClip[] landSounds;
        [SerializeField] private float footstepInterval = 0.5f;
        
        // Private fields
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isRunning;
        private bool isCrouching;
        private float xRotation = 0f;
        private float originalHeight;
        private float lastFootstepTime;
        private AudioSource audioSource;
        
        // Input
        private float mouseX, mouseY;
        private float horizontal, vertical;
        private bool jumpInput;
        private bool runInput;
        private bool crouchInput;
        
        // Properties
        public bool IsGrounded => isGrounded;
        public bool IsRunning => isRunning;
        public bool IsCrouching => isCrouching;
        public float CurrentSpeed => controller.velocity.magnitude;
        public Vector3 Velocity => controller.velocity;
        
        // Events
        public System.Action OnJump;
        public System.Action OnLand;
        public System.Action OnStartCrouch;
        public System.Action OnStopCrouch;
        public System.Action OnStartRunning;
        public System.Action OnStopRunning;
        
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }
            
            originalHeight = controller.height;
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void Update()
        {
            HandleInput();
            HandleMouseLook();
            CheckGrounded();
            HandleJump();
            HandleCrouch();
            HandleFootsteps();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        private void HandleInput()
        {
            // Movement input
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            jumpInput = Input.GetButtonDown("Jump");
            runInput = Input.GetKey(KeyCode.LeftShift);
            crouchInput = Input.GetKey(KeyCode.LeftControl);
            
            // Mouse input
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            if (invertY)
            {
                mouseY = -mouseY;
            }
            
            // Cursor unlock
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                    CursorLockMode.None : CursorLockMode.Locked;
            }
        }
        
        private void HandleMouseLook()
        {
            if (cameraTransform == null) return;
            
            // Rotate player horizontally
            transform.Rotate(Vector3.up * mouseX);
            
            // Rotate camera vertically
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        private void CheckGrounded()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small negative value to keep grounded
            }
            
            if (isGrounded && !wasGrounded)
            {
                OnLand?.Invoke();
                PlayLandSound();
            }
        }
        
        private void HandleJump()
        {
            if (jumpInput && isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                OnJump?.Invoke();
                PlayJumpSound();
            }
        }
        
        private void HandleCrouch()
        {
            if (!canCrouch) return;
            
            bool wasCrouching = isCrouching;
            isCrouching = crouchInput;
            
            if (isCrouching && !wasCrouching)
            {
                OnStartCrouch?.Invoke();
            }
            else if (!isCrouching && wasCrouching)
            {
                OnStopCrouch?.Invoke();
            }
            
            // Smoothly transition crouch height
            float targetHeight = isCrouching ? crouchHeight : originalHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        }
        
        private void HandleMovement()
        {
            // Calculate movement direction
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            
            // Determine speed based on state
            float currentSpeed = walkSpeed;
            if (isRunning && !isCrouching)
            {
                currentSpeed = runSpeed;
                if (!isRunning)
                {
                    OnStartRunning?.Invoke();
                }
            }
            else if (isRunning)
            {
                OnStopRunning?.Invoke();
            }
            else if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            
            // Apply movement
            controller.Move(move * currentSpeed * Time.deltaTime);
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        private void HandleFootsteps()
        {
            if (!isGrounded || footstepSounds.Length == 0) return;
            
            Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            if (horizontalVelocity.magnitude > 0.1f)
            {
                if (Time.time - lastFootstepTime >= footstepInterval)
                {
                    PlayFootstepSound();
                    lastFootstepTime = Time.time;
                }
            }
        }
        
        private void PlayFootstepSound()
        {
            if (audioSource != null && footstepSounds.Length > 0)
            {
                AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(randomFootstep);
            }
        }
        
        private void PlayJumpSound()
        {
            if (audioSource != null && jumpSounds.Length > 0)
            {
                AudioClip randomJump = jumpSounds[Random.Range(0, jumpSounds.Length)];
                audioSource.PlayOneShot(randomJump);
            }
        }
        
        private void PlayLandSound()
        {
            if (audioSource != null && landSounds.Length > 0)
            {
                AudioClip randomLand = landSounds[Random.Range(0, landSounds.Length)];
                audioSource.PlayOneShot(randomLand);
            }
        }
        
        public void SetMoveSpeed(float newSpeed)
        {
            walkSpeed = newSpeed;
        }
        
        public void SetRunSpeed(float newSpeed)
        {
            runSpeed = newSpeed;
        }
        
        public void SetJumpHeight(float newHeight)
        {
            jumpHeight = newHeight;
        }
        
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        public void TeleportTo(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
        }
        
        public void AddForce(Vector3 force)
        {
            velocity += force;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
        }
    }
}