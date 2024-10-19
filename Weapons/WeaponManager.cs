using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon[] _weapon;
    [SerializeField] private int _currentWeaponIndex;


    [SerializeField] private float toggleStretchDuration = 0.5f; 
    [SerializeField] private AudioSource _switchingWeaponSound;

    private void Awake()
    {
        foreach (var weapon in _weapon)
        {
            weapon._weaponHolder.gameObject.SetActive(true);
        }
    }
    private void Start()
    {
        foreach (var weapon in _weapon)
        {
            weapon.isVisible = false;
            weapon._weaponHolder.gameObject.SetActive(false);
        }

        _weapon[_currentWeaponIndex]._weaponHolder.gameObject.SetActive(true);
        _weapon[_currentWeaponIndex].isVisible = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleVisibility(_weapon[_currentWeaponIndex], true);
        }
    }
    public void ChangeWeapon(Weapon weapon)
    {
        ToggleVisibility(weapon, true);
    }

    private Coroutine show, hide;
    private void ToggleVisibility(Weapon weapon, bool state)
    {
        if (state) // If we're showing the new weapon
        {
            // First hide the current weapon
            if (_weapon[_currentWeaponIndex].isVisible)
            {
                hide = StartCoroutine(AnimateVisibility(false, _weapon[_currentWeaponIndex]._weaponHolder, _weapon[_currentWeaponIndex].originalScale));
                _weapon[_currentWeaponIndex].isVisible = false;
            }

            // Update current weapon index
            _currentWeaponIndex++;
            if (_currentWeaponIndex >= _weapon.Length)
            {
                _currentWeaponIndex = 0;
            }

            // Now show the new weapon
            Weapon newWeapon = _weapon[_currentWeaponIndex];
            newWeapon.isVisible = true;
            show = StartCoroutine(AnimateVisibility(true, newWeapon._weaponHolder, newWeapon.originalScale));
        }
    }

    private IEnumerator AnimateVisibility(bool show, Transform weapon, Vector3 originalScale)
    {
        Vector3 initialScale = weapon.localScale;
        Vector3 stretchUpScale = new Vector3(0.5f * originalScale.x, 0.5f * originalScale.y, 1.4f * originalScale.z); // Initial stretch scale for reveal
        Vector3 targetScale = show ? originalScale : Vector3.zero; // Final scale (original if showing, zero if hiding)

        float timeElapsed = 0f;
        float stretchDuration = toggleStretchDuration * 0.7f;  // 70% for the stretch phase
        float collapseDuration = toggleStretchDuration * 0.3f; // 30% for the collapse phase

        if (!show)
        {
            // Phase 1: Stretch up (Y increases, X/Z shrink)
            while (timeElapsed < stretchDuration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / stretchDuration;
                weapon.localScale = Vector3.Lerp(initialScale, stretchUpScale, t); // Stretch Y and shrink X/Z

                yield return null;
            }

            // Phase 2: Collapse to (0,0,0)
            timeElapsed = 0f;
            while (timeElapsed < collapseDuration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / collapseDuration;
                weapon.localScale = Vector3.Lerp(stretchUpScale, targetScale, t); // Collapse to zero

                yield return null;
            }

            // Ensure final scale matches zero when hidden
            weapon.localScale = targetScale;
        }
        else
        {
            weapon.gameObject.SetActive(show);

            timeElapsed = 0f;
            while (timeElapsed < toggleStretchDuration * 2)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / toggleStretchDuration;
                weapon.localScale = Vector3.Lerp(stretchUpScale, originalScale, t); // Stretch back to normal proportions

                yield return null;
            }

            // Ensure final scale matches the original when shown
            weapon.localScale = originalScale;
        }
    }
}
