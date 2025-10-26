using UnityEngine;

namespace UnityTemplates.Platformer2D
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        
        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 2f;
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;
        
        [Header("Bounds")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-10, -5);
        [SerializeField] private Vector2 maxBounds = new Vector2(10, 5);
        
        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = false;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 2f;
        
        private Vector3 velocity = Vector3.zero;
        private Vector3 lookAheadOffset = Vector3.zero;
        private Camera cam;
        
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
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            FollowTarget();
        }
        
        private void FollowTarget()
        {
            Vector3 targetPosition = target.position + offset;
            
            // Apply look ahead
            if (useLookAhead)
            {
                float targetLookAhead = target.GetComponent<Rigidbody2D>()?.velocity.x ?? 0f;
                lookAheadOffset.x = Mathf.Lerp(lookAheadOffset.x, 
                    Mathf.Sign(targetLookAhead) * lookAheadDistance, 
                    lookAheadSpeed * Time.deltaTime);
                targetPosition += lookAheadOffset;
            }
            
            // Apply follow constraints
            if (!followX) targetPosition.x = transform.position.x;
            if (!followY) targetPosition.y = transform.position.y;
            
            // Apply bounds
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
            
            // Smooth follow
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }
        
        public void DisableBounds()
        {
            useBounds = false;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, transform.position.z);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}