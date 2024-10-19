using System.Collections.Generic;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float _cutoutSize = .1f;
    [SerializeField] private float _cutoutFalloff = 0.05f;

    private Camera mainCamera;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Renderer, Material[]> cutoutMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);

        // Save and apply cutout materials
        ApplyCutoutMaterials(hitObjects, cutoutPos);

        // Check if the camera is inside an object and apply cutout
        CheckCameraInsideObject();
    }

    private void ApplyCutoutMaterials(RaycastHit[] hitObjects, Vector2 cutoutPos)
    {
        // Clear previous cutout materials
        foreach (var kvp in cutoutMaterials)
        {
            Renderer renderer = kvp.Key;
            if (renderer != null)
            {
                renderer.materials = originalMaterials[renderer];
            }
        }
        cutoutMaterials.Clear();

        // Apply cutout settings
        foreach (var hit in hitObjects)
        {
            Renderer renderer = hit.transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Save original materials if not already saved
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.materials;
                }

                // Create a copy of the materials to modify
                Material[] materials = renderer.materials;
                Material[] instanceMaterials = new Material[materials.Length];
                for (int i = 0; i < materials.Length; ++i)
                {
                    instanceMaterials[i] = new Material(materials[i]); // Create a new instance
                    instanceMaterials[i].SetVector("_CutoutPosition", cutoutPos);
                    instanceMaterials[i].SetFloat("_CutoutSize", _cutoutSize);
                    instanceMaterials[i].SetFloat("_FalloffSize", _cutoutFalloff);
                }

                renderer.materials = instanceMaterials;
                cutoutMaterials[renderer] = instanceMaterials;
            }
        }
    }

    private void CheckCameraInsideObject()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f, wallMask);
        foreach (var collider in colliders)
        {
            Renderer renderer = collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.materials;
                }

                // Create a copy of the materials to modify
                Material[] materials = renderer.materials;
                Material[] instanceMaterials = new Material[materials.Length];
                for (int i = 0; i < materials.Length; ++i)
                {
                    instanceMaterials[i] = new Material(materials[i]); // Create a new instance
                    instanceMaterials[i].SetVector("_CutoutPosition", new Vector2(0.5f, 0.5f)); // Center of the screen
                    instanceMaterials[i].SetFloat("_CutoutSize", _cutoutSize);
                    instanceMaterials[i].SetFloat("_FalloffSize", _cutoutFalloff);
                }

                renderer.materials = instanceMaterials;
                cutoutMaterials[renderer] = instanceMaterials;
            }
        }
    }
}
