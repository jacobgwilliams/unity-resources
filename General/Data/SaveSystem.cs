using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace UnityTemplates.General
{
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private int maxSaveSlots = 10;
        [SerializeField] private bool encryptSaves = false;
        [SerializeField] private string encryptionKey = "YourEncryptionKey";
        
        [Header("Auto Save")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool saveOnSceneChange = true;
        
        // Private fields
        private static SaveSystem instance;
        private string savePath;
        private Coroutine autoSaveCoroutine;
        private Dictionary<string, object> saveData = new Dictionary<string, object>();
        
        // Singleton
        public static SaveSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SaveSystem>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SaveSystem");
                        instance = go.AddComponent<SaveSystem>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        // Properties
        public string SavePath => savePath;
        public bool IsAutoSaveEnabled => enableAutoSave;
        public float AutoSaveInterval => autoSaveInterval;
        
        // Events
        public System.Action OnSaveStarted;
        public System.Action OnSaveCompleted;
        public System.Action OnLoadStarted;
        public System.Action OnLoadCompleted;
        public System.Action<string> OnSaveError;
        public System.Action<string> OnLoadError;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (enableAutoSave)
            {
                StartAutoSave();
            }
            
            if (saveOnSceneChange)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        
        private void OnDestroy()
        {
            if (saveOnSceneChange)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
        
        private void InitializeSaveSystem()
        {
            savePath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }
        
        private void StartAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
            
            autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        }
        
        private System.Collections.IEnumerator AutoSaveCoroutine()
        {
            while (enableAutoSave)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                SaveGame("autosave");
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (enableAutoSave)
            {
                SaveGame("autosave");
            }
        }
        
        public void SaveGame(string slotName = "default")
        {
            try
            {
                OnSaveStarted?.Invoke();
                
                // Collect save data from all saveable objects
                CollectSaveData();
                
                // Create save data structure
                SaveData saveData = new SaveData
                {
                    slotName = slotName,
                    saveTime = DateTime.Now,
                    version = Application.version,
                    sceneName = SceneManager.GetActiveScene().name,
                    data = this.saveData
                };
                
                // Serialize to JSON
                string json = JsonUtility.ToJson(saveData, true);
                
                // Encrypt if enabled
                if (encryptSaves)
                {
                    json = EncryptString(json);
                }
                
                // Write to file
                string filePath = Path.Combine(savePath, $"{slotName}.json");
                File.WriteAllText(filePath, json);
                
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                OnSaveError?.Invoke(e.Message);
                Debug.LogError($"Save failed: {e.Message}");
            }
        }
        
        public bool LoadGame(string slotName = "default")
        {
            try
            {
                OnLoadStarted?.Invoke();
                
                string filePath = Path.Combine(savePath, $"{slotName}.json");
                
                if (!File.Exists(filePath))
                {
                    OnLoadError?.Invoke("Save file not found");
                    return false;
                }
                
                // Read file
                string json = File.ReadAllText(filePath);
                
                // Decrypt if encrypted
                if (encryptSaves)
                {
                    json = DecryptString(json);
                }
                
                // Deserialize
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                
                // Apply save data
                this.saveData = saveData.data;
                ApplySaveData();
                
                OnLoadCompleted?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                OnLoadError?.Invoke(e.Message);
                Debug.LogError($"Load failed: {e.Message}");
                return false;
            }
        }
        
        public void DeleteSave(string slotName)
        {
            string filePath = Path.Combine(savePath, $"{slotName}.json");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        public bool SaveExists(string slotName)
        {
            string filePath = Path.Combine(savePath, $"{slotName}.json");
            return File.Exists(filePath);
        }
        
        public List<string> GetAvailableSaves()
        {
            List<string> saves = new List<string>();
            
            if (Directory.Exists(savePath))
            {
                string[] files = Directory.GetFiles(savePath, "*.json");
                
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    saves.Add(fileName);
                }
            }
            
            return saves;
        }
        
        public SaveData GetSaveInfo(string slotName)
        {
            try
            {
                string filePath = Path.Combine(savePath, $"{slotName}.json");
                
                if (!File.Exists(filePath))
                {
                    return null;
                }
                
                string json = File.ReadAllText(filePath);
                
                if (encryptSaves)
                {
                    json = DecryptString(json);
                }
                
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get save info: {e.Message}");
                return null;
            }
        }
        
        private void CollectSaveData()
        {
            saveData.Clear();
            
            // Find all saveable objects
            ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>() as ISaveable[];
            
            foreach (ISaveable saveable in saveableObjects)
            {
                if (saveable != null)
                {
                    string key = saveable.GetSaveKey();
                    object data = saveable.GetSaveData();
                    saveData[key] = data;
                }
            }
        }
        
        private void ApplySaveData()
        {
            // Find all saveable objects
            ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>() as ISaveable[];
            
            foreach (ISaveable saveable in saveableObjects)
            {
                if (saveable != null)
                {
                    string key = saveable.GetSaveKey();
                    if (saveData.ContainsKey(key))
                    {
                        saveable.LoadSaveData(saveData[key]);
                    }
                }
            }
        }
        
        private string EncryptString(string text)
        {
            // Simple XOR encryption (replace with proper encryption in production)
            string encrypted = "";
            for (int i = 0; i < text.Length; i++)
            {
                encrypted += (char)(text[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            return encrypted;
        }
        
        private string DecryptString(string encryptedText)
        {
            // Simple XOR decryption (replace with proper decryption in production)
            string decrypted = "";
            for (int i = 0; i < encryptedText.Length; i++)
            {
                decrypted += (char)(encryptedText[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            return decrypted;
        }
        
        public void SetAutoSave(bool enabled)
        {
            enableAutoSave = enabled;
            
            if (enabled)
            {
                StartAutoSave();
            }
            else if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }
        
        public void SetAutoSaveInterval(float interval)
        {
            autoSaveInterval = interval;
            
            if (enableAutoSave)
            {
                StartAutoSave();
            }
        }
        
        public void SetEncryption(bool enabled, string key = null)
        {
            encryptSaves = enabled;
            if (key != null)
            {
                encryptionKey = key;
            }
        }
        
        public void SetMaxSaveSlots(int slots)
        {
            maxSaveSlots = slots;
        }
        
        public void SetSaveFileName(string fileName)
        {
            saveFileName = fileName;
        }
        
        // Static convenience methods
        public static void SaveGameStatic(string slotName = "default")
        {
            Instance.SaveGame(slotName);
        }
        
        public static bool LoadGameStatic(string slotName = "default")
        {
            return Instance.LoadGame(slotName);
        }
        
        public static void DeleteSaveStatic(string slotName)
        {
            Instance.DeleteSave(slotName);
        }
        
        public static bool SaveExistsStatic(string slotName)
        {
            return Instance.SaveExists(slotName);
        }
        
        public static List<string> GetAvailableSavesStatic()
        {
            return Instance.GetAvailableSaves();
        }
    }
    
    [System.Serializable]
    public class SaveData
    {
        public string slotName;
        public DateTime saveTime;
        public string version;
        public string sceneName;
        public Dictionary<string, object> data;
    }
    
    public interface ISaveable
    {
        string GetSaveKey();
        object GetSaveData();
        void LoadSaveData(object data);
    }
}