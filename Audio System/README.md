# Audio System (C#)

Godot 4 C# audio manager with pooling and 3D support.

Currently in early stages, will be updated to fit all cases.

## Installation

1. Place the folder in your Godot FileSystem.  
2. Create a Node in your main game scene and attach the `AudioManager` script to it. Optionally rename the node to "Audio Manager".  
3. In the Audio panel, change the Audio Bus to use `default_bus_layout`.

## Important Configuration

The `AudioManager` node can be configured for specific use cases:

```csharp
[ExportGroup("Configuration")]
[Export] private float unloadDelay = 60f;               // Lifespan of a pooled audio before unloading
[Export] private int maxCachedSounds = 30;              // Max audio pooling count
[Export] private float loadedAudioCleanupTimer = 5f;    // Interval to check for unloading

[Export] private int preloadedPlayersNodesCount = 10;   // Preloaded AudioStreamPlayer nodes
[Export] private int preloadedPlayers3DNodesCount = 20; // Preloaded AudioStreamPlayer3D nodes
```

## Information

`AudioManager` handles backend logic and creates `AudioStreamPlayer`/`AudioStreamPlayer3D` nodes inside a child `audioHolderNode`.

## Usage Functions

**Play Sound**  
Plays audio using an `AudioStreamPlayer` node.  
`PlaySound(string path, AudioCategory category = AudioCategory.SFX, float volumeDb = 0f, float pitch = 1f)`

**Play Sound 3D**  
Plays audio using an `AudioStreamPlayer3D` node.  
`PlaySound3D(string path, Vector3 position, Node3D parent = null, AudioCategory category = AudioCategory.SFX, float volumeDb = 0f, float pitch = 1f)`

- `position` sets the global position.  
- `parent` sets the node parent.  
- After playing, the node will be reparented to `audioHolderNode`.

**Changing Bus Volumes**  
Sets the volume of a specific bus.  
`SetBusVolume(AudioCategory category, float volumeDb)`  
Example:  
`SetBusVolume(AudioCategory.Music, 2f);`

**Changing Bus Volume Multiplier**  
Temporarily changes the volume (e.g. for cutscenes).  
`SetBusVolumeMultiplier(AudioCategory category, float targetMultiplier, float fadeDuration = 0f)`  
Example:  
`SetBusVolumeMultiplier(AudioCategory.Music, 0.5f, 0.4f);`
