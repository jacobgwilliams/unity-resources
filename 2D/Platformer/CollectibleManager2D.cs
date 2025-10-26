using UnityEngine;
using System.Collections.Generic;

namespace UnityTemplates.Platformer2D
{
    [CreateAssetMenu(fileName = "CollectibleData", menuName = "Unity Templates/Platformer2D/Collectible Data")]
    public class CollectibleData : ScriptableObject
    {
        [Header("Collectible Types")]
        public CollectibleType[] collectibleTypes;
        
        [System.Serializable]
        public class CollectibleType
        {
            public Collectible2D.CollectibleType type;
            public string displayName;
            public Sprite icon;
            public Color color = Color.white;
        }
    }
    
    public class CollectibleManager2D : MonoBehaviour
    {
        [Header("Collectible Data")]
        [SerializeField] private CollectibleData collectibleData;
        
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Text coinText;
        [SerializeField] private UnityEngine.UI.Text healthText;
        
        // Collection tracking
        private Dictionary<Collectible2D.CollectibleType, int> collectedItems = new Dictionary<Collectible2D.CollectibleType, int>();
        
        // Events
        public System.Action<Collectible2D.CollectibleType, int> OnItemCollected;
        public System.Action<Collectible2D.CollectibleType, int> OnItemCountChanged;
        
        private void Awake()
        {
            InitializeCollection();
        }
        
        private void Start()
        {
            UpdateUI();
        }
        
        private void InitializeCollection()
        {
            // Initialize all collectible types to 0
            if (collectibleData != null)
            {
                foreach (var type in collectibleData.collectibleTypes)
                {
                    collectedItems[type.type] = 0;
                }
            }
        }
        
        public void CollectItem(Collectible2D.CollectibleType type, int value)
        {
            if (!collectedItems.ContainsKey(type))
            {
                collectedItems[type] = 0;
            }
            
            collectedItems[type] += value;
            
            OnItemCollected?.Invoke(type, value);
            OnItemCountChanged?.Invoke(type, collectedItems[type]);
            
            UpdateUI();
        }
        
        public int GetItemCount(Collectible2D.CollectibleType type)
        {
            return collectedItems.ContainsKey(type) ? collectedItems[type] : 0;
        }
        
        public void SetItemCount(Collectible2D.CollectibleType type, int count)
        {
            collectedItems[type] = count;
            OnItemCountChanged?.Invoke(type, count);
            UpdateUI();
        }
        
        public void ResetCollection()
        {
            collectedItems.Clear();
            InitializeCollection();
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (coinText != null)
            {
                coinText.text = GetItemCount(Collectible2D.CollectibleType.Coin).ToString();
            }
            
            if (healthText != null)
            {
                healthText.text = GetItemCount(Collectible2D.CollectibleType.Health).ToString();
            }
        }
        
        public CollectibleData.CollectibleType GetCollectibleTypeData(Collectible2D.CollectibleType type)
        {
            if (collectibleData == null) return null;
            
            foreach (var data in collectibleData.collectibleTypes)
            {
                if (data.type == type)
                {
                    return data;
                }
            }
            
            return null;
        }
    }
}