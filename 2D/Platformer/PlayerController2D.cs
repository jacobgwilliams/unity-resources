using UnityEngine;

namespace UnityTemplates.Platformer2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float coyoteTime = 0.2f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // Private fields
        private Rigidbody2D rb;
        private float horizontalInput;
        private bool isGrounded;
        private float lastGroundedTime;
        private float lastJumpTime;
        private bool facingRight = true;
        
        // Properties
        public bool IsGrounded => isGrounded;
        public float MoveSpeed => moveSpeed;
        public bool FacingRight => facingRight;
        
        // Events
        public System.Action OnJump;
        public System.Action OnLand;
        public System.Action<float> OnMove;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = groundCheckObj.transform;
            }
        }
        
        private void Update()
        {
            HandleInput();
            CheckGrounded();
            HandleJump();
            HandleAnimation();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        private void HandleInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            
            if (Input.GetButtonDown("Jump"))
            {
                lastJumpTime = Time.time;
            }
        }
        
        private void CheckGrounded()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
            
            if (isGrounded)
            {
                lastGroundedTime = Time.time;
                if (!wasGrounded)
                {
                    OnLand?.Invoke();
                }
            }
        }
        
        private void HandleJump()
        {
            bool canJump = isGrounded || (Time.time - lastGroundedTime <= coyoteTime);
            bool jumpPressed = Time.time - lastJumpTime <= jumpBufferTime;
            
            if (jumpPressed && canJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                lastJumpTime = 0;
                OnJump?.Invoke();
            }
        }
        
        private void HandleMovement()
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
            OnMove?.Invoke(horizontalInput);
            
            // Handle sprite flipping
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
        }
        
        private void HandleAnimation()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetFloat("VerticalVelocity", rb.velocity.y);
            }
        }
        
        private void Flip()
        {
            facingRight = !facingRight;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}