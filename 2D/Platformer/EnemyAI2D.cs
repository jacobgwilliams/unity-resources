using UnityEngine;

namespace UnityTemplates.Platformer2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class EnemyAI2D : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private EnemyType enemyType = EnemyType.Patrol;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private int damage = 1;
        
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private float patrolSpeed = 1f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckDistance = 0.5f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        
        public enum EnemyType
        {
            Patrol,
            Chase,
            Stationary,
            Flying
        }
        
        public enum EnemyState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Stunned
        }
        
        // Private fields
        private Rigidbody2D rb;
        private Transform player;
        private EnemyState currentState = EnemyState.Idle;
        private int currentPatrolIndex = 0;
        private float lastAttackTime;
        private float waitTimer;
        private bool facingRight = true;
        private Vector3 startPosition;
        
        // Properties
        public EnemyState CurrentState => currentState;
        public bool IsPlayerInRange => player != null && Vector2.Distance(transform.position, player.position) <= detectionRange;
        public bool IsPlayerInAttackRange => player != null && Vector2.Distance(transform.position, player.position) <= attackRange;
        
        // Events
        public System.Action OnAttack;
        public System.Action OnPlayerDetected;
        public System.Action OnPlayerLost;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
            
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = groundCheckObj.transform;
            }
        }
        
        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (enemyType == EnemyType.Patrol && patrolPoints.Length > 0)
            {
                currentState = EnemyState.Patrol;
            }
        }
        
        private void Update()
        {
            UpdateState();
            HandleAnimation();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        private void UpdateState()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;
                case EnemyState.Patrol:
                    HandlePatrolState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
                case EnemyState.Stunned:
                    HandleStunnedState();
                    break;
            }
        }
        
        private void HandleIdleState()
        {
            if (IsPlayerInRange && enemyType != EnemyType.Stationary)
            {
                currentState = EnemyState.Chase;
                OnPlayerDetected?.Invoke();
            }
        }
        
        private void HandlePatrolState()
        {
            if (IsPlayerInRange)
            {
                currentState = EnemyState.Chase;
                OnPlayerDetected?.Invoke();
                return;
            }
            
            if (patrolPoints.Length == 0) return;
            
            Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
            float distance = Vector2.Distance(transform.position, targetPoint);
            
            if (distance < 0.5f)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    waitTimer = 0f;
                }
            }
        }
        
        private void HandleChaseState()
        {
            if (!IsPlayerInRange)
            {
                currentState = enemyType == EnemyType.Patrol ? EnemyState.Patrol : EnemyState.Idle;
                OnPlayerLost?.Invoke();
                return;
            }
            
            if (IsPlayerInAttackRange)
            {
                currentState = EnemyState.Attack;
                return;
            }
        }
        
        private void HandleAttackState()
        {
            if (!IsPlayerInAttackRange)
            {
                currentState = EnemyState.Chase;
                return;
            }
            
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
        
        private void HandleStunnedState()
        {
            // Implement stun logic here
            // For now, just return to idle after a delay
            if (Time.time - lastAttackTime >= 1f)
            {
                currentState = EnemyState.Idle;
            }
        }
        
        private void HandleMovement()
        {
            if (currentState == EnemyState.Stunned) return;
            
            Vector2 movement = Vector2.zero;
            
            switch (currentState)
            {
                case EnemyState.Patrol:
                    if (patrolPoints.Length > 0)
                    {
                        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
                        movement = (targetPoint - transform.position).normalized * patrolSpeed;
                    }
                    break;
                    
                case EnemyState.Chase:
                    if (player != null)
                    {
                        movement = (player.position - transform.position).normalized * moveSpeed;
                    }
                    break;
            }
            
            // Check for ground if not flying
            if (enemyType != EnemyType.Flying && !IsGrounded() && movement.x != 0)
            {
                movement.x = 0;
            }
            
            rb.velocity = new Vector2(movement.x, rb.velocity.y);
            
            // Handle sprite flipping
            if (movement.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && facingRight)
            {
                Flip();
            }
        }
        
        private void HandleAnimation()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
                animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
                animator.SetBool("IsChasing", currentState == EnemyState.Chase);
            }
        }
        
        private void PerformAttack()
        {
            OnAttack?.Invoke();
            
            // Deal damage to player if in range
            if (player != null)
            {
                PlayerController2D playerController = player.GetComponent<PlayerController2D>();
                if (playerController != null)
                {
                    // Implement damage dealing here
                    // playerController.TakeDamage(damage);
                }
            }
        }
        
        private bool IsGrounded()
        {
            if (groundCheck == null) return true;
            
            return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayerMask);
        }
        
        private void Flip()
        {
            facingRight = !facingRight;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
        }
        
        public void Stun(float duration)
        {
            currentState = EnemyState.Stunned;
            lastAttackTime = Time.time - attackCooldown + duration;
        }
        
        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
            currentPatrolIndex = 0;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw patrol path
            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                        if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                        }
                    }
                }
            }
        }
    }
}