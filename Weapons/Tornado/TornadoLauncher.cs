using UnityEngine;

public class TornadoLauncher : Weapon
{
    [SerializeField] private GameObject tornadoPrefab; // Prefab of the tornado
    [SerializeField] private Transform spawnPoint; // Point where the tornado will spawn
    [SerializeField] private float baseStrength = 10f; // Base strength of the tornado
    [SerializeField] private float baseRadius = 5f; // Base radius of the tornado
    [SerializeField] private float maxMultiplier = 2f; // Maximum multiplier for the strength
    [SerializeField] private float chargeSpeed = 1f; // Speed at which the tornado grows while charging

    private float currentChargeTime = 0f;
    private bool isCharging = false;

    private float chargeAmount = 1f; 
    [SerializeField] private float maxScaleMultiplier = 2f;

    [SerializeField] private Transform _bone;
    private Vector3 _boneStartingScale;
    [SerializeField] private LauncherDistortion _distortion;

    [Header("Shake Effect")]
    [SerializeField] private float shakeMagnitude = 0;
    private Vector3 originalPosition;

    [SerializeField] private AudioSource _sound;
    [SerializeField] private AudioClip _chargingSound, _shootingSound;

    private bool wasCharging;

    private void Start()
    {
        _boneStartingScale = _bone.localScale;
        originalPosition = transform.localPosition;
    }

    protected override void WeaponUpdate()
    {
        if (InputHandler.GetInput._weaponPressed)
        {
            isCharging = true;
            currentChargeTime = 0f; // Reset charge time

            if(!wasCharging)
            {
                _sound.clip = _chargingSound;
                _sound.Play();
            }

            wasCharging = true;
        }

        if (InputHandler.GetInput.WeaponButtonReleased())
        {
            _sound.clip = _shootingSound;
            _sound.Play();

            isCharging = false;
            wasCharging = false;
            LaunchTornado();
        }

        if (isCharging)
        {
            ChargeTornado();

            if (chargeAmount >= maxScaleMultiplier)
            {
                if (!IsInvoking("ShakeLauncher"))
                {
                    InvokeRepeating("ShakeLauncher", 0f, 0.1f); // Start shaking
                }
            }
        }
        else
        {
            CancelInvoke("ShakeLauncher"); // Stop shaking when not charging
        }
    }

    private void ShakeLauncher()
    {
        float x = Random.Range(-shakeMagnitude, shakeMagnitude);
        float y = Random.Range(-shakeMagnitude, shakeMagnitude);
        transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
    }
    private void ChargeTornado()
    {
        // Increase charge amount based on time held
        chargeAmount += Time.deltaTime * chargeSpeed;
        chargeAmount = Mathf.Clamp(chargeAmount, 1f, maxScaleMultiplier); // Cap charge amount

        ScaleBone(chargeAmount);
    }
    private void ScaleBone(float scale)
    {
        if (_bone != null)
        {
            _bone.localScale = Vector3.one * scale; // Scale the bone
        }
    }
    private void LaunchTornado()
    {
        transform.localPosition = originalPosition;
        _bone.localScale = _boneStartingScale;
        _distortion.PlayBarrelEffect();

        GameObject tornado = Instantiate(tornadoPrefab, spawnPoint.position, Quaternion.identity);
        tornado.transform.rotation = spawnPoint.rotation;


        float tornadoRadius = baseRadius * chargeAmount;
        // Instantiate tornado in front of the car

        Tornado tornadoScript = tornado.GetComponent<Tornado>();
        if (tornadoScript != null)
        {
            // Set strength and radius based on charge
            tornadoScript.SetStrength(chargeAmount);
            tornadoScript.SetRadius(tornadoRadius); // Scale radius with charge
            tornado.transform.localScale *= chargeAmount;
        }

        chargeAmount = 1f;

        CancelInvoke("ShakeLauncher"); // Stop shaking when launching
    }
}
