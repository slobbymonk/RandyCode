using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flattener : MonoBehaviour
{
    [SerializeField] private FlySwatter _flySwatter;

    [SerializeField] private LayerMask flattenableLayer;           // Layer of objects that can be flattened
    [SerializeField] private float flattenThickness = 0.1f;        // Desired thickness after flattening
    [SerializeField] private float flattenDuration = 0.2f;         // Time it takes to flatten the object

    [SerializeField] private GameObject _smackEffect;
    [SerializeField] private GameObject _impactEffect;

    private void OnCollisionEnter(Collision other)
    {
        if (_flySwatter.GetSwattingState())
        {
            if (flattenableLayer == (flattenableLayer | (1 << other.gameObject.layer)))
            {
                StartCoroutine(Flatten(other.transform));
                Instantiate(_smackEffect, other.contacts[0].point, Quaternion.identity);
                Instantiate(_impactEffect, other.contacts[0].point, Quaternion.identity);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_flySwatter.GetSwattingState())
        {
            if (flattenableLayer == (flattenableLayer | (1 << other.gameObject.layer)))
            {
                StartCoroutine(Flatten(other.transform));
                Instantiate(_smackEffect, other.ClosestPoint(other.transform.position), Quaternion.identity);
                Instantiate(_impactEffect, other.ClosestPoint(other.transform.position), Quaternion.identity);
            }
        }
    }
    private IEnumerator Flatten(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 flattenedScale = new Vector3(originalScale.x, flattenThickness, originalScale.z); // Flatten y-axis

        float timeElapsed = 0f;

        // Smoothly flatten the object over time
        while (timeElapsed < flattenDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / flattenDuration;
            target.localScale = Vector3.Lerp(originalScale, flattenedScale, t);

            yield return null;
        }

        // Ensure the final scale is exactly the flattened scale
        target.localScale = flattenedScale;
    }
}
