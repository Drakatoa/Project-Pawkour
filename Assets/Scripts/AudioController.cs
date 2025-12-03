using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AudioController : MonoBehaviour
{
    public AudioSource lowVelocityClip;
    public AudioSource mediumVelocityClip;
    public AudioSource highVelocityClip;
    
    [Tooltip("Assign the player's CharacterController to read velocity from.")]
    public CharacterController playerController;

    public float mediumThreshold = 5f;
    public float highThreshold = 10f;
    public float fadeDuration = 1f; // seconds
    public bool debugVelocity = false; // Enable to see velocity values in console

    public float maxVol = 0.25f;

    private AudioSource currentClip;
    private Coroutine fadeCoroutine;
    void Start()
    {
        lowVelocityClip.Play();
        mediumVelocityClip.Play();
        highVelocityClip.Play();
        lowVelocityClip.volume = 1f;
        mediumVelocityClip.volume = 0f;
        highVelocityClip.volume = 0f; // Force to 0 again
        

        currentClip = lowVelocityClip;
        // Debug.Log($"[AudioThresholdManager] INITIAL STATE - low clip={GetClipName(lowVelocityClip)} (vol={lowVelocityClip?.volume:F2}), medium clip={GetClipName(mediumVelocityClip)} (vol={mediumVelocityClip?.volume:F2}), high clip={GetClipName(highVelocityClip)} (vol={highVelocityClip?.volume:F2}), playerController={playerController}");
    }

    void Update()
    {
        // Use horizontal velocity only (ignore Y component for vertical movement/gravity)
        Vector3 velocity3D = playerController.velocity;
        Vector2 horizontalVelocity = new Vector2(velocity3D.x, velocity3D.z);
        float velocity = horizontalVelocity.magnitude;

        // if (debugVelocity)
        // {
        //     Debug.Log($"[AudioThresholdManager] Horizontal velocity: {velocity:F2} (low<{mediumThreshold} <medium<{highThreshold} <high)");
        // }

        // Determine which clip should be playing
        AudioSource targetClip;
        if (velocity < mediumThreshold)
        {
            targetClip = lowVelocityClip;
        }
        else if (velocity < highThreshold)
        {
            targetClip = mediumVelocityClip;
        }
        else
        {
            targetClip = highVelocityClip;
        }

        // Switch if needed
        if (targetClip != currentClip && targetClip != null)
        {
            // if (debugVelocity)
            // {
            //     Debug.Log($"[AudioThresholdManager] Velocity changed: {velocity:F2} -> Switching from {GetClipName(currentClip)} to {GetClipName(targetClip)}");
            // }
            SwitchToClip(targetClip);
        }
        
        // Safety check: If no fade is running, ensure volumes are correct
        // This fixes cases where fade was interrupted or didn't complete
        if (fadeCoroutine == null && currentClip != null)
        {
            // Force volumes to be correct based on currentClip
            float expectedLowVol = (currentClip == lowVelocityClip) ? maxVol : 0f;
            float expectedMediumVol = (currentClip == mediumVelocityClip) ? maxVol : 0f;
            float expectedHighVol = (currentClip == highVelocityClip) ? maxVol : 0f;
            
            // Check if volumes are wrong (allow small tolerance for floating point)
            bool needsFix = false;
            if (lowVelocityClip != null && Mathf.Abs(lowVelocityClip.volume - expectedLowVol) > 0.01f) needsFix = true;
            if (mediumVelocityClip != null && Mathf.Abs(mediumVelocityClip.volume - expectedMediumVol) > 0.01f) needsFix = true;
            if (highVelocityClip != null && Mathf.Abs(highVelocityClip.volume - expectedHighVol) > 0.01f) needsFix = true;
            
            // Force correct volumes immediately if they're wrong
            if (needsFix)
            {
                if (lowVelocityClip != null) lowVelocityClip.volume = expectedLowVol;
                if (mediumVelocityClip != null) mediumVelocityClip.volume = expectedMediumVol;
                if (highVelocityClip != null) highVelocityClip.volume = expectedHighVol;
                
                // if (debugVelocity)
                // {
                //     Debug.Log($"[AudioThresholdManager] Fixed volumes: Low={expectedLowVol}, Medium={expectedMediumVol}, High={expectedHighVol}");
                // }
            }
        }
    }

    public void SwitchToClip(int clip)
    {
        switch(clip)
        {
            case 0:
                SwitchToClip(lowVelocityClip);
                break;
            case 1:
                SwitchToClip(mediumVelocityClip);
                break;
            case 2:
                SwitchToClip(highVelocityClip);
                break;
            default:
                break;
        }
    }

    void SwitchToClip(AudioSource newClip)
    {
        if (newClip == currentClip || newClip == null) return;
        
        // Stop any ongoing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Immediately update currentClip so Update() doesn't keep trying to switch
        currentClip = newClip;

        // Start new fade
        fadeCoroutine = StartCoroutine(FadeClips(newClip));
    }

    IEnumerator FadeClips(AudioSource targetClip)
    {
        // Capture current volumes RIGHT NOW (in case a previous fade was interrupted)
        float startLow = lowVelocityClip != null ? lowVelocityClip.volume : 0f;
        float startMedium = mediumVelocityClip != null ? mediumVelocityClip.volume : 0f;
        float startHigh = highVelocityClip != null ? highVelocityClip.volume : 0f;

        // Determine target volumes - only target clip gets 1.0, all others get 0.0
        float targetLow = (targetClip == lowVelocityClip) ? maxVol : 0f;
        float targetMedium = (targetClip == mediumVelocityClip) ? maxVol : 0f;
        float targetHigh = (targetClip == highVelocityClip) ? maxVol : 0f;

        // if (debugVelocity)
        // {
        //     Debug.Log($"[AudioThresholdManager] Starting fade: Low {startLow:F2}->{targetLow:F2}, Medium {startMedium:F2}->{targetMedium:F2}, High {startHigh:F2}->{targetHigh:F2}");
        // }

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            // Fade all three clips simultaneously
            if (lowVelocityClip != null)
            {
                lowVelocityClip.volume = Mathf.Lerp(startLow, targetLow, t);
            }
            if (mediumVelocityClip != null)
            {
                mediumVelocityClip.volume = Mathf.Lerp(startMedium, targetMedium, t);
            }
            if (highVelocityClip != null)
            {
                highVelocityClip.volume = Mathf.Lerp(startHigh, targetHigh, t);
            }

            yield return null;
        }

        // CRITICAL: Ensure final volumes are EXACTLY as intended (no floating point errors)
        if (lowVelocityClip != null) lowVelocityClip.volume = targetLow;
        if (mediumVelocityClip != null) mediumVelocityClip.volume = targetMedium;
        if (highVelocityClip != null) highVelocityClip.volume = targetHigh;

        // if (debugVelocity)
        // {
        //     Debug.Log($"[AudioThresholdManager] Fade complete: Low={lowVelocityClip?.volume:F2}, Medium={mediumVelocityClip?.volume:F2}, High={highVelocityClip?.volume:F2}");
        // }

        fadeCoroutine = null;
    }
}