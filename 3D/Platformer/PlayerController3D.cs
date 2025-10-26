using UnityEngine;

namespace UnityTemplates.Platformer3D
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = 1;
        
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string isGroundedParameter = "IsGrounded";
        [SerializeField] private string jumpTrigger = "Jump";
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private float footstepInterval = 0.5f;
        
        // Private fields
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isRunning;
        private float xRotation = 0f;
        private float lastFootstepTime;
        private AudioSource audioSource;
        
        // Input
        private float mouseX, mouseY;
        private float horizontal, vertical;
        
        // Properties
        public bool IsGrounded => isGrounded;
        public bool IsRunning => isRunning;
        public float CurrentSpeed => controller.velocity.magnitude;
        public Vector3 Velocity => controller.velocity;
        
        // Events
        public System.Action OnJump;
        public System.Action OnLand;
        public System.Action<float> OnMove;
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
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void Update()
        {
            HandleInput();
            HandleMouseLook();
            CheckGrounded();
            HandleJump();
            HandleAnimation();
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
            isRunning = Input.GetKey(KeyCode.LeftShift);
            
            // Mouse input
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
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
            }
        }
        
        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                OnJump?.Invoke();
                
                if (animator != null)
                {
                    animator.SetTrigger(jumpTrigger);
                }
            }
        }
        
        private void HandleMovement()
        {
            // Calculate movement direction
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Apply movement
            controller.Move(move * currentSpeed * Time.deltaTime);
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            
            // Notify movement
            if (move.magnitude > 0.1f)
            {
                OnMove?.Invoke(move.magnitude);
            }
        }
        
        private void HandleAnimation()
        {
            if (animator == null) return;
            
            Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            float speed = horizontalVelocity.magnitude;
            
            animator.SetFloat(speedParameter, speed);
            animator.SetBool(isGroundedParameter, isGrounded);
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