using UnityEngine;

namespace UnityTemplates.Platformer2D
{
    [RequireComponent(typeof(Collider2D))]
    public class Collectible2D : MonoBehaviour
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType collectibleType = CollectibleType.Coin;
        [SerializeField] private int value = 1;
        [SerializeField] private bool destroyOnCollect = true;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.5f;
        
        [Header("Animation")]
        [SerializeField] private bool useBobAnimation = true;
        [SerializeField] private bool useRotation = true;
        [SerializeField] private float rotationSpeed = 90f;
        
        private Vector3 startPosition;
        private AudioSource audioSource;
        private bool isCollected = false;
        
        public enum CollectibleType
        {
            Coin,
            Health,
            PowerUp,
            Key,
            Gem
        }
        
        // Events
        public System.Action<Collectible2D> OnCollected;
        
        private void Awake()
        {
            startPosition = transform.position;
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Ensure trigger is enabled
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        
        private void Update()
        {
            if (isCollected) return;
            
            HandleAnimation();
        }
        
        private void HandleAnimation()
        {
            if (useBobAnimation)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            if (useRotation)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;
            
            // Check if the collider belongs to the player
            if (other.CompareTag("Player"))
            {
                Collect(other.GetComponent<PlayerController2D>());
            }
        }
        
        private void Collect(PlayerController2D player)
        {
            if (isCollected) return;
            
            isCollected = true;
            
            // Play effects
            PlayCollectEffects();
            
            // Notify systems
            NotifyCollection(player);
            
            // Destroy or disable
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        private void PlayCollectEffects()
        {
            // Play sound
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            
            // Spawn effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
        }
        
        private void NotifyCollection(PlayerController2D player)
        {
            // Notify collectible manager
            CollectibleManager2D manager = FindObjectOfType<CollectibleManager2D>();
            if (manager != null)
            {
                manager.CollectItem(collectibleType, value);
            }
            
            // Notify player
            if (player != null)
            {
                player.OnCollectible?.Invoke(collectibleType, value);
            }
            
            // Invoke event
            OnCollected?.Invoke(this);
        }
        
        public void SetValue(int newValue)
        {
            value = newValue;
        }
        
        public void SetType(CollectibleType type)
        {
            collectibleType = type;
        }
    }
}