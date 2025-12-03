using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    void Start()
    {
        LoadPauseUI();
    }
    
    void LoadPauseUI()
    {
        // Check if UI_Pause is already loaded
        Scene pauseScene = SceneManager.GetSceneByName("UI_Pause");
        
        if (!pauseScene.isLoaded)
        {
            // Load UI_Pause on top of this level
            SceneManager.LoadScene("UI_Pause", LoadSceneMode.Additive);
        }
    }
}