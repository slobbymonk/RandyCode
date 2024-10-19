using System.Collections;
using UnityEngine;

public class BananaSlipping : MonoBehaviour
{
    [SerializeField] private float _stoppingTime = 2f;
    [SerializeField] private float _shrinkDuration = 1f; // Duration of the shrinking animation
    [SerializeField] private float _spinSpeed = 360f;    // Speed of spinning (degrees per second)
    private float stoppedTime;

    private CarController carController;
    private bool isStopped;

    [SerializeField] private AudioSource _audioSource;

    private void Update()
    {
        if (stoppedTime <= 0 && isStopped)
        {
            HandlePlayer(true);
            // Destroy the banana after re-enabling the car
            Destroy(gameObject);
        }
        else
        {
            stoppedTime -= Time.deltaTime;
        }
    }

    void HandlePlayer(bool state)
    {
        carController.enabled = state;
        isStopped = !state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            carController = other.gameObject.GetComponent<CarController>();
            HandlePlayer(false);
            stoppedTime = _stoppingTime;

            if(_audioSource != null) _audioSource.Play();

            // Disable the colliders to prevent further interaction
            DisableColliders();

            // Start the shrinking and spinning animation
            StartCoroutine(ShrinkAndSpin());
        }
    }

    private void DisableColliders()
    {
        // Disable all colliders attached to the banana
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private IEnumerator ShrinkAndSpin()
    {
        float elapsedTime = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsedTime < _shrinkDuration)
        {
            // Calculate the proportion of time passed
            float t = elapsedTime / _shrinkDuration;

            // Shrink the banana's scale over time
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            // Spin the banana around its Y axis
            transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait until the next frame
            yield return null;
        }

        // Ensure the banana is completely shrunk
        transform.localScale = Vector3.zero;

        // The object will be destroyed after the car is re-enabled in Update()
    }
}
