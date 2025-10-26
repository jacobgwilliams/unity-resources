using UnityEngine;
using System.Collections.Generic;

namespace UnityTemplates.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Unity Templates/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Basic Stats")]
        public int level = 1;
        public int experience = 0;
        public int experienceToNextLevel = 100;
        public int skillPoints = 0;
        
        [Header("Health & Mana")]
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int maxMana = 50;
        public int currentMana = 50;
        public float healthRegenRate = 1f;
        public float manaRegenRate = 2f;
        
        [Header("Combat Stats")]
        public int attack = 10;
        public int defense = 5;
        public int magic = 8;
        public int speed = 12;
        public int luck = 10;
        public float criticalChance = 0.05f;
        public float criticalMultiplier = 2f;
        
        [Header("Movement Stats")]
        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public float acceleration = 10f;
        public float deceleration = 10f;
        public float airControl = 0.5f;
        
        [Header("Resistances")]
        public float fireResistance = 0f;
        public float iceResistance = 0f;
        public float lightningResistance = 0f;
        public float poisonResistance = 0f;
        public float physicalResistance = 0f;
        
        [Header("Skills")]
        public List<Skill> skills = new List<Skill>();
        public int availableSkillPoints = 0;
        
        [Header("Equipment")]
        public EquipmentSlot head;
        public EquipmentSlot chest;
        public EquipmentSlot legs;
        public EquipmentSlot feet;
        public EquipmentSlot weapon;
        public EquipmentSlot shield;
        public EquipmentSlot accessory1;
        public EquipmentSlot accessory2;
        
        [Header("Inventory")]
        public int maxInventorySlots = 20;
        public List<InventoryItem> inventory = new List<InventoryItem>();
        
        [Header("Achievements")]
        public List<string> unlockedAchievements = new List<string>();
        public int totalAchievements = 0;
        
        [System.Serializable]
        public class Skill
        {
            public string skillName;
            public string description;
            public int level;
            public int maxLevel;
            public int costPerLevel;
            public SkillType type;
            public bool isUnlocked;
            
            public enum SkillType
            {
                Combat,
                Magic,
                Movement,
                Survival,
                Crafting,
                Social
            }
        }
        
        [System.Serializable]
        public class EquipmentSlot
        {
            public ItemData item;
            public bool isEmpty => item == null;
            
            public void Equip(ItemData newItem)
            {
                item = newItem;
            }
            
            public void Unequip()
            {
                item = null;
            }
        }
        
        [System.Serializable]
        public class InventoryItem
        {
            public ItemData item;
            public int quantity;
            public bool isEmpty => item == null || quantity <= 0;
        }
        
        // Methods for stat modifications
        public void AddExperience(int amount)
        {
            experience += amount;
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            experience -= experienceToNextLevel;
            level++;
            skillPoints += 2;
            availableSkillPoints += 2;
            
            // Increase stats on level up
            maxHealth += 10;
            maxMana += 5;
            attack += 2;
            defense += 1;
            magic += 2;
            speed += 1;
            luck += 1;
            
            // Restore health and mana
            currentHealth = maxHealth;
            currentMana = maxMana;
            
            // Increase experience needed for next level
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
        }
        
        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - defense);
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }
        
        public void UseMana(int amount)
        {
            currentMana = Mathf.Max(0, currentMana - amount);
        }
        
        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
        }
        
        public void UpdateRegeneration()
        {
            if (currentHealth < maxHealth)
            {
                Heal(Mathf.RoundToInt(healthRegenRate * Time.deltaTime));
            }
            
            if (currentMana < maxMana)
            {
                RestoreMana(Mathf.RoundToInt(manaRegenRate * Time.deltaTime));
            }
        }
        
        public void UpgradeSkill(int skillIndex)
        {
            if (skillIndex >= 0 && skillIndex < skills.Count && availableSkillPoints > 0)
            {
                Skill skill = skills[skillIndex];
                if (skill.level < skill.maxLevel)
                {
                    skill.level++;
                    availableSkillPoints--;
                    ApplySkillBonuses(skill);
                }
            }
        }
        
        private void ApplySkillBonuses(Skill skill)
        {
            // Apply skill-specific bonuses
            switch (skill.type)
            {
                case Skill.SkillType.Combat:
                    attack += 1;
                    criticalChance += 0.01f;
                    break;
                case Skill.SkillType.Magic:
                    magic += 1;
                    maxMana += 5;
                    break;
                case Skill.SkillType.Movement:
                    moveSpeed += 0.5f;
                    jumpForce += 1f;
                    break;
                case Skill.SkillType.Survival:
                    maxHealth += 5;
                    healthRegenRate += 0.1f;
                    break;
                case Skill.SkillType.Crafting:
                    // Implement crafting bonuses
                    break;
                case Skill.SkillType.Social:
                    // Implement social bonuses
                    break;
            }
        }
        
        public void EquipItem(ItemData item, EquipmentSlot slot)
        {
            if (slot != null && item != null)
            {
                // Unequip current item if any
                if (!slot.isEmpty)
                {
                    UnequipItem(slot);
                }
                
                slot.Equip(item);
                ApplyEquipmentBonuses(item);
            }
        }
        
        public void UnequipItem(EquipmentSlot slot)
        {
            if (slot != null && !slot.isEmpty)
            {
                RemoveEquipmentBonuses(slot.item);
                slot.Unequip();
            }
        }
        
        private void ApplyEquipmentBonuses(ItemData item)
        {
            maxHealth += item.healthBonus;
            maxMana += item.manaBonus;
            attack += item.attackBonus;
            defense += item.defenseBonus;
            speed += item.speedBonus;
        }
        
        private void RemoveEquipmentBonuses(ItemData item)
        {
            maxHealth -= item.healthBonus;
            maxMana -= item.manaBonus;
            attack -= item.attackBonus;
            defense -= item.defenseBonus;
            speed -= item.speedBonus;
        }
        
        public void AddItemToInventory(ItemData item, int quantity = 1)
        {
            // Check if item can be stacked
            if (item.maxStackSize > 1)
            {
                // Try to add to existing stack
                foreach (var inventoryItem in inventory)
                {
                    if (!inventoryItem.isEmpty && inventoryItem.item == item && inventoryItem.quantity < item.maxStackSize)
                    {
                        int canAdd = Mathf.Min(quantity, item.maxStackSize - inventoryItem.quantity);
                        inventoryItem.quantity += canAdd;
                        quantity -= canAdd;
                        
                        if (quantity <= 0) return;
                    }
                }
            }
            
            // Add to new slot
            if (inventory.Count < maxInventorySlots)
            {
                inventory.Add(new InventoryItem { item = item, quantity = quantity });
            }
        }
        
        public void RemoveItemFromInventory(ItemData item, int quantity = 1)
        {
            for (int i = inventory.Count - 1; i >= 0 && quantity > 0; i--)
            {
                if (!inventory[i].isEmpty && inventory[i].item == item)
                {
                    int removeAmount = Mathf.Min(quantity, inventory[i].quantity);
                    inventory[i].quantity -= removeAmount;
                    quantity -= removeAmount;
                    
                    if (inventory[i].quantity <= 0)
                    {
                        inventory.RemoveAt(i);
                    }
                }
            }
        }
        
        public void UnlockAchievement(string achievementId)
        {
            if (!unlockedAchievements.Contains(achievementId))
            {
                unlockedAchievements.Add(achievementId);
                totalAchievements++;
            }
        }
        
        public bool HasAchievement(string achievementId)
        {
            return unlockedAchievements.Contains(achievementId);
        }
        
        public void ResetStats()
        {
            level = 1;
            experience = 0;
            experienceToNextLevel = 100;
            skillPoints = 0;
            availableSkillPoints = 0;
            
            maxHealth = 100;
            currentHealth = 100;
            maxMana = 50;
            currentMana = 50;
            
            attack = 10;
            defense = 5;
            magic = 8;
            speed = 12;
            luck = 10;
            
            moveSpeed = 5f;
            jumpForce = 10f;
            
            // Reset skills
            foreach (var skill in skills)
            {
                skill.level = 0;
                skill.isUnlocked = false;
            }
            
            // Clear equipment
            head.Unequip();
            chest.Unequip();
            legs.Unequip();
            feet.Unequip();
            weapon.Unequip();
            shield.Unequip();
            accessory1.Unequip();
            accessory2.Unequip();
            
            // Clear inventory
            inventory.Clear();
            
            // Clear achievements
            unlockedAchievements.Clear();
            totalAchievements = 0;
        }
        
        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }
        
        public float GetManaPercentage()
        {
            return (float)currentMana / maxMana;
        }
        
        public float GetExperiencePercentage()
        {
            return (float)experience / experienceToNextLevel;
        }
        
        public bool IsAlive()
        {
            return currentHealth > 0;
        }
        
        public bool HasMana(int amount)
        {
            return currentMana >= amount;
        }
    }
}