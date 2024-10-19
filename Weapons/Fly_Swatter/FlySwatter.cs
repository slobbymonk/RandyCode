using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class FlySwatter : Weapon
{
    [SerializeField] private float swatDuration = 0.5f;            // Time for the swat animation
    [SerializeField] private float chargeDuration = 0.3f;          // Time for the charge-back animation
    [SerializeField] private AnimationCurve swatCurve;             // Animation curve for both charge and swat
    [SerializeField] private float toggleStretchDuration = 0.5f;   // Duration of the stretch/shrink effect
    [SerializeField] private Vector3 hiddenScale = new Vector3(0,0,0); // Target scale when hidden

    private Quaternion initialLocalRotation;                       // Initial local rotation before charge/swat
    private Quaternion chargeBackRotation;                         // Rotation when charging back
    private Quaternion swatLocalRotation;                          // Target local rotation after swat (90 degrees forward in local space)
    [SerializeField] private bool isSwatting = false;                               // To track if currently swatting


    [SerializeField] private AudioSource _whooshAudioSource;
    [SerializeField] private AudioSource _equipBroomAudioSource;

    protected override void WeaponAwake()
    {
        // Store the initial local rotation of the fly swatter
        initialLocalRotation = transform.localRotation;

        // Calculate the target rotation in local space (rotate 90 degrees forward for swat)
        swatLocalRotation = Quaternion.Euler(transform.localEulerAngles.x + 90f, transform.localEulerAngles.y, transform.localEulerAngles.z);

        // Calculate the charge-back rotation (rotate backward by -30 degrees)
        chargeBackRotation = Quaternion.Euler(transform.localEulerAngles.x - 30f, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    protected override void WeaponUpdate()
    {
        // Trigger swat when key is pressed and not already swatting
        if (InputHandler.GetInput.GetWeaponPressed() && !isSwatting && isVisible)
        {
            StartCoroutine(SwatWithCharge());
        }

        // Toggle swatter visibility when key is pressed
        if (InputHandler.GetInput.GetWeaponTogglePressed())
        {
            _equipBroomAudioSource.Play();
        }
    }

    private IEnumerator SwatWithCharge()
    {
        isSwatting = true;
        float timeElapsed = 0f;

        // Charge phase: rotate backward (wind-up) over the charge duration
        while (timeElapsed < chargeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = swatCurve.Evaluate(timeElapsed / chargeDuration);
            transform.localRotation = Quaternion.Lerp(initialLocalRotation, chargeBackRotation, t);
            yield return null;
        }

        // Ensure final rotation is exactly the charge-back rotation
        transform.localRotation = chargeBackRotation;

        // Reset the timer for the swat phase
        timeElapsed = 0f;

        _whooshAudioSource.Play();

        // Swat phase: rotate forward from the charge-back position to the target swat rotation
        while (timeElapsed < swatDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = swatCurve.Evaluate(timeElapsed / swatDuration);
            transform.localRotation = Quaternion.Lerp(chargeBackRotation, swatLocalRotation, t);
            yield return null;
        }

        // Ensure final rotation is exactly the swat rotation
        transform.localRotation = swatLocalRotation;


        // Reset rotation and allow swatting again after a short delay
        yield return new WaitForSeconds(0.5f);
        transform.localRotation = initialLocalRotation;

        isSwatting = false;
    }


    public bool GetSwattingState()
    {
        return isSwatting;
    }
}
