using System.Collections;
using UnityEngine;

public class Vacuum : Weapon
{
    public Transform suctionPosition; // Position where suction starts
    public float suctionRange = 5f;
    public float launchForce = 500f;
    public float pullStrength = 10f; // Strength of the pulling effect
    public Rigidbody suckedObject;
    private bool isVacuumOn = false;
    private bool hasObjectSucked = false; // To prevent multiple objects from being sucked in

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public Vector2 minMaxRotationX = new Vector2(-30f, 30f);

    public float suctionThreshold = 1f; // Distance at which the object gets completely sucked up

    [Header("Layer Settings")]
    public LayerMask ignoreLayerMask;

    [Header("Vacuum Bone Settings")]
    public Transform vacuumBone; // The bone that will enlarge when sucking
    public Vector3 enlargedBoneScale = new Vector3(1.5f, 1.5f, 1.5f); // Larger scale when object is sucked in
    private Vector3 originalBoneScale; // To store the original scale of the bone

    [Header("Effects")]
    [SerializeField] private LauncherDistortion _distortion;
    [SerializeField] private ParticleSystem _shootingParticle;


    [SerializeField] private MeshRenderer suckingConeMesh; // Mesh renderer for the tornado
    private Material suckingConeMaterial;

    [Header("Effects")]
    public float dissolveDuration = 1.5f; 
    public float targetDissolveValue = 1f;
    private float defaultDissolveValue = 0f;

    [SerializeField] private CameraShake cameraShake;

    [Header("Sound")]
    [SerializeField] private AudioSource _sound;
    [SerializeField] private AudioClip _suckingSound, _suckedSound, _shootingSound;

    void Start()
    {
        // Store the original scale of the vacuum bone
        if (vacuumBone != null)
        {
            originalBoneScale = vacuumBone.localScale;
        }

        // Get the material for sucking cone
        if (suckingConeMesh != null)
        {
            suckingConeMaterial = suckingConeMesh.material;
        }
    }
    private bool wasSucking;
    protected override void WeaponUpdate()
    {
        if (InputHandler.GetInput._weaponPressed)
        {
            if (!isVacuumOn && !hasObjectSucked)
            {
                if(!wasSucking)
                {
                    _sound.clip = _suckingSound;
                    _sound.Play();
                    _sound.loop = true;

                    wasSucking = true;
                }

                isVacuumOn = true;
                StartCoroutine(DissolveEffect(targetDissolveValue)); // Start dissolve
            }
        }
        else
        {
            if (isVacuumOn)
            {
                _sound.Stop();
                _sound.loop = false;

                isVacuumOn = false;
                wasSucking = false;

                StartCoroutine(DissolveEffect(defaultDissolveValue));
                RemoveOutlineFx();
            }
        }

        float scrollInput = InputHandler.GetInput._rotateDelta.y;
        if (scrollInput != 0)
        {
            // Adjust rotation based on scroll input
            AdjustRotation(scrollInput);
        }

        if (isVacuumOn && !hasObjectSucked)
        {
            // Continuously check for a new target using a forward raycast
            RaycastHit hit;
            if (Physics.Raycast(suctionPosition.position, suctionPosition.forward, out hit, suctionRange, ~ignoreLayerMask))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Add OutlineFx if the object is not already sucked
                    if (suckedObject != rb)
                    {
                        RemoveOutlineFx(); // Remove outline from previously sucked object
                        AddOutlineFx(rb); // Add outline to the new target
                        suckedObject = rb; // Update the sucked object
                    }
                }
            }

            if (suckedObject != null)
            {
                // Pull the sucked object towards the suction position
                Vector3 direction = (suctionPosition.position - suckedObject.position).normalized;
                suckedObject.AddForce(direction * pullStrength);

                // Check if the object is within the suction threshold
                if (Vector3.Distance(suckedObject.position, suctionPosition.position) < suctionThreshold)
                {
                    _sound.clip = _suckedSound;
                    _sound.loop = false;
                    _sound.Play();

                    cameraShake.ShakeCamera(.3f, .03f);

                    SuckUpObject();

                    StartCoroutine(DissolveEffect(defaultDissolveValue));
                }
            }
        }

        // Launch the sucked object
        if (InputHandler.GetInput.GetTriggerPressed() && suckedObject != null)
        {
            _sound.clip = _shootingSound;
            _sound.loop = false;
            _sound.Play();

            cameraShake.ShakeCamera(.1f, .25f);
            LaunchObject();
        }
    }

    private void SuckUpObject()
    {
        _distortion.PlayBarrelEffectReverse();

        // Parent the object to the suction position and set it to kinematic
        suckedObject.transform.SetParent(suctionPosition);
        suckedObject.isKinematic = true; // Set to kinematic
        suckedObject.gameObject.SetActive(false); // Disable the GameObject

        hasObjectSucked = true; // Set flag to prevent additional objects from being sucked in

        // Enlarge the vacuum bone to simulate something inside
        if (vacuumBone != null)
        {
            vacuumBone.localScale = enlargedBoneScale;
        }
    }

    private void AdjustRotation(float scrollInput)
    {
        // Calculate the change in rotation based on scroll input and speed
        float rotationChange = scrollInput * rotationSpeed * 100f * Time.deltaTime;

        // Get the current local rotation in the -180 to 180 range
        float currentRotationX = transform.localEulerAngles.x;
        currentRotationX = NormalizeAngle(currentRotationX);

        // Adjust the X rotation
        currentRotationX -= rotationChange;

        // Clamp the rotation within the specified limits
        currentRotationX = Mathf.Clamp(currentRotationX, minMaxRotationX.x, minMaxRotationX.y);

        // Apply the new local rotation
        transform.localEulerAngles = new Vector3(currentRotationX, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    // Normalize angle to the -180 to 180 range
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }

    void LaunchObject()
    {
        _shootingParticle.Play();
        _distortion.PlayBarrelEffect();

        suckedObject.gameObject.AddComponent<GrowingObject>();

        // Check if the sucked object has a Citizen component and faint it
        Citizen citizen = suckedObject.GetComponent<Citizen>();
        if (citizen != null)
        {
            suckedObject = citizen.Faint();
        }

        // Unparent the object before launching
        suckedObject.transform.SetParent(null);
        suckedObject.gameObject.SetActive(true); // Make it active again
        suckedObject.isKinematic = false; // Make it non-kinematic to apply physics
        suckedObject.AddForce(suctionPosition.forward * launchForce, ForceMode.Impulse);
        RemoveOutlineFx(); // Remove outline when launching

        // Reset the vacuum bone to its original scale
        if (vacuumBone != null)
        {
            vacuumBone.localScale = originalBoneScale;
        }

        hasObjectSucked = false; // Allow sucking of new objects

        suckedObject = null;
    }
    private IEnumerator DissolveEffect(float targetValue)
    {
        float startValue = suckingConeMaterial.GetFloat("_Dissolve");
        float timeElapsed = 0f;

        while (timeElapsed < dissolveDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / dissolveDuration;
            float dissolveValue = Mathf.Lerp(startValue, targetValue, t);
            suckingConeMaterial.SetFloat("_Dissolve", dissolveValue);
            yield return null;
        }

        // Ensure final value is set
        suckingConeMaterial.SetFloat("_Dissolve", targetValue);
    }

    private void AddOutlineFx(Rigidbody rb)
    {
        // Add OutlineFx.OutlineFx component to the target object
        if (rb != null && rb.gameObject.GetComponent<OutlineFx.OutlineFx>() == null)
        {
            rb.gameObject.AddComponent<OutlineFx.OutlineFx>();
        }
    }

    private void RemoveOutlineFx()
    {
        // Remove OutlineFx from the currently sucked object
        if (suckedObject != null)
        {
            OutlineFx.OutlineFx outline = suckedObject.GetComponent<OutlineFx.OutlineFx>();
            if (outline != null)
            {
                Destroy(outline);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a line in the direction of the suction position in pink
        Gizmos.color = Color.magenta; // Pink color
        Gizmos.DrawLine(suctionPosition.position, suctionPosition.position + suctionPosition.forward * suctionRange);

        // Draw a sphere indicating the suction range in green
        Gizmos.color = Color.green; // Green color
        Gizmos.DrawWireSphere(suctionPosition.position, suctionRange);
    }
}
