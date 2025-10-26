# Unity Template Scripts - Usage Guide

This guide will help you understand how to use the Unity template scripts in your projects.

## Quick Start

1. **Copy the desired scripts** from the appropriate folder to your Unity project
2. **Create the necessary ScriptableObjects** using the Create menu
3. **Attach scripts to GameObjects** and configure them in the Inspector
4. **Customize the scripts** to fit your specific game needs

## Script Categories

### 2D Games

#### Platformer Scripts
- **PlayerController2D.cs**: Complete 2D platformer player controller with jump, movement, and animation support
- **CameraFollow2D.cs**: Smooth camera following with look-ahead and bounds
- **Collectible2D.cs**: Collectible items with effects and animations
- **CollectibleManager2D.cs**: Manages collectible collection and UI updates
- **EnemyAI2D.cs**: AI system for 2D enemies with patrol, chase, and attack behaviors

#### Top-Down RPG Scripts
- **PlayerControllerTopDown.cs**: 2D top-down movement controller
- **InventorySystem.cs**: Complete inventory system with items, stacking, and UI
- **DialogueSystem.cs**: Dialogue system with choices, events, and character portraits

### 3D Games

#### Platformer Scripts
- **PlayerController3D.cs**: 3D character controller with mouse look and movement
- **CameraFollow3D.cs**: 3D camera following with collision and bounds

#### FPS Scripts
- **PlayerControllerFPS.cs**: First-person shooter controller with crouching and mouse look
- **WeaponSystem.cs**: Complete weapon system with recoil, ammo, and hit detection

### iOS Games

#### Touch Controls
- **TouchControls.cs**: Virtual joystick and touch button system
- **HapticFeedback.cs**: Haptic feedback system for iOS and Android

### General Utilities

#### UI Management
- **UIManager.cs**: Centralized UI management with panel switching and pause functionality

#### Audio
- **AudioManager.cs**: Complete audio management system with music, SFX, and ambient sounds

#### Scene Management
- **SceneLoader.cs**: Scene loading with fade transitions and loading screens

#### Data Management
- **SaveSystem.cs**: Save/load system with encryption and auto-save

### ScriptableObjects

#### Data Templates
- **GameData.cs**: Central game configuration and settings
- **LevelData.cs**: Level configuration with objectives and spawns
- **PlayerStats.cs**: Player statistics and progression system

## Setup Instructions

### 1. Basic Setup

1. **Import Scripts**: Copy the desired scripts to your Unity project
2. **Create Folders**: Organize scripts in appropriate folders (Scripts/Player, Scripts/UI, etc.)
3. **Set Up Tags**: Create the following tags in your project:
   - Player
   - Enemy
   - Collectible
   - Ground
   - UI

### 2. Player Controller Setup

#### 2D Platformer
```csharp
// 1. Add PlayerController2D to your player GameObject
// 2. Assign required components:
//    - Rigidbody2D
//    - CapsuleCollider2D
//    - Animator (optional)
//    - SpriteRenderer (optional)
// 3. Create a GroundCheck child object
// 4. Set the Ground Layer Mask
// 5. Configure movement and jump settings
```

#### 3D Platformer
```csharp
// 1. Add PlayerController3D to your player GameObject
// 2. Assign required components:
//    - CharacterController
//    - Camera (for mouse look)
// 3. Configure movement and camera settings
// 4. Set up input axes in Input Manager
```

### 3. Camera Setup

#### 2D Camera
```csharp
// 1. Add CameraFollow2D to your main camera
// 2. Assign the player as the target
// 3. Configure offset and follow settings
// 4. Set up bounds if needed
```

#### 3D Camera
```csharp
// 1. Add CameraFollow3D to your main camera
// 2. Assign the player as the target
// 3. Configure offset and follow settings
// 4. Set up collision and bounds
```

### 4. UI Setup

```csharp
// 1. Add UIManager to a GameObject in your scene
// 2. Create UI panels (MainMenu, Gameplay, Pause, etc.)
// 3. Assign panels to the UIManager
// 4. Set up button events to call UIManager methods
```

### 5. Audio Setup

```csharp
// 1. Add AudioManager to a GameObject in your scene
// 2. Create an AudioData ScriptableObject
// 3. Assign audio clips to the AudioData
// 4. Use AudioManager.Instance to play sounds
```

### 6. Save System Setup

```csharp
// 1. Add SaveSystem to a GameObject in your scene
// 2. Implement ISaveable interface on objects you want to save
// 3. Use SaveSystem.Instance to save/load games
```

## Customization Guide

### 1. Modifying Player Controllers

#### Adding New Movement Types
```csharp
// In PlayerController2D.cs, add new movement methods:
private void HandleWallJump()
{
    // Wall jump logic
}

private void HandleDash()
{
    // Dash logic
}
```

#### Adding New Input Methods
```csharp
// Add new input handling:
private void HandleSpecialInput()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        // Special action
    }
}
```

### 2. Customizing UI Systems

#### Adding New Panels
```csharp
// In UIManager.cs, add new panel references:
[SerializeField] private GameObject settingsPanel;
[SerializeField] private GameObject inventoryPanel;

// Add methods to show/hide panels:
public void ShowSettings()
{
    OpenPanel(settingsPanel);
}
```

#### Customizing Inventory
```csharp
// In InventorySystem.cs, add new item types:
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Quest,
    Misc,
    // Add your custom types
    Potion,
    Scroll,
    Gem
}
```

### 3. Extending Audio System

#### Adding New Audio Categories
```csharp
// In AudioManager.cs, add new audio sources:
[SerializeField] private AudioSource voiceSource;
[SerializeField] private AudioSource environmentSource;

// Add methods for new audio types:
public void PlayVoice(string clipName, float volume = 1f)
{
    // Voice audio logic
}
```

### 4. Customizing Save System

#### Adding New Save Data
```csharp
// In SaveSystem.cs, extend the SaveData class:
[System.Serializable]
public class SaveData
{
    // Existing fields...
    public int playerLevel;
    public float playTime;
    public List<string> unlockedItems;
}
```

## Best Practices

### 1. Code Organization
- Keep scripts in appropriate folders
- Use namespaces to avoid conflicts
- Comment your code thoroughly
- Follow Unity naming conventions

### 2. Performance
- Use object pooling for frequently spawned objects
- Cache references to avoid repeated GetComponent calls
- Use events instead of Update for infrequent checks
- Optimize for your target platform

### 3. Memory Management
- Dispose of resources properly
- Use weak references where appropriate
- Avoid memory leaks in coroutines
- Profile your game regularly

### 4. Platform Considerations
- Test on all target platforms
- Use platform-specific code when needed
- Consider different input methods
- Optimize for different screen sizes

## Troubleshooting

### Common Issues

#### Player Controller Not Moving
- Check if Rigidbody2D/CharacterController is assigned
- Verify input axes are set up correctly
- Check if the player is grounded (for jump)

#### Camera Not Following
- Ensure the target is assigned
- Check if the camera script is enabled
- Verify the follow settings are correct

#### UI Not Showing
- Check if the UI Manager is in the scene
- Verify panels are assigned
- Ensure the UI canvas is set up correctly

#### Audio Not Playing
- Check if AudioManager is in the scene
- Verify audio clips are assigned
- Check volume settings
- Ensure audio sources are configured

#### Save System Not Working
- Check if SaveSystem is in the scene
- Verify objects implement ISaveable
- Check file permissions
- Ensure save path is accessible

### Debug Tips

1. **Use Debug.Log()** to track script execution
2. **Check the Console** for error messages
3. **Use the Inspector** to verify component assignments
4. **Test in Play Mode** to see runtime behavior
5. **Use Unity Profiler** to check performance

## Advanced Features

### 1. Event System
Most scripts include event systems for communication:
```csharp
// Subscribe to events
playerController.OnJump += HandlePlayerJump;
playerController.OnLand += HandlePlayerLand;

// Unsubscribe when done
playerController.OnJump -= HandlePlayerJump;
```

### 2. ScriptableObject Integration
Use ScriptableObjects for data-driven design:
```csharp
// Create data assets
[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float fireRate;
}
```

### 3. Custom Editors
Create custom inspectors for better workflow:
```csharp
[CustomEditor(typeof(PlayerController2D))]
public class PlayerController2DEditor : Editor
{
    // Custom inspector code
}
```

## Support and Updates

### Getting Help
- Check the Unity documentation
- Search Unity forums
- Ask questions in Unity communities
- Review the script comments

### Updating Scripts
- Keep backups of your customized scripts
- Compare changes before updating
- Test thoroughly after updates
- Document your customizations

### Contributing
- Report bugs and issues
- Suggest improvements
- Share your customizations
- Help others in the community

## License and Credits

These template scripts are provided as-is for educational and development purposes. Feel free to modify and use them in your projects. Remember to:

- Give credit where appropriate
- Test thoroughly before release
- Follow Unity's terms of service
- Respect third-party asset licenses

---

Happy game development! 🎮