using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    [SerializeField]
    Camera mainCam, pauseCam;
    private Scene pauseScene;
    void Start()
    {
        SceneManager.LoadScene("UI_Pause", LoadSceneMode.Additive);
    }
}