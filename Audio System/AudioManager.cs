using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AudioManager : Node
{
    [ExportGroup("Configuration")]
    [Export] private float unloadDelay = 60f;  // Unload after 60s inactivity
    [Export] private int maxCachedSounds = 30;  // Prevent memory bloat
    [Export] private int preloadedPlayersNodesCount = 10;  // Prevent memory bloat
    [Export] private int preloadedPlayers3DNodesCount = 20;  // Prevent memory bloat
    [Export] private float loadedAudioCleanupTimer = 5f;  //Amount of seconds to check and clean audios
    [Export] private Node3D audioHolderNode;

    private float cleanupTimer;


    [ExportGroup("Audio Storage")]
    [ExportSubgroup("AudioStream Pool")]
    private Dictionary<string, AudioStream> loadedAudiosDic = new();
    private Dictionary<string, float> lastUsedTimesAudioDic = new();

    [ExportSubgroup("Nodes Pool")]
    private List<AudioStreamPlayer> playersNodesList = new();
    private List<AudioStreamPlayer3D> players3DNodesList = new();
    private List<Node> activePlayersList = new(); //This is used for pausing the game


    [ExportGroup("Audio Buses")]
    private Dictionary<AudioCategory, float> audioBusVolumeDb = new()
    {
        [AudioCategory.Master] = 0f,
        [AudioCategory.Music] = 0f,
        [AudioCategory.SFX] = 0f,
        [AudioCategory.UI] = 0f,
        [AudioCategory.Voice] = 0f
    };

    private Dictionary<AudioCategory, float> audioBusTempVolumeDbMultiplier = new()
    {
        [AudioCategory.Master] = 1f,
        [AudioCategory.Music] = 1f,
        [AudioCategory.SFX] = 1f,
        [AudioCategory.UI] = 1f,
        [AudioCategory.Voice] = 1f
    };


    public static AudioManager Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;

        for (int i = 0; i < preloadedPlayersNodesCount; i++)
        {
            AudioStreamPlayer playerNode = new AudioStreamPlayer();
            audioHolderNode.AddChild(playerNode);
            playersNodesList.Add(playerNode);
        }

        for (int i = 0; i < preloadedPlayers3DNodesCount; i++)
        {
            AudioStreamPlayer3D player3DNode = new AudioStreamPlayer3D();
            audioHolderNode.AddChild(player3DNode);
            players3DNodesList.Add(player3DNode);
        }

    }

    public override void _Process(double delta)
    {
        cleanupTimer -= (float)delta;
        if (cleanupTimer <= 0)
        {
            cleanupTimer = loadedAudioCleanupTimer;

            if (lastUsedTimesAudioDic.Count > 0)
            {
                var keysToRemove = lastUsedTimesAudioDic
                    .Where(kvp => (Time.GetTicksMsec() / 1000f) - kvp.Value > unloadDelay)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    loadedAudiosDic.Remove(key);
                    lastUsedTimesAudioDic.Remove(key);
                }
            }
        }
    }


    #region Player

    /// <summary>
    /// Called to play a sound using AudioStreamPlayer. 
    /// <para>The audio is pooled in memory.</para>
    /// <para>It's NOT a 3D space audio - call PlaySound3D for world-space audio.</para>
    /// </summary>
    /// <param name="path">Path to the audio file</param>
    /// <param name="category">Audio bus category (SFX/Music/etc.)</param>
    /// <param name="volumeDb">Volume in decibels (0 = default)</param>
    /// <param name="pitch">Pitch multiplier (1 = normal)</param>
    public void PlaySound(string path, AudioCategory category = AudioCategory.SFX, float volumeDb = 0f, float pitch = 1f)
    {
        //Stream is returned from the dic, if it doesn't exist, the if statement is read
        if (loadedAudiosDic.TryGetValue(path, out AudioStream stream) == false)
        {
            stream = GD.Load<AudioStream>(path);

            //Cache Limit, remove oldest audio there
            if (loadedAudiosDic.Count >= maxCachedSounds)
            {
                var oldestKey = lastUsedTimesAudioDic.MinBy(kvp => kvp.Value).Key;
                loadedAudiosDic.Remove(oldestKey);
                lastUsedTimesAudioDic.Remove(oldestKey);
            }

            //Add new audio
            loadedAudiosDic.Add(path, stream);
        }

        //Set time To now
        lastUsedTimesAudioDic[path] = Time.GetTicksMsec() / 1000f;

        AudioStreamPlayer playerNode = playersNodesList.First(p => p.Stream == null);

        //If no free players found, replace the first audio player, while this is extremely rare, it solves memory leaks and issues
        if (playerNode == null)
        {
            playerNode = playersNodesList[0];
        }

        playerNode.Stream = stream;
        playerNode.VolumeDb = volumeDb;
        playerNode.PitchScale = pitch;
        playerNode.Bus = category.ToString();
        playerNode.Play();

        //Track and auto-clean players
        activePlayersList.Add(playerNode);
        playerNode.Finished += () => CleanupPlayer(playerNode);
    }

    /// <summary>
    ///<para> Called to remove an audio after its done playing </para>
    ///<para> Removes the audio from the activePlayersList List </para>
    /// </summary>
    /// <param name="playerNode"></param>
    private void CleanupPlayer(AudioStreamPlayer playerNode)
    {
        activePlayersList.Remove(playerNode);
        playerNode.Stream = null;
    }

    #endregion

    #region Player 3D

    /// <summary>
    /// <para> Called to play a spatial 3d sound using AudioStreamPlayer3D, the audio is pooled in memory </para>
    /// <para> It's NOT to be used for UI or Music, call PlaySound for those cases </para>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    /// <param name="parent"></param>
    public void PlaySound3D(string path, Vector3 position, Node3D parent = null,
                           AudioCategory category = AudioCategory.SFX, float volumeDb = 0f, float pitch = 1f)
    {
        //Stream is returned from the dic, if it doesn't exist, the if statement is read
        if (loadedAudiosDic.TryGetValue(path, out AudioStream stream) == false)
        {
            stream = GD.Load<AudioStream>(path);

            //Cache Limit, remove oldest audio there
            if (loadedAudiosDic.Count >= maxCachedSounds)
            {
                var oldestKey = lastUsedTimesAudioDic.MinBy(kvp => kvp.Value).Key;
                loadedAudiosDic.Remove(oldestKey);
                lastUsedTimesAudioDic.Remove(oldestKey);
            }

            //Add new audio
            loadedAudiosDic.Add(path, stream);
        }

        //Set time To now
        lastUsedTimesAudioDic[path] = Time.GetTicksMsec() / 1000f;

        AudioStreamPlayer3D player3DNode = players3DNodesList.First(p => p.Stream == null);

        //If no free players 3D found, replace the first audio player, while this is extremely rare, it solves memory leaks and issues
        if (player3DNode == null)
        {
            player3DNode = players3DNodesList[0];
        }

        if (parent != null)
        {
            player3DNode.Reparent(parent);
        }

        player3DNode.GlobalPosition = position;

        player3DNode.Stream = stream;
        player3DNode.VolumeDb = volumeDb;
        player3DNode.PitchScale = pitch;
        player3DNode.Bus = category.ToString();
        player3DNode.Play();

        //Track and auto-clean players
        activePlayersList.Add(player3DNode);
        player3DNode.Finished += () => CleanupPlayer3D(player3DNode);
    }

    /// <summary>
    /// <para> Called to remove an audio after its done playing </para>
    /// <para> Removes the audio from the active3DPlayersList List </para>
    /// </summary>
    /// <param name="player3DNode"></param>
    private void CleanupPlayer3D(AudioStreamPlayer3D player3DNode)
    {
        activePlayersList.Remove(player3DNode);
        player3DNode.GlobalPosition = Vector3.Zero;
        player3DNode.Reparent(audioHolderNode);
        player3DNode.Stream = null;
    }

    #endregion

    #region  Bus Manager

    /// <summary>
    /// <para> Used for settings to set the audio of each bus separately </para>
    /// </summary>
    /// <param name="busName"></param>
    /// <param name="volumeDb"></param>
    /// <param name="fadeDuration"></param>
    public void SetBusVolume(AudioCategory category, float volumeDb)
    {
        audioBusVolumeDb[category] = volumeDb;
        UpdateFinalVolume(category);
    }

    /// <summary>
    /// <para> Used for Calling fadeDuration:(float) to smoothly change the audio bus, if a scene requires the music to calm down a bit for example </para>
    /// </summary>
    /// <param name="busName"></param>
    /// <param name="volumeDb"></param>
    /// <param name="fadeDuration"></param>
    public void SetBusVolumeTemp(AudioCategory category, float targetMultiplier, float fadeDuration = 0f)
    {
        var busIdx = AudioServer.GetBusIndex(category.ToString());
        float currentMultiplier = audioBusTempVolumeDbMultiplier.GetValueOrDefault(category, 1f);

        // Instant change
        if (fadeDuration <= 0)
        {
            audioBusTempVolumeDbMultiplier[category] = targetMultiplier;
            UpdateFinalVolume(category);
        }
        else // Smooth tween of the multiplier
        {
            CreateTween()
                .TweenMethod(
                    Callable.From<float>(m =>
                    {
                        audioBusTempVolumeDbMultiplier[category] = m;
                        UpdateFinalVolume(category);
                    }),
                    currentMultiplier,
                    targetMultiplier,
                    fadeDuration
                );
        }
    }

    /// <summary>
    /// Updates specific bus volumeDb
    /// </summary>
    /// <param name="category"></param>
    public void UpdateFinalVolume(AudioCategory category)
    {
        int busIdx = AudioServer.GetBusIndex(category.ToString());
        float baseVolume = audioBusVolumeDb.GetValueOrDefault(category, 0f);
        float multiplier = audioBusTempVolumeDbMultiplier.GetValueOrDefault(category, 1f);

        AudioServer.SetBusVolumeDb(busIdx, baseVolume + Mathf.Log(multiplier) * 20f);
    }

    #endregion

}

public enum AudioCategory
{
    Master,
    Music,
    SFX,
    UI,
    Voice
}