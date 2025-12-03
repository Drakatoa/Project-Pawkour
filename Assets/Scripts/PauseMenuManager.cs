using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;
    public GameObject darkOverlay;
    
    private bool isPaused = false;
    
    void Update()
    {
        // Check if player presses ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
    
    public void Pause()
    {
        // Show pause menu
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        if (darkOverlay != null)
            darkOverlay.SetActive(true);
        
        Time.timeScale = 0f;  // Freeze game
        isPaused = true;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Resume()
    {
        // Hide pause menu
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (darkOverlay != null)
            darkOverlay.SetActive(false);
        
        Time.timeScale = 1f;  // Unfreeze game
        isPaused = false;
        
        // Hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void OpenOptions()
    {
        // For now, just resume - we'll add options later if needed
        Resume();
        Debug.Log("Options button pressed - functionality coming soon");
    }
    
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;  // IMPORTANT: Reset time
        SceneManager.LoadScene("MainMenu");
    }
}