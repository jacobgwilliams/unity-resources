using UnityEngine;
using System.Collections;

namespace UnityTemplates.FPS3D
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Unity Templates/FPS3D/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName;
        public WeaponType weaponType;
        public Sprite weaponIcon;
        
        [Header("Damage")]
        public int damage = 25;
        public float range = 100f;
        public float fireRate = 0.1f;
        public int maxAmmo = 30;
        public int ammoPerShot = 1;
        
        [Header("Accuracy")]
        public float accuracy = 0.95f;
        public float recoilAmount = 1f;
        public float recoilRecoverySpeed = 2f;
        
        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
        
        [Header("Visual Effects")]
        public GameObject muzzleFlash;
        public GameObject bulletHole;
        public GameObject impactEffect;
        
        public enum WeaponType
        {
            Pistol,
            Rifle,
            Shotgun,
            Sniper,
            SMG
        }
    }
    
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask enemyLayerMask = 1;
        
        [Header("Recoil")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float recoilStrength = 1f;
        [SerializeField] private float recoilRecoverySpeed = 2f;
        
        [Header("Crosshair")]
        [SerializeField] private GameObject crosshairUI;
        [SerializeField] private float crosshairSpread = 0f;
        [SerializeField] private float maxCrosshairSpread = 50f;
        [SerializeField] private float crosshairRecoverySpeed = 2f;
        
        // Private fields
        private int currentAmmo;
        private float lastFireTime;
        private float currentRecoil;
        private bool isReloading;
        private AudioSource audioSource;
        private Camera playerCamera;
        
        // Properties
        public WeaponData CurrentWeapon => currentWeapon;
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => currentWeapon != null ? currentWeapon.maxAmmo : 0;
        public bool IsReloading => isReloading;
        public bool CanFire => !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= currentWeapon.fireRate;
        
        // Events
        public System.Action<WeaponData> OnWeaponChanged;
        public System.Action OnWeaponFired;
        public System.Action OnWeaponReloaded;
        public System.Action OnAmmoChanged;
        public System.Action OnHit;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            playerCamera = Camera.main;
            if (cameraTransform == null)
            {
                cameraTransform = playerCamera?.transform;
            }
            
            if (currentWeapon != null)
            {
                EquipWeapon(currentWeapon);
            }
        }
        
        private void Update()
        {
            HandleInput();
            UpdateRecoil();
            UpdateCrosshair();
        }
        
        private void HandleInput()
        {
            if (currentWeapon == null) return;
            
            // Fire
            if (Input.GetButton("Fire1") && CanFire)
            {
                Fire();
            }
            
            // Reload
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < currentWeapon.maxAmmo)
            {
                StartReload();
            }
        }
        
        public void Fire()
        {
            if (!CanFire) return;
            
            lastFireTime = Time.time;
            currentAmmo -= currentWeapon.ammoPerShot;
            
            // Apply recoil
            ApplyRecoil();
            
            // Play fire sound
            if (currentWeapon.fireSound != null)
            {
                audioSource.PlayOneShot(currentWeapon.fireSound);
            }
            
            // Muzzle flash
            if (currentWeapon.muzzleFlash != null && firePoint != null)
            {
                GameObject flash = Instantiate(currentWeapon.muzzleFlash, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }
            
            // Raycast for hit detection
            PerformRaycast();
            
            OnWeaponFired?.Invoke();
            OnAmmoChanged?.Invoke();
        }
        
        private void PerformRaycast()
        {
            if (playerCamera == null) return;
            
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            
            // Add accuracy variation
            float accuracyModifier = currentWeapon.accuracy;
            float spread = (1f - accuracyModifier) * maxCrosshairSpread;
            rayDirection += Random.insideUnitSphere * spread * 0.01f;
            rayDirection.Normalize();
            
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, currentWeapon.range))
            {
                // Create bullet hole
                if (currentWeapon.bulletHole != null)
                {
                    GameObject hole = Instantiate(currentWeapon.bulletHole, hit.point + hit.normal * 0.01f, 
                        Quaternion.LookRotation(hit.normal));
                    hole.transform.SetParent(hit.transform);
                    Destroy(hole, 10f);
                }
                
                // Impact effect
                if (currentWeapon.impactEffect != null)
                {
                    GameObject impact = Instantiate(currentWeapon.impactEffect, hit.point, 
                        Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
                
                // Deal damage
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(currentWeapon.damage);
                    OnHit?.Invoke();
                }
            }
        }
        
        private void ApplyRecoil()
        {
            if (cameraTransform == null) return;
            
            float recoilAmount = currentWeapon.recoilAmount * recoilStrength;
            currentRecoil += recoilAmount;
            
            // Apply vertical recoil
            float verticalRecoil = Random.Range(-recoilAmount, -recoilAmount * 0.5f);
            cameraTransform.Rotate(-verticalRecoil, 0, 0);
            
            // Apply horizontal recoil
            float horizontalRecoil = Random.Range(-recoilAmount * 0.3f, recoilAmount * 0.3f);
            transform.Rotate(0, horizontalRecoil, 0);
        }
        
        private void UpdateRecoil()
        {
            if (cameraTransform == null) return;
            
            // Recover from recoil
            currentRecoil = Mathf.Lerp(currentRecoil, 0, recoilRecoverySpeed * Time.deltaTime);
            
            // Reset camera rotation
            Vector3 currentRotation = cameraTransform.localEulerAngles;
            currentRotation.x = Mathf.LerpAngle(currentRotation.x, 0, recoilRecoverySpeed * Time.deltaTime);
            cameraTransform.localEulerAngles = currentRotation;
        }
        
        private void UpdateCrosshair()
        {
            if (crosshairUI == null) return;
            
            // Update crosshair spread based on movement and firing
            float targetSpread = 0f;
            
            if (isReloading)
            {
                targetSpread = maxCrosshairSpread * 0.5f;
            }
            else if (Time.time - lastFireTime < currentWeapon.fireRate)
            {
                targetSpread = maxCrosshairSpread * 0.8f;
            }
            
            crosshairSpread = Mathf.Lerp(crosshairSpread, targetSpread, crosshairRecoverySpeed * Time.deltaTime);
            
            // Apply crosshair spread to UI (implementation depends on your UI system)
            // This is a placeholder - you'll need to implement based on your UI setup
        }
        
        public void StartReload()
        {
            if (isReloading || currentAmmo >= currentWeapon.maxAmmo) return;
            
            StartCoroutine(ReloadCoroutine());
        }
        
        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            
            // Play reload sound
            if (currentWeapon.reloadSound != null)
            {
                audioSource.PlayOneShot(currentWeapon.reloadSound);
            }
            
            // Wait for reload time (you might want to make this configurable)
            yield return new WaitForSeconds(2f);
            
            // Refill ammo
            currentAmmo = currentWeapon.maxAmmo;
            
            isReloading = false;
            OnWeaponReloaded?.Invoke();
            OnAmmoChanged?.Invoke();
        }
        
        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon == null) return;
            
            currentWeapon = weapon;
            currentAmmo = weapon.maxAmmo;
            isReloading = false;
            
            OnWeaponChanged?.Invoke(weapon);
            OnAmmoChanged?.Invoke();
        }
        
        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Min(currentAmmo + amount, currentWeapon.maxAmmo);
            OnAmmoChanged?.Invoke();
        }
        
        public bool HasAmmo()
        {
            return currentAmmo > 0;
        }
        
        public float GetAmmoPercentage()
        {
            return (float)currentAmmo / currentWeapon.maxAmmo;
        }
    }
    
    // Interface for objects that can take damage
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}