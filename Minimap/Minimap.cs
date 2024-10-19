using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _minimapCamera;
    private Vector3 previousPlayerPos;

    [SerializeField] private Animator _mainSpriteAnimator;

    private Vector3 direction;

    [SerializeField] private Transform _mainIcon;
    [SerializeField] private float _mainIconRotationOffset;

    private void Start(){ if (_minimapCamera == null) _minimapCamera = transform; }

    void Update()
    {
        _minimapCamera.position = new Vector3(_target.position.x, _minimapCamera.position.y, _target.position.z);

        direction = (_target.position - previousPlayerPos).normalized;

        MainSpriteAnimation();

        previousPlayerPos = _target.position;

        var targetRotation = _target.eulerAngles;
        _mainIcon.eulerAngles = new Vector3(_mainIcon.eulerAngles.x, _mainIcon.eulerAngles.y, -targetRotation.y + _mainIconRotationOffset);

        //_minimapCamera.rotation = new Quaternion(_minimapCamera.rotation.x, _target.rotation.y, _minimapCamera.rotation.z, 0);
    }

    private void MainSpriteAnimation()
    {
        if (_mainSpriteAnimator != null)
        {
            _mainSpriteAnimator.SetFloat("Horizontal", direction.x);
            _mainSpriteAnimator.SetFloat("Vertical", direction.z);
        }
    }
}
