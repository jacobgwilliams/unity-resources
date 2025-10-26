using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityTemplates.TopDownRPG
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Unity Templates/TopDownRPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        public string description;
        public Sprite icon;
        public ItemType itemType;
        public ItemRarity rarity = ItemRarity.Common;
        
        [Header("Properties")]
        public int maxStackSize = 1;
        public int value = 0;
        public bool isConsumable = false;
        public bool isTradeable = true;
        
        [Header("Stats (if applicable)")]
        public int healthBonus = 0;
        public int manaBonus = 0;
        public int attackBonus = 0;
        public int defenseBonus = 0;
        public int speedBonus = 0;
        
        public enum ItemType
        {
            Weapon,
            Armor,
            Consumable,
            Quest,
            Misc
        }
        
        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
    }
    
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData itemData;
        public int quantity;
        public bool isEmpty => itemData == null || quantity <= 0;
        
        public InventorySlot()
        {
            itemData = null;
            quantity = 0;
        }
        
        public InventorySlot(ItemData item, int qty)
        {
            itemData = item;
            quantity = qty;
        }
    }
    
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int inventorySize = 20;
        [SerializeField] private bool autoSort = true;
        
        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private Transform inventoryParent;
        [SerializeField] private GameObject inventorySlotPrefab;
        
        // Inventory data
        private List<InventorySlot> inventory = new List<InventorySlot>();
        private Dictionary<ItemData.ItemType, List<InventorySlot>> itemsByType = new Dictionary<ItemData.ItemType, List<InventorySlot>>();
        
        // Events
        public event Action<ItemData, int> OnItemAdded;
        public event Action<ItemData, int> OnItemRemoved;
        public event Action<ItemData, int, int> OnItemQuantityChanged;
        public event Action OnInventoryChanged;
        
        // Properties
        public int InventorySize => inventorySize;
        public int UsedSlots => inventory.Count(slot => !slot.isEmpty);
        public bool IsFull => UsedSlots >= inventorySize;
        
        private void Awake()
        {
            InitializeInventory();
            InitializeItemTypeDictionary();
        }
        
        private void Start()
        {
            if (inventoryUI != null)
            {
                inventoryUI.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }
        }
        
        private void InitializeInventory()
        {
            inventory.Clear();
            for (int i = 0; i < inventorySize; i++)
            {
                inventory.Add(new InventorySlot());
            }
        }
        
        private void InitializeItemTypeDictionary()
        {
            itemsByType.Clear();
            foreach (ItemData.ItemType type in Enum.GetValues(typeof(ItemData.ItemType)))
            {
                itemsByType[type] = new List<InventorySlot>();
            }
        }
        
        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;
            
            // Check if item can be stacked
            if (itemData.maxStackSize > 1)
            {
                // Try to add to existing stack
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (!inventory[i].isEmpty && 
                        inventory[i].itemData == itemData && 
                        inventory[i].quantity < itemData.maxStackSize)
                    {
                        int canAdd = Mathf.Min(quantity, itemData.maxStackSize - inventory[i].quantity);
                        inventory[i].quantity += canAdd;
                        quantity -= canAdd;
                        
                        OnItemQuantityChanged?.Invoke(itemData, inventory[i].quantity, canAdd);
                        
                        if (quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }
            
            // Add to empty slots
            while (quantity > 0)
            {
                int emptySlotIndex = FindEmptySlot();
                if (emptySlotIndex == -1) return false; // No space
                
                int addAmount = Mathf.Min(quantity, itemData.maxStackSize);
                inventory[emptySlotIndex] = new InventorySlot(itemData, addAmount);
                quantity -= addAmount;
                
                // Update type dictionary
                itemsByType[itemData.itemType].Add(inventory[emptySlotIndex]);
                
                OnItemAdded?.Invoke(itemData, addAmount);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;
            
            int remainingToRemove = quantity;
            
            for (int i = inventory.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                if (!inventory[i].isEmpty && inventory[i].itemData == itemData)
                {
                    int removeAmount = Mathf.Min(remainingToRemove, inventory[i].quantity);
                    inventory[i].quantity -= removeAmount;
                    remainingToRemove -= removeAmount;
                    
                    OnItemQuantityChanged?.Invoke(itemData, inventory[i].quantity, -removeAmount);
                    
                    if (inventory[i].quantity <= 0)
                    {
                        // Remove from type dictionary
                        itemsByType[itemData.itemType].Remove(inventory[i]);
                        
                        inventory[i] = new InventorySlot();
                        OnItemRemoved?.Invoke(itemData, removeAmount);
                    }
                }
            }
            
            if (remainingToRemove == 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
            
            return false;
        }
        
        public bool HasItem(ItemData itemData, int quantity = 1)
        {
            int totalQuantity = 0;
            foreach (var slot in inventory)
            {
                if (!slot.isEmpty && slot.itemData == itemData)
                {
                    totalQuantity += slot.quantity;
                }
            }
            return totalQuantity >= quantity;
        }
        
        public int GetItemQuantity(ItemData itemData)
        {
            int totalQuantity = 0;
            foreach (var slot in inventory)
            {
                if (!slot.isEmpty && slot.itemData == itemData)
                {
                    totalQuantity += slot.quantity;
                }
            }
            return totalQuantity;
        }
        
        public List<InventorySlot> GetItemsByType(ItemData.ItemType type)
        {
            return itemsByType[type];
        }
        
        public InventorySlot GetSlot(int index)
        {
            if (index >= 0 && index < inventory.Count)
            {
                return inventory[index];
            }
            return null;
        }
        
        public bool SwapSlots(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= inventory.Count || 
                toIndex < 0 || toIndex >= inventory.Count)
                return false;
            
            var temp = inventory[fromIndex];
            inventory[fromIndex] = inventory[toIndex];
            inventory[toIndex] = temp;
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        public void ClearInventory()
        {
            inventory.Clear();
            InitializeInventory();
            InitializeItemTypeDictionary();
            OnInventoryChanged?.Invoke();
        }
        
        private int FindEmptySlot()
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].isEmpty)
                {
                    return i;
                }
            }
            return -1;
        }
        
        private void ToggleInventory()
        {
            if (inventoryUI != null)
            {
                bool isActive = inventoryUI.activeSelf;
                inventoryUI.SetActive(!isActive);
                
                // Pause game when inventory is open
                Time.timeScale = isActive ? 1f : 0f;
            }
        }
        
        public void UseItem(ItemData itemData)
        {
            if (itemData == null || !itemData.isConsumable) return;
            
            if (RemoveItem(itemData, 1))
            {
                // Apply item effects
                ApplyItemEffects(itemData);
            }
        }
        
        private void ApplyItemEffects(ItemData itemData)
        {
            // Apply health bonus
            if (itemData.healthBonus != 0)
            {
                // Implement health restoration
                Debug.Log($"Restored {itemData.healthBonus} health");
            }
            
            // Apply mana bonus
            if (itemData.manaBonus != 0)
            {
                // Implement mana restoration
                Debug.Log($"Restored {itemData.manaBonus} mana");
            }
            
            // Add other stat bonuses as needed
        }
    }
}