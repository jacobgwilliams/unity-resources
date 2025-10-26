using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace UnityTemplates.General
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject dialoguePanel;
        
        [Header("Settings")]
        [SerializeField] private bool pauseGameOnMenuOpen = true;
        [SerializeField] private bool hideCursorInGameplay = true;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        
        // Private fields
        private Stack<GameObject> panelStack = new Stack<GameObject>();
        private GameObject currentActivePanel;
        private bool isPaused = false;
        
        // Properties
        public bool IsPaused => isPaused;
        public GameObject CurrentActivePanel => currentActivePanel;
        
        // Events
        public event Action<GameObject> OnPanelOpened;
        public event Action<GameObject> OnPanelClosed;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        
        private void Awake()
        {
            // Ensure only one UIManager exists
            if (FindObjectsOfType<UIManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        
        private void Start()
        {
            // Start with main menu
            if (mainMenuPanel != null)
            {
                OpenPanel(mainMenuPanel);
            }
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
        
        private void InitializeUI()
        {
            // Hide all panels initially
            GameObject[] allPanels = { mainMenuPanel, gameplayPanel, pausePanel, settingsPanel, inventoryPanel, dialoguePanel };
            
            foreach (GameObject panel in allPanels)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
            
            // Setup cursor visibility
            UpdateCursorVisibility();
        }
        
        public void OpenPanel(GameObject panel)
        {
            if (panel == null) return;
            
            // Close current panel if it exists
            if (currentActivePanel != null && currentActivePanel != panel)
            {
                ClosePanel(currentActivePanel);
            }
            
            // Add to stack
            panelStack.Push(panel);
            currentActivePanel = panel;
            
            // Show panel
            panel.SetActive(true);
            
            // Handle game pause
            if (pauseGameOnMenuOpen && IsMenuPanel(panel))
            {
                PauseGame();
            }
            
            OnPanelOpened?.Invoke(panel);
        }
        
        public void ClosePanel(GameObject panel)
        {
            if (panel == null) return;
            
            panel.SetActive(false);
            
            // Remove from stack
            if (panelStack.Count > 0 && panelStack.Peek() == panel)
            {
                panelStack.Pop();
            }
            
            // Update current active panel
            if (currentActivePanel == panel)
            {
                currentActivePanel = panelStack.Count > 0 ? panelStack.Peek() : null;
            }
            
            // Resume game if closing menu panel
            if (IsMenuPanel(panel) && isPaused)
            {
                ResumeGame();
            }
            
            OnPanelClosed?.Invoke(panel);
        }
        
        public void CloseCurrentPanel()
        {
            if (currentActivePanel != null)
            {
                ClosePanel(currentActivePanel);
            }
        }
        
        public void CloseAllPanels()
        {
            while (panelStack.Count > 0)
            {
                GameObject panel = panelStack.Pop();
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
            
            currentActivePanel = null;
            ResumeGame();
        }
        
        public void PauseGame()
        {
            if (isPaused) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            if (pausePanel != null)
            {
                OpenPanel(pausePanel);
            }
            
            OnGamePaused?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (!isPaused) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            
            if (pausePanel != null)
            {
                ClosePanel(pausePanel);
            }
            
            OnGameResumed?.Invoke();
        }
        
        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        public void ShowGameplayUI()
        {
            if (gameplayPanel != null)
            {
                OpenPanel(gameplayPanel);
            }
        }
        
        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
            {
                OpenPanel(mainMenuPanel);
            }
        }
        
        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                OpenPanel(settingsPanel);
            }
        }
        
        public void ShowInventory()
        {
            if (inventoryPanel != null)
            {
                OpenPanel(inventoryPanel);
            }
        }
        
        public void ShowDialogue()
        {
            if (dialoguePanel != null)
            {
                OpenPanel(dialoguePanel);
            }
        }
        
        private bool IsMenuPanel(GameObject panel)
        {
            return panel == mainMenuPanel || panel == pausePanel || panel == settingsPanel || 
                   panel == inventoryPanel || panel == dialoguePanel;
        }
        
        private void UpdateCursorVisibility()
        {
            if (hideCursorInGameplay)
            {
                Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isPaused;
            }
        }
        
        public void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
        
        public bool IsPanelActive(GameObject panel)
        {
            return panel != null && panel.activeInHierarchy;
        }
        
        public void SetPauseKey(KeyCode newKey)
        {
            pauseKey = newKey;
        }
        
        public void SetPauseGameOnMenuOpen(bool pause)
        {
            pauseGameOnMenuOpen = pause;
        }
        
        public void SetHideCursorInGameplay(bool hide)
        {
            hideCursorInGameplay = hide;
            UpdateCursorVisibility();
        }
        
        // Button event handlers (can be called from UI buttons)
        public void OnMainMenuButtonClicked()
        {
            ShowMainMenu();
        }
        
        public void OnResumeButtonClicked()
        {
            ResumeGame();
        }
        
        public void OnSettingsButtonClicked()
        {
            ShowSettings();
        }
        
        public void OnInventoryButtonClicked()
        {
            ShowInventory();
        }
        
        public void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}