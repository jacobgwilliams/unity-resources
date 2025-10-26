using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UnityTemplates.General
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("Loading Settings")]
        [SerializeField] private float minLoadingTime = 1f;
        [SerializeField] private bool showLoadingScreen = true;
        [SerializeField] private GameObject loadingScreenPrefab;
        
        [Header("Fade Settings")]
        [SerializeField] private bool useFadeTransition = true;
        [SerializeField] private float fadeTime = 1f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        
        // Private fields
        private static SceneLoader instance;
        private GameObject currentLoadingScreen;
        private bool isLoading = false;
        
        // Singleton
        public static SceneLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneLoader>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SceneLoader");
                        instance = go.AddComponent<SceneLoader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        // Properties
        public bool IsLoading => isLoading;
        
        // Events
        public System.Action<string> OnSceneLoadStarted;
        public System.Action<string> OnSceneLoadCompleted;
        public System.Action<float> OnLoadingProgress;
        
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
            // Setup fade canvas group if not assigned
            if (fadeCanvasGroup == null && useFadeTransition)
            {
                SetupFadeCanvas();
            }
        }
        
        private void SetupFadeCanvas()
        {
            GameObject fadeObj = new GameObject("FadeCanvas");
            fadeObj.transform.SetParent(transform);
            
            Canvas canvas = fadeObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            CanvasScaler scaler = fadeObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            fadeObj.AddComponent<GraphicRaycaster>();
            
            fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            
            // Create black background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(fadeObj.transform);
            
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image image = background.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
        }
        
        public void LoadScene(string sceneName)
        {
            if (isLoading) return;
            
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }
        
        public void LoadScene(int sceneIndex)
        {
            if (isLoading) return;
            
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }
        
        public void ReloadCurrentScene()
        {
            if (isLoading) return;
            
            string currentSceneName = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneCoroutine(currentSceneName));
        }
        
        public void LoadNextScene()
        {
            if (isLoading) return;
            
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = (currentIndex + 1) % SceneManager.sceneCountInBuildSettings;
            LoadScene(nextIndex);
        }
        
        public void LoadPreviousScene()
        {
            if (isLoading) return;
            
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int previousIndex = currentIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = SceneManager.sceneCountInBuildSettings - 1;
            }
            LoadScene(previousIndex);
        }
        
        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneName);
            
            // Show loading screen
            if (showLoadingScreen && loadingScreenPrefab != null)
            {
                ShowLoadingScreen();
            }
            
            // Fade out
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            float loadingStartTime = Time.time;
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLoadingProgress?.Invoke(progress);
                
                // Ensure minimum loading time
                if (asyncLoad.progress >= 0.9f && Time.time - loadingStartTime >= minLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // Wait for scene to be fully loaded
            yield return new WaitForEndOfFrame();
            
            // Hide loading screen
            if (currentLoadingScreen != null)
            {
                HideLoadingScreen();
            }
            
            // Fade in
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        
        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null)
            {
                currentLoadingScreen = Instantiate(loadingScreenPrefab);
                currentLoadingScreen.transform.SetParent(transform);
            }
        }
        
        private void HideLoadingScreen()
        {
            if (currentLoadingScreen != null)
            {
                Destroy(currentLoadingScreen);
                currentLoadingScreen = null;
            }
        }
        
        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;
            
            fadeCanvasGroup.blocksRaycasts = true;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
        }
        
        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        public void SetMinLoadingTime(float time)
        {
            minLoadingTime = time;
        }
        
        public void SetFadeTime(float time)
        {
            fadeTime = time;
        }
        
        public void SetUseFadeTransition(bool use)
        {
            useFadeTransition = use;
        }
        
        public void SetShowLoadingScreen(bool show)
        {
            showLoadingScreen = show;
        }
        
        public void SetLoadingScreenPrefab(GameObject prefab)
        {
            loadingScreenPrefab = prefab;
        }
        
        // Static convenience methods
        public static void LoadSceneStatic(string sceneName)
        {
            Instance.LoadScene(sceneName);
        }
        
        public static void LoadSceneStatic(int sceneIndex)
        {
            Instance.LoadScene(sceneIndex);
        }
        
        public static void ReloadCurrentSceneStatic()
        {
            Instance.ReloadCurrentScene();
        }
        
        public static void LoadNextSceneStatic()
        {
            Instance.LoadNextScene();
        }
        
        public static void LoadPreviousSceneStatic()
        {
            Instance.LoadPreviousScene();
        }
    }
}