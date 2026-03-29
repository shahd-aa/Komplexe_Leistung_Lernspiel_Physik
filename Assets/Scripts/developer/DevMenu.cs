using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class DevMenu : MonoBehaviour
{
    private bool devMenuOpen = false;
    private bool showConsole = false;
    private bool showStats = false;
    private bool timelinePaused = false;

    private static DevMenu instance;
    private PlayableDirector currentTimeline; // ← cached timeline
    private List<IPausable> pausables = new List<IPausable>();

    private List<string> logMessages = new List<string>();
    private Vector2 consoleScroll;
    private float deltaTime = 0f;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        SceneManager.sceneLoaded += OnSceneLoaded; // ← listen for scene changes
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        SceneManager.sceneLoaded -= OnSceneLoaded; // ← clean up
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        logMessages.Add($"[{type}] {message}");
        if (logMessages.Count > 100)
            logMessages.RemoveAt(0);
    }

    // ← fires every time a new scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentTimeline = FindFirstObjectByType<PlayableDirector>();
        timelinePaused = false;

        // finds everything that implements IPausable in the scene
        pausables.Clear();
        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPausable p)
                pausables.Add(p);
        }
    }
    void Update()
    {
        // Ctrl+Shift+D → toggle dev menu
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.D))
        {
            devMenuOpen = !devMenuOpen;
        }

        // Ctrl+Space → pause/unpause timeline
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTimeline();
        }

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void ToggleTimeline()
    {
        if (currentTimeline == null)
            currentTimeline = FindFirstObjectByType<PlayableDirector>();

        if (timelinePaused)
        {
            if (currentTimeline != null) currentTimeline.Play();
            foreach (IPausable p in pausables) p.SetPaused(false);
            timelinePaused = false;
            Debug.Log("[DevMenu] Timeline RESUMED");
        }
        else
        {
            if (currentTimeline != null) currentTimeline.Pause();
            foreach (IPausable p in pausables) p.SetPaused(true);
            timelinePaused = true;
            Debug.Log($"[DevMenu] Timeline PAUSED | Pausables found: {pausables.Count}");
        }

        if (currentTimeline == null)
            Debug.LogWarning("[DevMenu] No PlayableDirector found in scene!");
    }

    void OnGUI()
    {
        if (!devMenuOpen) return;

        // ← slightly taller box to fit new button
        GUI.Box(new Rect(10, 10, 220, 310), "DEV MENU");

        GUI.Label(new Rect(20, 35, 200, 20), "Skip to Level:");

        if (GUI.Button(new Rect(20, 55, 85, 25), "Level 1")) SceneManager.LoadScene("Level_1");
        if (GUI.Button(new Rect(115, 55, 85, 25), "Level 2")) SceneManager.LoadScene("Level_2");
        if (GUI.Button(new Rect(20, 90, 85, 25), "Level 3")) SceneManager.LoadScene("Level_3");
        if (GUI.Button(new Rect(115, 90, 85, 25), "Level 4")) SceneManager.LoadScene("Level_4");
        if (GUI.Button(new Rect(20, 125, 85, 25), "Level 5")) SceneManager.LoadScene("Level_5");
        if (GUI.Button(new Rect(115, 125, 85, 25), "Level 6")) SceneManager.LoadScene("Level_6");
        if (GUI.Button(new Rect(20, 160, 85, 25), "Level 7")) SceneManager.LoadScene("Level_7");
        if (GUI.Button(new Rect(115, 160, 85, 25), "End Scene")) SceneManager.LoadScene("EndScene");

        // ← timeline button with live state label
        string timelineLabel = currentTimeline == null
            ? "No Timeline"
            : timelinePaused ? "▶ Resume Timeline" : "⏸ Pause Timeline";

        if (GUI.Button(new Rect(20, 195, 190, 25), timelineLabel))
            ToggleTimeline();

        if (GUI.Button(new Rect(20, 230, 190, 25), showConsole ? "Hide Console" : "Show Console"))
            showConsole = !showConsole;

        if (GUI.Button(new Rect(20, 265, 190, 25), showStats ? "Hide Stats" : "Show Stats"))
            showStats = !showStats;

        // --- Console Window ---
        if (showConsole)
        {
            GUI.Box(new Rect(240, 10, 500, 300), "Console");
            consoleScroll = GUI.BeginScrollView(
                new Rect(245, 30, 490, 265),
                consoleScroll,
                new Rect(0, 0, 470, logMessages.Count * 18)
            );
            for (int i = 0; i < logMessages.Count; i++)
                GUI.Label(new Rect(0, i * 18, 470, 18), logMessages[i]);
            GUI.EndScrollView();
        }

        // --- Stats Window ---
        if (showStats)
        {
            GUI.Box(new Rect(240, 320, 300, 145), "Stats");
            float fps = 1.0f / deltaTime;
            float totalMemoryMB = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            float reservedMemoryMB = Profiler.GetTotalReservedMemoryLong() / 1048576f;
            GUI.Label(new Rect(250, 345, 280, 22), $"FPS: {Mathf.Ceil(fps)}");
            GUI.Label(new Rect(250, 368, 280, 22), $"Allocated Memory: {totalMemoryMB:F1} MB");
            GUI.Label(new Rect(250, 391, 280, 22), $"Reserved Memory: {reservedMemoryMB:F1} MB");
            GUI.Label(new Rect(250, 414, 280, 22), $"Frame Count: {Time.frameCount}");
            GUI.Label(new Rect(250, 437, 280, 22), $"Time Scale: {Time.timeScale}");
        }
    }
}