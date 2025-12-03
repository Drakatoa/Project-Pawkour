using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject levelSelectPanel;
    public GameObject optionsPanel;
    public GameObject darkOverlay;
    
    void Start()
    {
        // Make sure we start with main menu visible
        ShowMainMenu();
    }
    
    public void ShowMainMenu()
    {
        // Hide all panels and overlay
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
        
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        
        if (darkOverlay != null)
            darkOverlay.SetActive(false);
    }
    
    public void ShowLevelSelect()
    {
        // Show level select panel and dark overlay
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
        
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        
        // if (darkOverlay != null)
        //     darkOverlay.SetActive(true);
    }
    
    public void ShowOptions()
    {
        // Show options panel and dark overlay
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
        
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
        
        if (darkOverlay != null)
            darkOverlay.SetActive(true);
    }
    
    // Called by Start Run button - loads Level1
    public void StartRun()
    {
        SceneManager.LoadScene("Cutscenes_Intro");
    }
    
    // Called by level buttons - pass in level name
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
    
    // Called by Quit button
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}