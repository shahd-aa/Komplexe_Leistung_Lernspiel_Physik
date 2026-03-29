using UnityEngine;
using UnityEngine.UI;

public class UI_QUIT : MonoBehaviour
{
    private static UI_QUIT instance;
    public GameObject confirmPanel;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        confirmPanel.SetActive(false); // hidden on start
    }

    public void QuitGame()
    {
        confirmPanel.SetActive(true); // show the "are you sure?"
    }

    public void OnYesClicked()
    {
        Application.Quit();
        Debug.Log("applicated quit thru script: UI_QUIT");
    }

    public void OnNoClicked()
    {
        confirmPanel.SetActive(false); // just hide it again
    }
}