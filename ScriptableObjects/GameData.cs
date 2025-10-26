using UnityEngine;
using System.Collections.Generic;

namespace UnityTemplates.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameData", menuName = "Unity Templates/Game Data")]
    public class GameData : ScriptableObject
    {
        [Header("Game Settings")]
        public string gameName = "My Game";
        public string version = "1.0.0";
        public string description = "Game description";
        
        [Header("Player Settings")]
        public float playerSpeed = 5f;
        public float playerJumpForce = 10f;
        public int playerMaxHealth = 100;
        public int playerMaxMana = 50;
        
        [Header("Gameplay Settings")]
        public float gameTime = 300f; // 5 minutes
        public int maxLives = 3;
        public float respawnTime = 3f;
        public bool allowPause = true;
        
        [Header("Difficulty Settings")]
        public DifficultyLevel difficulty = DifficultyLevel.Normal;
        public float enemySpeedMultiplier = 1f;
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        
        [Header("Audio Settings")]
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1f;
        public float ambientVolume = 0.5f;
        
        [Header("Graphics Settings")]
        public int targetFrameRate = 60;
        public bool vsyncEnabled = true;
        public int qualityLevel = 2;
        public bool fullscreen = false;
        
        [Header("Input Settings")]
        public float mouseSensitivity = 2f;
        public bool invertY = false;
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode fireKey = KeyCode.Mouse0;
        public KeyCode reloadKey = KeyCode.R;
        public KeyCode pauseKey = KeyCode.Escape;
        
        [Header("UI Settings")]
        public bool showFPS = false;
        public bool showDebugInfo = false;
        public float uiScale = 1f;
        public bool hideUIInGameplay = false;
        
        [Header("Save Data")]
        public int highScore = 0;
        public int totalPlayTime = 0;
        public int levelsCompleted = 0;
        public List<string> unlockedAchievements = new List<string>();
        public List<string> unlockedItems = new List<string>();
        
        public enum DifficultyLevel
        {
            Easy,
            Normal,
            Hard,
            Expert
        }
        
        // Methods for runtime modifications
        public void SetDifficulty(DifficultyLevel newDifficulty)
        {
            difficulty = newDifficulty;
            
            switch (newDifficulty)
            {
                case DifficultyLevel.Easy:
                    enemySpeedMultiplier = 0.7f;
                    enemyHealthMultiplier = 0.8f;
                    enemyDamageMultiplier = 0.6f;
                    break;
                case DifficultyLevel.Normal:
                    enemySpeedMultiplier = 1f;
                    enemyHealthMultiplier = 1f;
                    enemyDamageMultiplier = 1f;
                    break;
                case DifficultyLevel.Hard:
                    enemySpeedMultiplier = 1.3f;
                    enemyHealthMultiplier = 1.5f;
                    enemyDamageMultiplier = 1.5f;
                    break;
                case DifficultyLevel.Expert:
                    enemySpeedMultiplier = 1.6f;
                    enemyHealthMultiplier = 2f;
                    enemyDamageMultiplier = 2f;
                    break;
            }
        }
        
        public void UpdateHighScore(int newScore)
        {
            if (newScore > highScore)
            {
                highScore = newScore;
            }
        }
        
        public void AddPlayTime(int seconds)
        {
            totalPlayTime += seconds;
        }
        
        public void CompleteLevel()
        {
            levelsCompleted++;
        }
        
        public void UnlockAchievement(string achievementId)
        {
            if (!unlockedAchievements.Contains(achievementId))
            {
                unlockedAchievements.Add(achievementId);
            }
        }
        
        public void UnlockItem(string itemId)
        {
            if (!unlockedItems.Contains(itemId))
            {
                unlockedItems.Add(itemId);
            }
        }
        
        public bool IsAchievementUnlocked(string achievementId)
        {
            return unlockedAchievements.Contains(achievementId);
        }
        
        public bool IsItemUnlocked(string itemId)
        {
            return unlockedItems.Contains(itemId);
        }
        
        public void ResetProgress()
        {
            highScore = 0;
            totalPlayTime = 0;
            levelsCompleted = 0;
            unlockedAchievements.Clear();
            unlockedItems.Clear();
        }
        
        public void ResetSettings()
        {
            masterVolume = 1f;
            musicVolume = 0.7f;
            sfxVolume = 1f;
            ambientVolume = 0.5f;
            mouseSensitivity = 2f;
            invertY = false;
            showFPS = false;
            showDebugInfo = false;
            uiScale = 1f;
            hideUIInGameplay = false;
        }
    }
}