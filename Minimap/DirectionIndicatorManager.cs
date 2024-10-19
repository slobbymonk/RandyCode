using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct DirectionIndicator
{
    public string indicatorID;
    public Sprite indictorSprite;
    public Transform target;
    public GameObject indicatorObject;
}

public class DirectionIndicatorManager : MonoBehaviour
{
    [SerializeField] private List<DirectionIndicator> _directionIndicators;
    [SerializeField] private Sprite _defaultIndicatorSprite;
    [SerializeField] private Transform _directionStartTransform;
    [SerializeField] private Vector2 _indicatorSize = new Vector2(.35f, .35f);
    [SerializeField] private float _minimapRadius;
    [SerializeField] private float _angleOffset = 0f;
    [SerializeField] private float _switchDistance = 10f; // Distance at which to switch to position-based indicators

    private RectTransform _minimapRectTransform;

    private bool fullScreenMode;

    [SerializeField] private Transform _minimapCenterTransform;

    private void Start()
    {
        _minimapRectTransform = GetComponent<RectTransform>();
        // Initialization code if needed
    }

    private void Update()
    {
        UpdateIndicators();
        CheckForBrokenIndicators();
    }

    private void UpdateIndicators()
    {
        foreach (var directionIndicator in _directionIndicators)
        {
            if (directionIndicator.target != null && directionIndicator.indicatorObject != null)
            {
                UpdateDirectionBasedIndicator(directionIndicator);
            }
        }
    }

    private void UpdateDirectionBasedIndicator(DirectionIndicator directionIndicator)
    {
        Vector3 worldDirection = directionIndicator.target.position - _directionStartTransform.position;

        float angle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
        angle += _angleOffset;

        float angleRad = angle * Mathf.Deg2Rad;
        Vector2 newPosition = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * _minimapRadius;

        directionIndicator.indicatorObject.GetComponent<RectTransform>().localPosition = newPosition;
    }


    void CheckForBrokenIndicators()
    {
        foreach (var directionIndicator in _directionIndicators)
        {
            if (directionIndicator.target == null)
            {
                //Debug.Log("Broken indicator: " + directionIndicator.indicatorID);
                RemoveDirectionIndicator(directionIndicator.indicatorID);
            }

        }
    }
    public void AddDirectionIndicator(string indicatorID, Sprite indicatorSprite, Transform target)
    {
        if (target != null)
        {
            if (indicatorSprite == null)
            {
                indicatorSprite = _defaultIndicatorSprite;
            }
            DirectionIndicator newIndicator = new DirectionIndicator
            {
                indicatorID = indicatorID,
                indictorSprite = indicatorSprite != null ? indicatorSprite : _defaultIndicatorSprite,
                target = target,
                indicatorObject = null
            };

            CreateIndicatorObject(ref newIndicator);
            _directionIndicators.Add(newIndicator);

            StartCoroutine(PulseEffect(newIndicator.indicatorObject));

            // Debugging: Log the addition of a new indicator
            //Debug.Log($"Added new indicator: {indicatorID}");
        }
    }

    private void CreateIndicatorObject(ref DirectionIndicator indicator)
    {
        GameObject indicatorObject = new GameObject(indicator.indicatorID);
        indicatorObject.transform.SetParent(this.transform);
        indicatorObject.transform.localScale = _indicatorSize;

        Image indicatorImage = indicatorObject.AddComponent<Image>();
        indicatorImage.sprite = indicator.indictorSprite;

        indicator.indicatorObject = indicatorObject;

        // Debugging: Log the creation of a new indicator object
        //Debug.Log($"Created indicator object: {indicator.indicatorID}");
    }
    [SerializeField] private float _pulseDuration = 0.5f; // Duration of the pulse effect
    [SerializeField] private float _pulseScaleMultiplier = 1.5f; // Scale multiplier for the pulse effect
    private IEnumerator PulseEffect(GameObject indicatorObject)
    {
        if (indicatorObject != null)
        {
            RectTransform rectTransform = indicatorObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 originalScale = rectTransform.localScale;
                float elapsedTime = 0f;

                while (elapsedTime < _pulseDuration)
                {
                    // Calculate scale based on a sine wave
                    float scaleMultiplier = 1 + (_pulseScaleMultiplier - 1) * Mathf.Abs(Mathf.Sin(elapsedTime / _pulseDuration * Mathf.PI * 2));
                    rectTransform.localScale = originalScale * scaleMultiplier;

                    elapsedTime += Time.deltaTime;
                    yield return null; // Wait until the next frame
                }

                // Ensure the final scale is set correctly
                rectTransform.localScale = originalScale;
            }
        }
    }


    public void RemoveDirectionIndicator(string indicatorID)
    {
        DirectionIndicator? indicatorToRemove = null;
        foreach (var directionIndicator in _directionIndicators)
        {
            if (directionIndicator.indicatorID == indicatorID)
            {
                indicatorToRemove = directionIndicator;
                break;
            }
        }

        if (indicatorToRemove != null)
        {
            Destroy(((DirectionIndicator)indicatorToRemove).indicatorObject);
            _directionIndicators.Remove((DirectionIndicator)indicatorToRemove);

            // Debugging: Log the removal of an indicator
            //Debug.Log($"Removed indicator: {indicatorID}");
        }
        else
        {
            // Debugging: Log if the indicator was not found
            Debug.LogWarning($"Indicator {indicatorID} not found!");
        }
    }
    public Transform GetDirectionIndicatorTarget(string indicatorID)
    {
        foreach (var directionIndicator in _directionIndicators)
        {
            if (directionIndicator.indicatorID == indicatorID)
            {
                return directionIndicator.target;
            }
        }
        return null;
    }


    public void SetFullscreen(bool state)
    {
        fullScreenMode = state;
    }
}