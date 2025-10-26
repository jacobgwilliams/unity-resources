using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityTemplates.iOS
{
    public class TouchControls : MonoBehaviour
    {
        [Header("Virtual Joystick")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRange = 50f;
        [SerializeField] private bool snapToCenter = true;
        
        [Header("Touch Buttons")]
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button fireButton;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button pauseButton;
        
        [Header("Settings")]
        [SerializeField] private bool showOnMobileOnly = true;
        [SerializeField] private bool hideInEditor = true;
        
        // Private fields
        private Vector2 joystickInput;
        private bool isJoystickActive;
        private int joystickTouchId = -1;
        private Vector2 joystickCenter;
        private Camera uiCamera;
        private Canvas canvas;
        
        // Touch tracking
        private Dictionary<int, Vector2> touchPositions = new Dictionary<int, Vector2>();
        private Dictionary<int, Vector2> touchStartPositions = new Dictionary<int, Vector2>();
        
        // Properties
        public Vector2 JoystickInput => joystickInput;
        public bool IsJoystickActive => isJoystickActive;
        
        // Events
        public System.Action<Vector2> OnJoystickInput;
        public System.Action OnJumpPressed;
        public System.Action OnFirePressed;
        public System.Action OnReloadPressed;
        public System.Action OnPausePressed;
        public System.Action<Vector2> OnTouchStart;
        public System.Action<Vector2> OnTouchEnd;
        
        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            uiCamera = canvas?.worldCamera;
            
            // Hide on non-mobile platforms if specified
            if (hideInEditor && Application.isEditor)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (showOnMobileOnly && !IsMobilePlatform())
            {
                gameObject.SetActive(false);
                return;
            }
            
            SetupButtons();
        }
        
        private void Start()
        {
            if (joystickBackground != null)
            {
                joystickCenter = joystickBackground.position;
            }
        }
        
        private void Update()
        {
            HandleTouchInput();
        }
        
        private void HandleTouchInput()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchBegan(touch);
                        break;
                    case TouchPhase.Moved:
                        HandleTouchMoved(touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandleTouchEnded(touch);
                        break;
                }
            }
        }
        
        private void HandleTouchBegan(Touch touch)
        {
            Vector2 touchPosition = GetTouchWorldPosition(touch.position);
            touchPositions[touch.fingerId] = touchPosition;
            touchStartPositions[touch.fingerId] = touchPosition;
            
            // Check if touch is on joystick
            if (joystickBackground != null && IsTouchOnJoystick(touchPosition))
            {
                joystickTouchId = touch.fingerId;
                isJoystickActive = true;
                OnTouchStart?.Invoke(touchPosition);
            }
        }
        
        private void HandleTouchMoved(Touch touch)
        {
            if (!touchPositions.ContainsKey(touch.fingerId)) return;
            
            Vector2 touchPosition = GetTouchWorldPosition(touch.position);
            touchPositions[touch.fingerId] = touchPosition;
            
            // Handle joystick movement
            if (touch.fingerId == joystickTouchId && isJoystickActive)
            {
                UpdateJoystick(touchPosition);
            }
        }
        
        private void HandleTouchEnded(Touch touch)
        {
            if (!touchPositions.ContainsKey(touch.fingerId)) return;
            
            Vector2 touchPosition = GetTouchWorldPosition(touch.position);
            
            // Handle joystick release
            if (touch.fingerId == joystickTouchId)
            {
                ResetJoystick();
            }
            
            touchPositions.Remove(touch.fingerId);
            touchStartPositions.Remove(touch.fingerId);
            OnTouchEnd?.Invoke(touchPosition);
        }
        
        private void UpdateJoystick(Vector2 touchPosition)
        {
            Vector2 direction = touchPosition - joystickCenter;
            float distance = direction.magnitude;
            
            // Clamp to joystick range
            if (distance > joystickRange)
            {
                direction = direction.normalized * joystickRange;
            }
            
            // Update joystick input
            joystickInput = direction / joystickRange;
            
            // Move joystick handle
            if (joystickHandle != null)
            {
                joystickHandle.position = joystickCenter + direction;
            }
            
            OnJoystickInput?.Invoke(joystickInput);
        }
        
        private void ResetJoystick()
        {
            joystickInput = Vector2.zero;
            isJoystickActive = false;
            joystickTouchId = -1;
            
            if (joystickHandle != null && snapToCenter)
            {
                joystickHandle.position = joystickCenter;
            }
            
            OnJoystickInput?.Invoke(joystickInput);
        }
        
        private bool IsTouchOnJoystick(Vector2 touchPosition)
        {
            if (joystickBackground == null) return false;
            
            float distance = Vector2.Distance(touchPosition, joystickCenter);
            return distance <= joystickRange;
        }
        
        private Vector2 GetTouchWorldPosition(Vector2 screenPosition)
        {
            if (uiCamera != null)
            {
                return uiCamera.ScreenToWorldPoint(screenPosition);
            }
            else
            {
                return screenPosition;
            }
        }
        
        private void SetupButtons()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(() => OnJumpPressed?.Invoke());
            }
            
            if (fireButton != null)
            {
                fireButton.onClick.AddListener(() => OnFirePressed?.Invoke());
            }
            
            if (reloadButton != null)
            {
                reloadButton.onClick.AddListener(() => OnReloadPressed?.Invoke());
            }
            
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(() => OnPausePressed?.Invoke());
            }
        }
        
        private bool IsMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }
        
        public void SetJoystickSensitivity(float sensitivity)
        {
            joystickRange = sensitivity;
        }
        
        public void ShowControls(bool show)
        {
            gameObject.SetActive(show);
        }
        
        public void SetButtonVisibility(string buttonName, bool visible)
        {
            Button button = null;
            
            switch (buttonName.ToLower())
            {
                case "jump":
                    button = jumpButton;
                    break;
                case "fire":
                    button = fireButton;
                    break;
                case "reload":
                    button = reloadButton;
                    break;
                case "pause":
                    button = pauseButton;
                    break;
            }
            
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }
        
        public Vector2 GetJoystickDirection()
        {
            return joystickInput;
        }
        
        public bool IsButtonPressed(string buttonName)
        {
            Button button = null;
            
            switch (buttonName.ToLower())
            {
                case "jump":
                    button = jumpButton;
                    break;
                case "fire":
                    button = fireButton;
                    break;
                case "reload":
                    button = reloadButton;
                    break;
                case "pause":
                    button = pauseButton;
                    break;
            }
            
            return button != null && button.IsInteractable();
        }
    }
}