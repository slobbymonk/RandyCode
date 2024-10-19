using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombThrower : Weapon
{
    [SerializeField] private GameObject _bombPrefab;
    [SerializeField] private Transform _bombThrowerPosition;

    [SerializeField] private float _throwingStrength;

    [SerializeField] private LauncherDistortion _distortion;

    [SerializeField] private ParticleSystem _shootingParticle;

    [SerializeField] private float _knockBackStrength;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private AudioSource _shootingSound;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector2 minMaxRotationX = new Vector2(-30f, 30f);

    protected override void WeaponUpdate()
    {
        // Get the scroll wheel input
        float scrollInput = InputHandler.GetInput._rotateDelta.y;
        if (scrollInput != 0)
        {
            // Adjust rotation based on scroll input
            AdjustRotation(scrollInput);
        }

        if (InputHandler.GetInput.GetWeaponPressed())
        {
            _distortion.PlayBarrelEffect();
            Invoke("ShootBomb", .1f);
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
    void ShootBomb()
    {
        _shootingParticle.Play(); 
        _shootingSound.Play();
        KnockBack();

        var bomb = Instantiate(_bombPrefab, _bombThrowerPosition.position, Quaternion.identity);

        Rigidbody bombRb = bomb.GetComponent<Rigidbody>();

        bombRb.AddForce(_bombThrowerPosition.forward * _throwingStrength, ForceMode.Impulse);
    }

    void KnockBack()
    {
        _rb.AddForce(-transform.forward * _knockBackStrength, ForceMode.Impulse);
    }

}
