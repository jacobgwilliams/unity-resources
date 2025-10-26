using UnityEngine;

namespace UnityTemplates.Platformer3D
{
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
        [SerializeField] private bool followTarget = true;
        
        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 2f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private bool smoothFollow = true;
        [SerializeField] private bool smoothRotation = true;
        
        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = false;
        [SerializeField] private float lookAheadDistance = 3f;
        [SerializeField] private float lookAheadSpeed = 2f;
        [SerializeField] private Vector3 lookAheadOffset = Vector3.zero;
        
        [Header("Collision")]
        [SerializeField] private bool useCollision = true;
        [SerializeField] private LayerMask collisionMask = 1;
        [SerializeField] private float collisionRadius = 0.5f;
        [SerializeField] private float collisionOffset = 0.1f;
        
        [Header("Bounds")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector3 minBounds = new Vector3(-10, -5, -10);
        [SerializeField] private Vector3 maxBounds = new Vector3(10, 5, 10);
        
        // Private fields
        private Vector3 velocity = Vector3.zero;
        private Vector3 rotationVelocity = Vector3.zero;
        private Vector3 currentLookAhead = Vector3.zero;
        private Vector3 lastTargetPosition;
        private Camera cam;
        
        // Properties
        public Transform Target => target;
        public Vector3 Offset => offset;
        public bool IsFollowing => followTarget;
        
        // Events
        public System.Action<Transform> OnTargetChanged;
        public System.Action OnFollowStarted;
        public System.Action OnFollowStopped;
        
        private void Awake()
        {
            cam = GetComponent<Camera>();
            
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
            
            lastTargetPosition = target != null ? target.position : transform.position;
        }
        
        private void LateUpdate()
        {
            if (target == null || !followTarget) return;
            
            HandleFollow();
            HandleLookAhead();
            HandleCollision();
            HandleBounds();
        }
        
        private void HandleFollow()
        {
            Vector3 targetPosition = target.position + offset;
            
            if (smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
            
            // Handle rotation
            if (smoothRotation)
            {
                Vector3 targetDirection = (target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.LookAt(target);
            }
        }
        
        private void HandleLookAhead()
        {
            if (!useLookAhead) return;
            
            Vector3 targetVelocity = target.position - lastTargetPosition;
            targetVelocity.y = 0; // Only consider horizontal movement
            
            Vector3 targetLookAhead = targetVelocity.normalized * lookAheadDistance;
            currentLookAhead = Vector3.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            
            transform.position += currentLookAhead + lookAheadOffset;
            lastTargetPosition = target.position;
        }
        
        private void HandleCollision()
        {
            if (!useCollision) return;
            
            Vector3 direction = (target.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, target.position);
            
            RaycastHit hit;
            if (Physics.SphereCast(target.position, collisionRadius, -direction, out hit, distance, collisionMask))
            {
                Vector3 newPosition = hit.point + hit.normal * collisionRadius + direction * collisionOffset;
                transform.position = newPosition;
            }
        }
        
        private void HandleBounds()
        {
            if (!useBounds) return;
            
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            position.z = Mathf.Clamp(position.z, minBounds.z, maxBounds.z);
            transform.position = position;
        }
        
        public void SetTarget(Transform newTarget)
        {
            if (target != newTarget)
            {
                target = newTarget;
                OnTargetChanged?.Invoke(target);
                
                if (target != null)
                {
                    lastTargetPosition = target.position;
                    OnFollowStarted?.Invoke();
                }
                else
                {
                    OnFollowStopped?.Invoke();
                }
            }
        }
        
        public void SetFollow(bool follow)
        {
            followTarget = follow;
            
            if (follow)
            {
                OnFollowStarted?.Invoke();
            }
            else
            {
                OnFollowStopped?.Invoke();
            }
        }
        
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
        
        public void SetBounds(Vector3 min, Vector3 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }
        
        public void DisableBounds()
        {
            useBounds = false;
        }
        
        public void SnapToTarget()
        {
            if (target == null) return;
            
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
        
        public void SetLookAhead(bool enabled, float distance = 3f, float speed = 2f)
        {
            useLookAhead = enabled;
            lookAheadDistance = distance;
            lookAheadSpeed = speed;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            
            // Draw follow line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.position);
            
            // Draw offset
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);
            
            // Draw collision sphere
            if (useCollision)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, collisionRadius);
            }
            
            // Draw bounds
            if (useBounds)
            {
                Gizmos.color = Color.red;
                Vector3 center = (minBounds + maxBounds) / 2;
                Vector3 size = maxBounds - minBounds;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}