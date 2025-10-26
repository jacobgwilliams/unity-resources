using UnityEngine;
using System.Collections.Generic;

namespace UnityTemplates.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Unity Templates/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Basic Info")]
        public string levelName = "Level 1";
        public string levelDescription = "Level description";
        public int levelNumber = 1;
        public Sprite levelThumbnail;
        
        [Header("Scene Settings")]
        public string sceneName = "Level1";
        public bool isUnlocked = true;
        public int requiredLevelsCompleted = 0;
        
        [Header("Gameplay Settings")]
        public float timeLimit = 300f; // 5 minutes
        public int maxLives = 3;
        public int maxHealth = 100;
        public int maxMana = 50;
        
        [Header("Objectives")]
        public List<Objective> objectives = new List<Objective>();
        public bool allObjectivesRequired = true;
        
        [Header("Enemies")]
        public List<EnemySpawnData> enemySpawns = new List<EnemySpawnData>();
        public float enemySpawnRate = 1f;
        public int maxEnemies = 10;
        
        [Header("Collectibles")]
        public List<CollectibleSpawnData> collectibleSpawns = new List<CollectibleSpawnData>();
        public int totalCollectibles = 0;
        
        [Header("Environment")]
        public Vector3 playerSpawnPosition = Vector3.zero;
        public Vector3 cameraStartPosition = Vector3.zero;
        public Color ambientLightColor = Color.white;
        public float ambientLightIntensity = 1f;
        
        [Header("Audio")]
        public AudioClip backgroundMusic;
        public AudioClip ambientSound;
        public float musicVolume = 0.7f;
        public float ambientVolume = 0.5f;
        
        [Header("Rewards")]
        public int experienceReward = 100;
        public int coinReward = 50;
        public List<string> itemRewards = new List<string>();
        public List<string> achievementRewards = new List<string>();
        
        [System.Serializable]
        public class Objective
        {
            public string objectiveName;
            public string objectiveDescription;
            public ObjectiveType type;
            public int targetValue;
            public int currentValue;
            public bool isCompleted;
            public bool isOptional;
            
            public enum ObjectiveType
            {
                CollectItems,
                DefeatEnemies,
                SurviveTime,
                ReachLocation,
                CompletePuzzle,
                FindSecret,
                NoDamage,
                SpeedRun
            }
        }
        
        [System.Serializable]
        public class EnemySpawnData
        {
            public GameObject enemyPrefab;
            public Vector3 spawnPosition;
            public float spawnDelay = 0f;
            public bool spawnOnStart = true;
            public int maxSpawns = 1;
            public float respawnTime = 0f;
        }
        
        [System.Serializable]
        public class CollectibleSpawnData
        {
            public GameObject collectiblePrefab;
            public Vector3 spawnPosition;
            public bool spawnOnStart = true;
            public float respawnTime = 0f;
            public int value = 1;
        }
        
        // Methods for runtime modifications
        public void CompleteObjective(int objectiveIndex)
        {
            if (objectiveIndex >= 0 && objectiveIndex < objectives.Count)
            {
                objectives[objectiveIndex].isCompleted = true;
            }
        }
        
        public void UpdateObjectiveProgress(int objectiveIndex, int progress)
        {
            if (objectiveIndex >= 0 && objectiveIndex < objectives.Count)
            {
                objectives[objectiveIndex].currentValue = progress;
                if (objectives[objectiveIndex].currentValue >= objectives[objectiveIndex].targetValue)
                {
                    objectives[objectiveIndex].isCompleted = true;
                }
            }
        }
        
        public bool AreAllRequiredObjectivesCompleted()
        {
            foreach (var objective in objectives)
            {
                if (!objective.isOptional && !objective.isCompleted)
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool AreAllObjectivesCompleted()
        {
            foreach (var objective in objectives)
            {
                if (!objective.isCompleted)
                {
                    return false;
                }
            }
            return true;
        }
        
        public int GetCompletedObjectiveCount()
        {
            int count = 0;
            foreach (var objective in objectives)
            {
                if (objective.isCompleted)
                {
                    count++;
                }
            }
            return count;
        }
        
        public float GetObjectiveCompletionPercentage()
        {
            if (objectives.Count == 0) return 100f;
            
            int completed = GetCompletedObjectiveCount();
            return (float)completed / objectives.Count * 100f;
        }
        
        public void ResetObjectives()
        {
            foreach (var objective in objectives)
            {
                objective.currentValue = 0;
                objective.isCompleted = false;
            }
        }
        
        public void UnlockLevel()
        {
            isUnlocked = true;
        }
        
        public void LockLevel()
        {
            isUnlocked = false;
        }
        
        public bool CanUnlock(int completedLevels)
        {
            return completedLevels >= requiredLevelsCompleted;
        }
        
        public void SetPlayerSpawn(Vector3 position)
        {
            playerSpawnPosition = position;
        }
        
        public void SetCameraStart(Vector3 position)
        {
            cameraStartPosition = position;
        }
        
        public void AddEnemySpawn(GameObject enemyPrefab, Vector3 position, float delay = 0f)
        {
            EnemySpawnData spawnData = new EnemySpawnData
            {
                enemyPrefab = enemyPrefab,
                spawnPosition = position,
                spawnDelay = delay,
                spawnOnStart = delay == 0f
            };
            enemySpawns.Add(spawnData);
        }
        
        public void AddCollectibleSpawn(GameObject collectiblePrefab, Vector3 position, int value = 1)
        {
            CollectibleSpawnData spawnData = new CollectibleSpawnData
            {
                collectiblePrefab = collectiblePrefab,
                spawnPosition = position,
                value = value,
                spawnOnStart = true
            };
            collectibleSpawns.Add(spawnData);
        }
        
        public void SetBackgroundMusic(AudioClip music, float volume = 0.7f)
        {
            backgroundMusic = music;
            musicVolume = volume;
        }
        
        public void SetAmbientSound(AudioClip ambient, float volume = 0.5f)
        {
            ambientSound = ambient;
            ambientVolume = volume;
        }
        
        public void SetTimeLimit(float time)
        {
            timeLimit = time;
        }
        
        public void SetMaxLives(int lives)
        {
            maxLives = lives;
        }
        
        public void SetMaxHealth(int health)
        {
            maxHealth = health;
        }
        
        public void SetMaxMana(int mana)
        {
            maxMana = mana;
        }
        
        public void SetRewards(int experience, int coins, List<string> items = null, List<string> achievements = null)
        {
            experienceReward = experience;
            coinReward = coins;
            
            if (items != null)
            {
                itemRewards = items;
            }
            
            if (achievements != null)
            {
                achievementRewards = achievements;
            }
        }
    }
}