using UnityEngine;

namespace UnityTemplates.iOS
{
    public class HapticFeedback : MonoBehaviour
    {
        [Header("Haptic Settings")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private float hapticIntensity = 1f;
        
        public enum HapticType
        {
            Light,
            Medium,
            Heavy,
            Success,
            Warning,
            Error,
            Selection
        }
        
        // Singleton instance
        private static HapticFeedback instance;
        public static HapticFeedback Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<HapticFeedback>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("HapticFeedback");
                        instance = go.AddComponent<HapticFeedback>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Check if haptics are supported on this device
            if (!IsHapticSupported())
            {
                Debug.LogWarning("Haptic feedback is not supported on this device");
                enableHaptics = false;
            }
        }
        
        public void TriggerHaptic(HapticType type)
        {
            if (!enableHaptics) return;
            
            switch (type)
            {
                case HapticType.Light:
                    TriggerLightHaptic();
                    break;
                case HapticType.Medium:
                    TriggerMediumHaptic();
                    break;
                case HapticType.Heavy:
                    TriggerHeavyHaptic();
                    break;
                case HapticType.Success:
                    TriggerSuccessHaptic();
                    break;
                case HapticType.Warning:
                    TriggerWarningHaptic();
                    break;
                case HapticType.Error:
                    TriggerErrorHaptic();
                    break;
                case HapticType.Selection:
                    TriggerSelectionHaptic();
                    break;
            }
        }
        
        private void TriggerLightHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Handheld.Vibrate();
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                Handheld.Vibrate();
            }
            #endif
        }
        
        private void TriggerMediumHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // iOS specific haptic feedback
                iOSHapticFeedback.TriggerImpactFeedback(iOSHapticFeedback.ImpactFeedbackStyle.Medium);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                // Android specific haptic feedback
                AndroidHapticFeedback.TriggerImpactFeedback(AndroidHapticFeedback.ImpactFeedbackStyle.Medium);
            }
            #endif
        }
        
        private void TriggerHeavyHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                iOSHapticFeedback.TriggerImpactFeedback(iOSHapticFeedback.ImpactFeedbackStyle.Heavy);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidHapticFeedback.TriggerImpactFeedback(AndroidHapticFeedback.ImpactFeedbackStyle.Heavy);
            }
            #endif
        }
        
        private void TriggerSuccessHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                iOSHapticFeedback.TriggerNotificationFeedback(iOSHapticFeedback.NotificationFeedbackStyle.Success);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                // Custom success pattern for Android
                StartCoroutine(PlayHapticPattern(new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f }));
            }
            #endif
        }
        
        private void TriggerWarningHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                iOSHapticFeedback.TriggerNotificationFeedback(iOSHapticFeedback.NotificationFeedbackStyle.Warning);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                // Custom warning pattern for Android
                StartCoroutine(PlayHapticPattern(new float[] { 0.2f, 0.1f, 0.2f }));
            }
            #endif
        }
        
        private void TriggerErrorHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                iOSHapticFeedback.TriggerNotificationFeedback(iOSHapticFeedback.NotificationFeedbackStyle.Error);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                // Custom error pattern for Android
                StartCoroutine(PlayHapticPattern(new float[] { 0.3f, 0.1f, 0.3f, 0.1f, 0.3f }));
            }
            #endif
        }
        
        private void TriggerSelectionHaptic()
        {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                iOSHapticFeedback.TriggerSelectionFeedback();
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidHapticFeedback.TriggerSelectionFeedback();
            }
            #endif
        }
        
        private System.Collections.IEnumerator PlayHapticPattern(float[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                Handheld.Vibrate();
                yield return new WaitForSeconds(pattern[i]);
            }
        }
        
        private bool IsHapticSupported()
        {
            #if UNITY_IOS
            return Application.platform == RuntimePlatform.IPhonePlayer;
            #elif UNITY_ANDROID
            return Application.platform == RuntimePlatform.Android;
            #else
            return false;
            #endif
        }
        
        public void SetHapticEnabled(bool enabled)
        {
            enableHaptics = enabled;
        }
        
        public void SetHapticIntensity(float intensity)
        {
            hapticIntensity = Mathf.Clamp01(intensity);
        }
        
        // Convenience methods for common game events
        public void OnButtonPress()
        {
            TriggerHaptic(HapticType.Selection);
        }
        
        public void OnItemCollected()
        {
            TriggerHaptic(HapticType.Success);
        }
        
        public void OnPlayerHit()
        {
            TriggerHaptic(HapticType.Medium);
        }
        
        public void OnPlayerDeath()
        {
            TriggerHaptic(HapticType.Error);
        }
        
        public void OnWeaponFire()
        {
            TriggerHaptic(HapticType.Light);
        }
        
        public void OnExplosion()
        {
            TriggerHaptic(HapticType.Heavy);
        }
    }
    
    // iOS specific haptic feedback implementation
    #if UNITY_IOS
    public static class iOSHapticFeedback
    {
        public enum ImpactFeedbackStyle
        {
            Light,
            Medium,
            Heavy
        }
        
        public enum NotificationFeedbackStyle
        {
            Success,
            Warning,
            Error
        }
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerImpactFeedback(int style);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerNotificationFeedback(int style);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerSelectionFeedback();
        
        public static void TriggerImpactFeedback(ImpactFeedbackStyle style)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _TriggerImpactFeedback((int)style);
            }
        }
        
        public static void TriggerNotificationFeedback(NotificationFeedbackStyle style)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _TriggerNotificationFeedback((int)style);
            }
        }
        
        public static void TriggerSelectionFeedback()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _TriggerSelectionFeedback();
            }
        }
    }
    #endif
    
    // Android specific haptic feedback implementation
    #if UNITY_ANDROID
    public static class AndroidHapticFeedback
    {
        public enum ImpactFeedbackStyle
        {
            Light,
            Medium,
            Heavy
        }
        
        private static AndroidJavaClass unityPlayer;
        private static AndroidJavaObject currentActivity;
        private static AndroidJavaObject vibrator;
        
        static AndroidHapticFeedback()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
        }
        
        public static void TriggerImpactFeedback(ImpactFeedbackStyle style)
        {
            if (Application.platform == RuntimePlatform.Android && vibrator != null)
            {
                long duration = (long)((int)style * 50); // 50ms, 100ms, 150ms
                vibrator.Call("vibrate", duration);
            }
        }
        
        public static void TriggerSelectionFeedback()
        {
            if (Application.platform == RuntimePlatform.Android && vibrator != null)
            {
                vibrator.Call("vibrate", 10L); // 10ms
            }
        }
    }
    #endif
}