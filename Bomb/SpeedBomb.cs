using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedBomb : MonoBehaviour
{
    ///Put this on an object that you want to explode when it moves too slow, aka a car in my case
    ///- Slobby

    public float _minSpeed;
    [Tooltip("The time you can be under the speedlimit before triggering the bomb")]
    public float _timeBeforeTriggered;
    private float timeUnderSpeedlimit;

    [Tooltip("Does the time immediately reset to default when back above the speed limit or not")]
    [SerializeField] private bool _instantDelayRest;

    [SerializeField] private Speedometer _spedometer;

    [Header("Effects")]
    [SerializeField] private GameObject _explosionEffect;

    public TMP_Text _countdownText;
    private Vector2 _originalCountdownTextSize;


    private bool _startedTicking = true;  //Only play audio once

    //For countdownanimation
    public int previousCountDownSecond;

    private GameManager gameManager;


    public bool _isArmed;

    private void Awake()
    {
        timeUnderSpeedlimit = _timeBeforeTriggered;
        previousCountDownSecond = (int)timeUnderSpeedlimit + 1;

        gameManager = FindAnyObjectByType<GameManager>();

        _originalCountdownTextSize = _countdownText.transform.localScale;
    }
    
    private void Update()
    {
        if (_isArmed)
        {
            if (!IsOverSpeedlimit())
            {
                timeUnderSpeedlimit -= Time.deltaTime;
                _countdownText.gameObject.SetActive(true);
                _countdownText.text = timeUnderSpeedlimit.ToString("F0");

                if ((int)(timeUnderSpeedlimit - .5f) <= previousCountDownSecond)
                {
                    _countdownText.transform.DOShakeScale(0.5f, 0.1f, 10, 90, true);
                    previousCountDownSecond -= 1;
                }

                if (_startedTicking)
                {
                    AudioManager.instance.Play("CarBomb");
                    _startedTicking = false;
                }

                if (TooLongUnderSpeedLimit())
                    TriggerBomb();

            }
            else
            {
                ResetExplosionTimer();
            }
        }
    }

    private void ResetExplosionTimer()
    {
        _startedTicking = true;

        _countdownText.gameObject.SetActive(false);

        AudioManager.instance.Stop("CarBomb");

        if (_instantDelayRest)
            timeUnderSpeedlimit = _timeBeforeTriggered;
        else
        {
            if (timeUnderSpeedlimit < _timeBeforeTriggered)
            {
                timeUnderSpeedlimit += Time.deltaTime;
            }
            else
            {
                timeUnderSpeedlimit = _timeBeforeTriggered;
            }
        }

        previousCountDownSecond = (int)timeUnderSpeedlimit + 1;

        _countdownText.transform.localScale = _originalCountdownTextSize;
    }

    bool IsOverSpeedlimit()
    {
        if (_spedometer._currentSpeed >= _minSpeed)
            return true;
        else
            return false;
    }

    bool TooLongUnderSpeedLimit()
    {
        if(timeUnderSpeedlimit <= 0)
            return true;
        else
            return false;
    }

    void TriggerBomb()
    {
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        AudioManager.instance.Stop("CarBomb");
        AudioManager.instance.Play("Explosion");

        gameObject.SetActive(false);

        gameManager.Lose(false, true, true, null);
    }

    public void ResetBomb()
    {
        timeUnderSpeedlimit = _timeBeforeTriggered;
    }
}
