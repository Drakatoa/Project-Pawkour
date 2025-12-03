using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Using New Input System

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;    // The buttons (Resume, Options, Exit)
    public GameObject optionsPanel;  // The new Prefab you added
    public GameObject darkOverlay;
    
    private bool isPaused = false;
    
    void Update()
    {
        // Toggle Pause with Escape key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                // If Options are open, ESC should go back to Pause Menu
                CloseOptions(); 
            }
            else if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (darkOverlay != null) darkOverlay.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false); // Ensure options are closed
        
        Time.timeScale = 0f;
        isPaused = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        Time.timeScale = 1f;
        isPaused = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void OpenOptions()
    {
        // Hide the main pause buttons, show the sliders
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        // Hide the sliders, show the main pause buttons
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }
    
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}