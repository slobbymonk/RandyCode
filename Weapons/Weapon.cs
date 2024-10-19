using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Transform _weaponHolder;

    public bool isVisible;
    public Vector3 originalScale;
    public Coroutine visibilityCoroutine;

    private void Awake()
    {
        originalScale = _weaponHolder.localScale;

        WeaponAwake();
    }
    private void Update()
    {
        if(isVisible)
            WeaponUpdate();
    }
    protected virtual void WeaponAwake() { }
    protected virtual void WeaponUpdate() { }

    public void SetVisiblity(bool visiblity)
    {
        isVisible = visiblity;
    }
}
