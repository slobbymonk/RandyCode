using UnityEngine;
using System.Collections;

public class LauncherDistortion : MonoBehaviour
{
    [SerializeField] private Transform[] barrelBones;  // Bones of the barrel (from base to tip)
    [SerializeField] private float maxDistortion = 1.5f;  // Maximum scale factor for distortion
    [SerializeField] private float distortionSpeed = 5f;  // Speed of the distortion wave
    [SerializeField] private float returnSpeed = 2f;  // Speed of returning to normal size
    [SerializeField] private float overlapDuration = 0.1f;  // How much time the transitions between bones overlap

    private bool[] isDistortingBone;  // Track which bones are distorting

    private void Awake()
    {
        // Initialize the isDistortingBone array to track the state of each bone
        isDistortingBone = new bool[barrelBones.Length];
    }

    public void PlayBarrelEffect()
    {
        StartCoroutine(DistortBonesSmooth());
    }

    public void PlayBarrelEffectReverse()
    {
        StartCoroutine(DistortBonesReverseSmooth());
    }

    IEnumerator DistortBonesSmooth()
    {
        for (int i = 0; i < barrelBones.Length; i++)
        {
            // Check if the bone is already distorting, skip if it is
            if (!isDistortingBone[i])
            {
                StartCoroutine(DistortBone(barrelBones[i], i));
            }

            // Overlap the distortion by starting the next bone while the current one is scaling down
            if (i < barrelBones.Length - 1)
            {
                yield return new WaitForSeconds(overlapDuration);  // Wait briefly before starting the next bone
            }
        }
    }

    IEnumerator DistortBonesReverseSmooth()
    {
        for (int i = barrelBones.Length - 1; i >= 0; i--)
        {
            // Check if the bone is already distorting, skip if it is
            if (!isDistortingBone[i])
            {
                StartCoroutine(DistortBone(barrelBones[i], i));
            }

            // Overlap the distortion by starting the next bone while the current one is scaling down
            if (i > 0)
            {
                yield return new WaitForSeconds(overlapDuration);  // Wait briefly before starting the next bone
            }
        }
    }

    IEnumerator DistortBone(Transform bone, int index)
    {
        // Mark this bone as distorting
        isDistortingBone[index] = true;

        Vector3 originalScale = bone.localScale;
        Vector3 targetScale = new Vector3(originalScale.x * maxDistortion, originalScale.y / maxDistortion, originalScale.z * maxDistortion);

        // Scale the bone up smoothly
        while (Vector3.Distance(bone.localScale, targetScale) > 0.01f)
        {
            bone.localScale = Vector3.Lerp(bone.localScale, targetScale, Time.deltaTime * distortionSpeed);
            yield return null;
        }

        // Start scaling it back down immediately after reaching max distortion
        while (Vector3.Distance(bone.localScale, originalScale) > 0.01f)
        {
            bone.localScale = Vector3.Lerp(bone.localScale, originalScale, Time.deltaTime * returnSpeed);
            yield return null;
        }

        // Ensure the bone returns to its exact original scale
        bone.localScale = originalScale;

        // Mark this bone as done distorting
        isDistortingBone[index] = false;
    }
}
