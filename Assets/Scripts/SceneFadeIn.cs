using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 1.0f; // Time in seconds to fade in
    
    private Image fadeImage;

    void Start()
    {
        fadeImage = GetComponent<Image>();
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
            fadeImage.color = new Color(0,0,0,1 - (elapsed / fadeDuration));
            yield return null;
        }

        // Ensure it is fully invisible
        fadeImage.color = new Color(0,0,0,0);
        
        // Optional: Disable the object entirely to save performance
        gameObject.SetActive(false); 
    }

    public void Fade()
    {
        StartCoroutine(FadeToWhite());
    }

    private IEnumerator FadeToWhite()
    {
        float elapsed = 0f;
        fadeImage.color = new Color(1,1,1,0);
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
            fadeImage.color = new Color(1,1,1,elapsed / fadeDuration);
            yield return null;
        }

        // Ensure it is fully invisible
        fadeImage.color = new Color(1,1,1,1);
        
    }
}