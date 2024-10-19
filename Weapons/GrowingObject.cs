using System.Collections;
using UnityEngine;

public class GrowingObject : MonoBehaviour
{
    [SerializeField] private float growingTime = 1f; // Time to grow to full size
    private Vector3 finalSize;
    private Vector3 originalSize;

    private void Awake()
    {
        finalSize = transform.localScale; // Store the final size
        originalSize = Vector3.zero; // Start from zero scale
        transform.localScale = originalSize; // Set initial scale to zero
        StartCoroutine(Grow()); // Start growing coroutine
    }

    private IEnumerator Grow()
    {
        float elapsedTime = 0f;

        // Scale from zero to the final size
        while (elapsedTime < growingTime)
        {
            transform.localScale = Vector3.Lerp(originalSize, finalSize, elapsedTime / growingTime);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure it reaches the final size
        transform.localScale = finalSize;
    }
}
