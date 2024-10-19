using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    private AudioSource audioSource; // Reference to the AudioSource component
    private bool hasStartedPlaying = false; // Flag to check if the audio has started

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found on this GameObject.");
            enabled = false; // Disable the script if AudioSource is not found
        }
    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            hasStartedPlaying = true; // Audio has started playing
        }

        if (hasStartedPlaying && !audioSource.isPlaying)
        {
            // Audio has stopped playing
            Destroy(gameObject); // Destroy this GameObject
        }
    }
}
