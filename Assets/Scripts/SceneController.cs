using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Methode um zu einer bestimmten Scene zu wechseln
    public void LoadScene(string sceneName)
    {
        Debug.Log("Lade Scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
    
    // Methode um zum nächsten Level zu gehen
    public void LoadNextLevel(int currentLevel)
    {
        string nextLevelName = "Level" + (currentLevel + 1);
        Debug.Log("Lade nächstes Level: " + nextLevelName);
        SceneManager.LoadScene(nextLevelName);
    }
    
    // Methode um zum Main Menu zurück zu gehen
    public void LoadMainMenu()
    {
        Debug.Log("Zurück zum Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
    
    // Methode um Spiel zu beenden
    public void QuitGame()
    {
        Debug.Log("Spiel beenden");
        Application.Quit();
        
        // Für Testing im Editor:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}