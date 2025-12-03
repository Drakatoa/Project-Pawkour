using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AudioThresholdManager : MonoBehaviour
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

    private AudioSource currentClip;
    private Coroutine fadeCoroutine;

    // Auto-create AudioThresholdManager if none exists when the game starts
    // Only creates if we don't have any AudioThresholdManager in the scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureAudioManager()
    {
        if (FindFirstObjectByType<AudioThresholdManager>() == null)
        {
            // Try to find Player GameObject to attach to
            PlayerController player = FindFirstObjectByType<PlayerController>();
            GameObject targetGO = player != null ? player.gameObject : new GameObject("AudioThresholdManager");
            
            var manager = targetGO.GetComponent<AudioThresholdManager>();
            if (manager == null)
            {
                manager = targetGO.AddComponent<AudioThresholdManager>();
            }
            Debug.Log($"[AudioThresholdManager] Auto-created AudioThresholdManager on {targetGO.name}");
        }
    }

    void Start()
    {
        // Auto-find player CharacterController if not assigned
        if (playerController == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
            }
        }

        // Ensure we have AudioSource components (for auto-created instances)
        SetupAudioSources();

        // Stop any ongoing fades
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // FORCE initial state: Only LOW clip should be playing at full volume
        // Set volumes BEFORE playing to avoid any brief audio glitches
        if (lowVelocityClip != null)
        {
            lowVelocityClip.volume = 0f; // Set to 0 first
            if (!lowVelocityClip.isPlaying)
            {
                lowVelocityClip.Play();
            }
            lowVelocityClip.volume = 1f; // Then set to 1
        }
        
        if (mediumVelocityClip != null)
        {
            mediumVelocityClip.volume = 0f;
            if (!mediumVelocityClip.isPlaying)
            {
                mediumVelocityClip.Play();
            }
            mediumVelocityClip.volume = 0f; // Force to 0 again
        }
        
        if (highVelocityClip != null)
        {
            highVelocityClip.volume = 0f;
            if (!highVelocityClip.isPlaying)
            {
                highVelocityClip.Play();
            }
            highVelocityClip.volume = 0f; // Force to 0 again
        }

        currentClip = lowVelocityClip;

        // Debug info to help diagnose audio-not-playing issues
        Debug.Log($"[AudioThresholdManager] INITIAL STATE - low clip={GetClipName(lowVelocityClip)} (vol={lowVelocityClip?.volume:F2}), medium clip={GetClipName(mediumVelocityClip)} (vol={mediumVelocityClip?.volume:F2}), high clip={GetClipName(highVelocityClip)} (vol={highVelocityClip?.volume:F2}), playerController={playerController}");
    }

    void SetupAudioSources()
    {
        // Create AudioSource components if they don't exist
        var sources = GetComponents<AudioSource>();
        if (sources.Length < 3)
        {
            for (int i = sources.Length; i < 3; i++)
            {
                gameObject.AddComponent<AudioSource>();
            }
            sources = GetComponents<AudioSource>();
        }

        if (lowVelocityClip == null) lowVelocityClip = sources[0];
        if (mediumVelocityClip == null) mediumVelocityClip = sources[1];
        if (highVelocityClip == null) highVelocityClip = sources[2];

        // Configure AudioSource settings
        if (lowVelocityClip != null)
        {
            lowVelocityClip.playOnAwake = false;
            lowVelocityClip.loop = true;
            lowVelocityClip.spatialBlend = 0f;
        }
        if (mediumVelocityClip != null)
        {
            mediumVelocityClip.playOnAwake = false;
            mediumVelocityClip.loop = true;
            mediumVelocityClip.spatialBlend = 0f;
        }
        if (highVelocityClip != null)
        {
            highVelocityClip.playOnAwake = false;
            highVelocityClip.loop = true;
            highVelocityClip.spatialBlend = 0f;
        }

#if UNITY_EDITOR
        // In editor, try to auto-assign clips using AssetDatabase
        if (!Application.isPlaying)
        {
            AssignPawrkourSFX();
        }
#endif
    }

    void Update()
    {
        if (playerController == null)
        {
            Debug.LogWarning("[AudioThresholdManager] playerController is null - cannot read velocity!");
            return;
        }

        // Use horizontal velocity only (ignore Y component for vertical movement/gravity)
        Vector3 velocity3D = playerController.velocity;
        Vector2 horizontalVelocity = new Vector2(velocity3D.x, velocity3D.z);
        float velocity = horizontalVelocity.magnitude;

        if (debugVelocity)
        {
            Debug.Log($"[AudioThresholdManager] Horizontal velocity: {velocity:F2} (low<{mediumThreshold} <medium<{highThreshold} <high)");
        }

        // Clamp very small velocities to 0 to ensure low clip plays when stationary
        if (velocity < 0.1f)
        {
            velocity = 0f;
        }

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
            if (debugVelocity)
            {
                Debug.Log($"[AudioThresholdManager] Velocity changed: {velocity:F2} -> Switching from {GetClipName(currentClip)} to {GetClipName(targetClip)}");
            }
            SwitchToClip(targetClip);
        }
        
        // Safety check: If no fade is running, ensure volumes are correct
        // This fixes cases where fade was interrupted or didn't complete
        if (fadeCoroutine == null && currentClip != null)
        {
            // Force volumes to be correct based on currentClip
            float expectedLowVol = (currentClip == lowVelocityClip) ? 1f : 0f;
            float expectedMediumVol = (currentClip == mediumVelocityClip) ? 1f : 0f;
            float expectedHighVol = (currentClip == highVelocityClip) ? 1f : 0f;
            
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
                
                if (debugVelocity)
                {
                    Debug.Log($"[AudioThresholdManager] Fixed volumes: Low={expectedLowVol}, Medium={expectedMediumVol}, High={expectedHighVol}");
                }
            }
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
        float targetLow = (targetClip == lowVelocityClip) ? 1f : 0f;
        float targetMedium = (targetClip == mediumVelocityClip) ? 1f : 0f;
        float targetHigh = (targetClip == highVelocityClip) ? 1f : 0f;

        if (debugVelocity)
        {
            Debug.Log($"[AudioThresholdManager] Starting fade: Low {startLow:F2}->{targetLow:F2}, Medium {startMedium:F2}->{targetMedium:F2}, High {startHigh:F2}->{targetHigh:F2}");
        }

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

        if (debugVelocity)
        {
            Debug.Log($"[AudioThresholdManager] Fade complete: Low={lowVelocityClip?.volume:F2}, Medium={mediumVelocityClip?.volume:F2}, High={highVelocityClip?.volume:F2}");
        }

        fadeCoroutine = null;
    }

    private string GetClipName(AudioSource src)
    {
        if (src == null) return "(AudioSource null)";
        if (src.clip == null) return "(clip null)";
        return src.clip.name;
    }

#if UNITY_EDITOR
    // Automatically try to assign the three Pawrkour audio files (in increasing order)
    // to the three AudioSources when the component is validated in the editor.
    void OnValidate()
    {
        AssignPawrkourSFX();
    }

    // Public so a menu command or other editor code can call it.
    public void AssignPawrkourSFX()
    {
        // Ensure there are three AudioSource components on this GameObject and assign them
        var existing = this.GetComponents<AudioSource>();
        // If there are fewer than 3, add more
        for (int i = existing.Length; i < 3; i++)
        {
            existing = this.GetComponents<AudioSource>();
            this.gameObject.AddComponent<AudioSource>();
        }
        existing = this.GetComponents<AudioSource>();

        if (lowVelocityClip == null) lowVelocityClip = existing.Length > 0 ? existing[0] : this.gameObject.AddComponent<AudioSource>();
        if (mediumVelocityClip == null) mediumVelocityClip = existing.Length > 1 ? existing[1] : this.gameObject.AddComponent<AudioSource>();
        if (highVelocityClip == null) highVelocityClip = existing.Length > 2 ? existing[2] : this.gameObject.AddComponent<AudioSource>();

        // Auto-find player CharacterController if not assigned
        if (playerController == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
            }
        }

        // Try both .wav and .m4a extensions
        string[] extensions = new string[] { ".wav", ".m4a" };
        string[] baseNames = new string[] { "Pawrkour", "Pawrkour 2", "Pawrkour 3" };
        AudioClip[] found = new AudioClip[3];

        // Search for ALL audio clips in Resources/SFX first
        string[] allGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/SFX" });
        AudioClip[] allClips = new AudioClip[allGuids.Length];
        for (int g = 0; g < allGuids.Length; g++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuids[g]);
            allClips[g] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        // Now match each baseName to the correct clip by EXACT name match
        for (int i = 0; i < baseNames.Length; i++)
        {
            // FIRST: Try exact filename match in Resources/SFX
            foreach (string ext in extensions)
            {
                string fileName = baseNames[i] + ext;
                string resourcePath = $"Assets/Resources/SFX/{fileName}";
                if (System.IO.File.Exists(resourcePath))
                {
                    found[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(resourcePath);
                    if (found[i] != null && found[i].name == baseNames[i])
                    {
                        Debug.Log($"[AudioThresholdManager] Found clip #{i+1} ({baseNames[i]}) by exact file path: {resourcePath}");
                        break;
                    }
                    found[i] = null; // Reset if name doesn't match
                }
            }
            
            // SECOND: Search through all clips in Resources/SFX for exact name match
            if (found[i] == null)
            {
                foreach (AudioClip clip in allClips)
                {
                    if (clip != null && clip.name == baseNames[i])
                    {
                        found[i] = clip;
                        string path = AssetDatabase.GetAssetPath(clip);
                        Debug.Log($"[AudioThresholdManager] Found clip #{i+1} ({baseNames[i]}) by exact name match: {path}");
                        break;
                    }
                }
            }
            
            if (found[i] == null)
            {
                Debug.LogError($"[AudioThresholdManager] Could not find audio clip with exact name '{baseNames[i]}' in Resources/SFX");
            }
        }

        // Assign clips in order: [0]=low, [1]=medium, [2]=high
        if (found[0] != null && lowVelocityClip != null)
        {
            lowVelocityClip.clip = found[0];
            Debug.Log($"[AudioThresholdManager] Assigned LOW clip: '{found[0].name}'");
        }
        else
        {
            Debug.LogError($"[AudioThresholdManager] FAILED to assign LOW clip! Expected 'Pawrkour' but found[0] is null");
        }

        if (found[1] != null && mediumVelocityClip != null)
        {
            mediumVelocityClip.clip = found[1];
            Debug.Log($"[AudioThresholdManager] Assigned MEDIUM clip: '{found[1].name}'");
        }
        else
        {
            Debug.LogError($"[AudioThresholdManager] FAILED to assign MEDIUM clip! Expected 'Pawrkour 2' but found[1] is null");
        }

        if (found[2] != null && highVelocityClip != null)
        {
            highVelocityClip.clip = found[2];
            Debug.Log($"[AudioThresholdManager] Assigned HIGH clip: '{found[2].name}'");
        }
        else
        {
            Debug.LogError($"[AudioThresholdManager] FAILED to assign HIGH clip! Expected 'Pawrkour 3' but found[2] is null");
        }

        // Ensure PlayOnAwake is off so Start's Play calls are deterministic
        if (lowVelocityClip != null) { lowVelocityClip.playOnAwake = false; lowVelocityClip.loop = true; lowVelocityClip.spatialBlend = 0f; }
        if (mediumVelocityClip != null) { mediumVelocityClip.playOnAwake = false; mediumVelocityClip.loop = true; mediumVelocityClip.spatialBlend = 0f; }
        if (highVelocityClip != null) { highVelocityClip.playOnAwake = false; highVelocityClip.loop = true; highVelocityClip.spatialBlend = 0f; }

        // Mark scene dirty so user sees changes
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            if (this.gameObject != null) EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
        }
    }

    [MenuItem("Tools/Assign Pawrkour SFX To Selected AudioThresholdManager")]
    private static void AssignSFXToSelection()
    {
        foreach (var go in Selection.gameObjects)
        {
            var comp = go.GetComponent<AudioThresholdManager>();
            if (comp != null) comp.AssignPawrkourSFX();
        }
    }
#endif
}