using UnityEngine;

namespace UnityTemplates.TopDownRPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerControllerTopDown : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runMultiplier = 1.5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private string walkAnimationName = "Walk";
        [SerializeField] private string idleAnimationName = "Idle";
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private float footstepInterval = 0.5f;
        
        // Private fields
        private Rigidbody2D rb;
        private Vector2 inputVector;
        private Vector2 currentVelocity;
        private bool isRunning;
        private float lastFootstepTime;
        private AudioSource audioSource;
        
        // Properties
        public Vector2 InputVector => inputVector;
        public bool IsMoving => inputVector.magnitude > 0.1f;
        public bool IsRunning => isRunning;
        public float CurrentSpeed => currentVelocity.magnitude;
        
        // Events
        public System.Action<Vector2> OnMove;
        public System.Action OnStartMoving;
        public System.Action OnStopMoving;
        public System.Action OnStartRunning;
        public System.Action OnStopRunning;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Update()
        {
            HandleInput();
            HandleAnimation();
            HandleFootsteps();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        private void HandleInput()
        {
            inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            // Notify events
            if (IsMoving)
            {
                OnMove?.Invoke(inputVector);
                if (!IsMoving)
                {
                    OnStartMoving?.Invoke();
                }
            }
            else if (IsMoving)
            {
                OnStopMoving?.Invoke();
            }
            
            if (isRunning && IsMoving)
            {
                OnStartRunning?.Invoke();
            }
            else if (!isRunning)
            {
                OnStopRunning?.Invoke();
            }
        }
        
        private void HandleMovement()
        {
            float targetSpeed = moveSpeed * (isRunning ? runMultiplier : 1f);
            Vector2 targetVelocity = inputVector * targetSpeed;
            
            // Smooth acceleration/deceleration
            if (inputVector.magnitude > 0.1f)
            {
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }
            
            rb.velocity = currentVelocity;
        }
        
        private void HandleAnimation()
        {
            if (animator == null) return;
            
            bool wasMoving = animator.GetBool("IsMoving");
            bool isMoving = IsMoving;
            
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning && isMoving);
            animator.SetFloat("Speed", currentVelocity.magnitude);
            
            // Set direction for 8-directional movement
            if (isMoving)
            {
                float angle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;
                animator.SetFloat("Direction", angle);
                
                // Flip sprite based on horizontal movement
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = inputVector.x < 0;
                }
            }
        }
        
        private void HandleFootsteps()
        {
            if (!IsMoving || footstepSounds.Length == 0) return;
            
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                PlayFootstepSound();
                lastFootstepTime = Time.time;
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
            moveSpeed = newSpeed;
        }
        
        public void SetRunMultiplier(float multiplier)
        {
            runMultiplier = multiplier;
        }
        
        public void StopMovement()
        {
            inputVector = Vector2.zero;
            currentVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
        
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            rb.velocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
    }
}