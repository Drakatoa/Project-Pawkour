using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // REQUIRED: Added this namespace
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Panels")]
    public GameObject cutscene1;
    public GameObject cutscene2;
    public GameObject cutscene3;
    
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;  // How long fade takes in seconds
    
    [Header("Next Scene")]
    public string nextSceneName = "Tutorial";
    
    private int currentCutscene = 1;
    private bool isTransitioning = false;  // Prevent spam clicking
    
    void Start()
    {
        // Start with cutscene 1 visible
        ShowCutsceneImmediate(1);
    }
    
    void Update()
    {
        if (isTransitioning) return;

        // NEW INPUT SYSTEM CHECK
        // We check if the Keyboard exists AND a key was pressed, OR if the Mouse exists AND left click was pressed
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (keyPressed || mouseClicked)
        {
            NextCutscene();
        }
    }
    
    void NextCutscene()
    {
        currentCutscene++;
        
        if (currentCutscene == 2)
        {
            StartCoroutine(FadeToNextCutscene(2));
        }
        else if (currentCutscene == 3)
        {
            StartCoroutine(FadeToNextCutscene(3));
        }
        else if (currentCutscene > 3)
        {
            // All cutscenes done, fade out and load next scene
            StartCoroutine(FadeToNextScene());
        }
    }
    
    // Coroutine for fading between cutscenes
    IEnumerator FadeToNextCutscene(int nextCutsceneNumber)
    {
        isTransitioning = true;
        
        // Get the current and next cutscene objects
        GameObject currentObj = GetCutsceneObject(currentCutscene - 1);
        GameObject nextObj = GetCutsceneObject(nextCutsceneNumber);
        
        if (currentObj != null && nextObj != null)
        {
            // Make sure next cutscene is active but transparent
            nextObj.SetActive(true);
            CanvasGroup nextGroup = nextObj.GetComponent<CanvasGroup>();
            if (nextGroup != null)
                nextGroup.alpha = 0;
            
            // Get canvas groups
            CanvasGroup currentGroup = currentObj.GetComponent<CanvasGroup>();
            
            // Fade out current and fade in next simultaneously
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                
                // Fade out current
                if (currentGroup != null)
                    currentGroup.alpha = 1 - t;
                
                // Fade in next
                if (nextGroup != null)
                    nextGroup.alpha = t;
                
                yield return null;
            }
            
            // Ensure final values
            if (currentGroup != null)
                currentGroup.alpha = 0;
            if (nextGroup != null)
                nextGroup.alpha = 1;
            
            // Deactivate old cutscene
            currentObj.SetActive(false);
        }
        
        isTransitioning = false;
    }
    
    // Coroutine for fading to next scene
    IEnumerator FadeToNextScene()
    {
        isTransitioning = true;
        
        // Fade out current cutscene
        GameObject currentObj = GetCutsceneObject(currentCutscene - 1);
        if (currentObj != null)
        {
            CanvasGroup currentGroup = currentObj.GetComponent<CanvasGroup>();
            
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                
                if (currentGroup != null)
                    currentGroup.alpha = 1 - t;
                
                yield return null;
            }
        }
        
        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }
    
    GameObject GetCutsceneObject(int cutsceneNumber)
    {
        if (cutsceneNumber == 1)
            return cutscene1;
        else if (cutsceneNumber == 2)
            return cutscene2;
        else if (cutsceneNumber == 3)
            return cutscene3;
        else
            return null;
    }
    
    // Show cutscene immediately without fade (used at start)
    void ShowCutsceneImmediate(int cutsceneNumber)
    {
        // Hide all
        if (cutscene1 != null)
        {
            cutscene1.SetActive(false);
            CanvasGroup cg1 = cutscene1.GetComponent<CanvasGroup>();
            if (cg1 != null) cg1.alpha = 0;
        }
        
        if (cutscene2 != null)
        {
            cutscene2.SetActive(false);
            CanvasGroup cg2 = cutscene2.GetComponent<CanvasGroup>();
            if (cg2 != null) cg2.alpha = 0;
        }
        
        if (cutscene3 != null)
        {
            cutscene3.SetActive(false);
            CanvasGroup cg3 = cutscene3.GetComponent<CanvasGroup>();
            if (cg3 != null) cg3.alpha = 0;
        }
        
        // Show the requested one
        GameObject targetObj = GetCutsceneObject(cutsceneNumber);
        if (targetObj != null)
        {
            targetObj.SetActive(true);
            CanvasGroup targetGroup = targetObj.GetComponent<CanvasGroup>();
            if (targetGroup != null)
                targetGroup.alpha = 1;
        }
    }
}