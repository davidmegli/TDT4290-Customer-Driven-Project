using UnityEngine; 
using System;
using System.Collections;
using System.Collections.Generic; 

/// <summary>
/// Manages level loading, transitions, and sequencing within the game.
/// Handles instantiation of level prefabs, listens for level completion events,
/// and coordinates interactions with other systems such as the elevator and voice router.
/// Implements a Singleton pattern for easy global access across scenes.
/// </summary>
public class LevelManager : MonoBehaviour
{ 
    public static LevelManager Instance { get; private set; }
    // Timestamp of the last level load. Other systems can use this to ignore
    // immediate collisions/spikes that happen right after a level is instantiated.
    [SerializeField] private VoiceAudioRouter voiceRouter;
    public static float lastLoadTime = -999f;
    // Global event that can be triggered by any level when completed 
    public static event Action OnLevelCompleted;
    [SerializeField] private Elevator elevator;
    public float delayBeforeActivatingElevator = 5.0f;
    public float delayBeforeActivatingElevatorFirstLevel = 15.0f;
    private List<GameObject> levels = new List<GameObject>();
    private GameObject currentLevelInstance;
    public static int currentLevelIndex = 0;

    /// <summary>
    /// Initializes the LevelManager singleton instance and loads all available level prefabs.
    /// Ensures the object persists across scene loads.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern for easy global access 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Load all level prefabs from Resources/Levels 
        LoadAllLevels();
    }
    
    /// <summary>
    /// Called when the game starts. Subscribes to level completion events,
    /// loads the initial level, and ensures the elevator is inactive at startup.
    /// </summary>
    private void Start()
    { 
        // Subscribe to the level-completed event 
        OnLevelCompleted += HandleLevelCompleted;
        // Load the first level at startup 
        LoadLevel(0); 
        // Ensure elevator is set to false after spawning in the first level 
        elevator.gameObject.SetActive(false);
    }

    /// <summary>
    /// Unsubscribes from level completion events when the LevelManager is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        OnLevelCompleted -= HandleLevelCompleted;
    }
    
    /// <summary>
    /// Handles logic to transition to the next level when the current one is completed.
    /// Initiates the coroutine that manages the transition delay and voice playback.
    /// </summary>
    private void HandleLevelCompleted()
    {
        StartCoroutine(LoadNextLevel());
    }

    /// <summary> 
    /// Loads all level prefabs dynamically from the Resources/Levels folder.
    /// Prefabs are sorted alphabetically to determine their play order.
    /// </summary> 
    private void LoadAllLevels()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Levels");
        // Sort levels alphabetically by name (e.g., "Level 1", "Level 2", ...)
        Array.Sort(loadedPrefabs, (a, b) => a.name.CompareTo(b.name));
        levels.AddRange(loadedPrefabs);
    }

    /// <summary> 
    /// Loads the level at the given index, destroying the previous level instance if necessary.
    /// Updates the current level index and records the load timestamp for physics safety.
    /// </summary> 
    private void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
        {
            return;
        }
        // Destroy current level if one exists
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }
        // Instantiate the new level at position (0,0,0)
        currentLevelInstance = Instantiate(levels[index], Vector3.zero, Quaternion.identity);
        currentLevelIndex = index;
        // Record when we finished instantiating the level so other scripts can
        // temporarily ignore noisy collision events that happen on spawn.
        lastLoadTime = Time.time;
    }

    /// <summary> 
    /// Coroutine that handles the transition to the next level.
    /// Waits for any active voice lines to finish playing before activating the elevator.
    /// </summary> 
    public IEnumerator LoadNextLevel()
    {
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        while (voiceRouter.IsAnyVoicePlaying())
        {
            yield return new WaitForSeconds(1.0f);
        }

        elevator.gameObject.SetActive(true);
    }

    /// <summary>
    /// Loads the next level in sequence immediately.
    /// Used by the elevator or other systems to advance the game.
    /// </summary>
    public static void StartNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        Instance.LoadLevel(nextIndex);
    }

    /// <summary> 
    /// Static helper method for levels to notify completion. 
    /// Triggers the global OnLevelCompleted event and initiates the transition sequence.
    /// </summary> 
    public static void LevelCompleted()
    {
        OnLevelCompleted?.Invoke();
    }
}
