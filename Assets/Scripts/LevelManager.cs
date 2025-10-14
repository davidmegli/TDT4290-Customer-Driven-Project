using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // Global event that can be triggered by any level when completed
    public static event Action OnLevelCompleted;

    [SerializeField] private Elevator elevator;

    private List<GameObject> levels = new List<GameObject>();
    private GameObject currentLevelInstance;
    private int currentLevelIndex = 0;

    private void Awake()
    {
        // Singleton pattern for easy global access
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load all level prefabs from Resources/Levels
        LoadAllLevels();
    }

    private void Start()
    {
        // Subscribe to the level-completed event
        OnLevelCompleted += LoadNextLevel;

        // Load the first level at startup
        LoadLevel(0);

        // added: ensure elevator is set to false after spawning in the first level
        elevator.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnLevelCompleted -= LoadNextLevel;
    }

    /// <summary>
    /// Loads all level prefabs dynamically from the Resources/Levels folder.
    /// </summary>
    private void LoadAllLevels()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Levels");

        // Sort levels alphabetically by name (e.g., "Level 1", "Level 2", ...)
        Array.Sort(loadedPrefabs, (a, b) => a.name.CompareTo(b.name));

        levels.AddRange(loadedPrefabs);
    }

    /// <summary>
    /// Loads the level at the given index and destroys the previous one.
    /// </summary>
    private void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
        {
            Debug.Log("🎉 All levels completed!");
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
        Debug.Log($"🔹 Loaded: {levels[index].name}");
    }

    /// <summary>
    /// Loads the next level in the list.
    /// </summary>
    public void LoadNextLevel()
    {
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }
        
        elevator.gameObject.SetActive(true);
    }

    public static void StartNextLevel()
    {
        int nextIndex = Instance.currentLevelIndex + 1;
        Instance.LoadLevel(nextIndex);
    }

    /// <summary>
    /// Static helper method for levels to notify completion.
    /// Can be called from any script inside a level.
    /// </summary>
    public static void LevelCompleted()
    {
        OnLevelCompleted?.Invoke();
    }
}
