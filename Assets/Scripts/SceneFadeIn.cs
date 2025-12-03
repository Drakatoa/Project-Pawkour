using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 1.0f; // Time in seconds to fade in
    
    private CanvasGroup fadeGroup;

    void Awake()
    {
        // Get the CanvasGroup component
        fadeGroup = GetComponent<CanvasGroup>();
        
        // If you forgot to add it manually, add it via code
        if (fadeGroup == null)
            fadeGroup = gameObject.AddComponent<CanvasGroup>();

        // IMPORTANT: Force the screen to be black instantly before the first frame renders
        fadeGroup.alpha = 1; 
        fadeGroup.blocksRaycasts = true; // Block clicks while screen is black
    }

    void Start()
    {
        // Start the fade automatically
        StartCoroutine(FadeFromBlack());
    }

    IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        while(elapsed < 1)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // Calculate transparency (Starts at 1, goes to 0)
            fadeGroup.alpha = 1 - (elapsed / fadeDuration);
            yield return null;
        }

        // Ensure it is fully invisible
        fadeGroup.alpha = 0;
        
        // Disable interaction so it doesn't block buttons in the game
        fadeGroup.blocksRaycasts = false;
        
        // Optional: Disable the object entirely to save performance
        gameObject.SetActive(false); 
    }
}