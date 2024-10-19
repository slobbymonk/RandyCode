using System.Collections;
using UnityEngine;

[System.Serializable]
public struct SoundSettings
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

public class CarSoundController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the CarController script.")]
    [SerializeField] private CarController _carController;

    [Header("Engine Sounds")]
    [Tooltip("Sound settings for the idle engine sound.")]
    [SerializeField] private SoundSettings _engineIdleSound;
    [Tooltip("Sound settings for the driving engine sound.")]
    [SerializeField] private SoundSettings _engineDrivingSound;
    [Tooltip("Sound settings for the boost sound.")]
    [SerializeField] private SoundSettings _boostSound;

    [Header("Tire Sounds")]
    [Tooltip("Sound settings for the tire skidding sound.")]
    [SerializeField] private SoundSettings _tireSkidSound;
    [Tooltip("Sound settings for the tire skid one-shot sound.")]
    [SerializeField] private SoundSettings _tireSkidOneShotSound;

    [Header("Settings")]
    [Tooltip("Range for pitch variation based on speed.")]
    [SerializeField] private float _pitchRange = 0.5f;
    [Tooltip("Speed of volume transition between idle and driving sounds.")]
    [SerializeField] private float _transitionSpeed = 5.0f;
    [Tooltip("Speed of volume transition for the rocket boost sound.")]
    [SerializeField] private float _boostFadeOutSpeed = 3.0f;
    [Tooltip("Threshold for considering a skid as a long drift.")]
    [SerializeField] private float _skidThreshold = 0.1f;
    [Tooltip("Minimum time for a skid to be considered a long drift.")]
    [SerializeField] private float _minSkidTime = 0.5f;

    private AudioSource _engineIdleAudioSource;
    private AudioSource _engineDrivingAudioSource;
    private AudioSource _boostAudioSource;
    private AudioSource _tireAudioSource;
    private float _skidTime;
    private bool _isSkidding;

    void Start()
    {
        CreateAudioSources();
        StartEngineSounds();
    }

    void Update()
    {
        HandleEngineSounds();
        HandleTireSound();
    }

    private void CreateAudioSources()
    {
        _engineIdleAudioSource = gameObject.AddComponent<AudioSource>();
        _engineIdleAudioSource.loop = true;

        _engineDrivingAudioSource = gameObject.AddComponent<AudioSource>();
        _engineDrivingAudioSource.loop = true;

        _boostAudioSource = gameObject.AddComponent<AudioSource>();
        _boostAudioSource.loop = true;

        _tireAudioSource = gameObject.AddComponent<AudioSource>();
        _tireAudioSource.loop = false;
    }

    private void StartEngineSounds()
    {
        _engineIdleAudioSource.clip = _engineIdleSound.clip;
        _engineIdleAudioSource.volume = _engineIdleSound.volume;
        _engineIdleAudioSource.Play();

        _engineDrivingAudioSource.clip = _engineDrivingSound.clip;
        _engineDrivingAudioSource.volume = 0;
        _engineDrivingAudioSource.Play();

        _boostAudioSource.clip = _boostSound.clip;
        _boostAudioSource.volume = 0;
        _boostAudioSource.Play();
    }

    private void HandleEngineSounds()
    {
        float carSpeedRatio = _carController.GetCarVelocityRatio();
        bool isBoosting = _carController.IsBoosting();

        if (isBoosting)
        {
            _boostAudioSource.volume = Mathf.Lerp(_boostAudioSource.volume, _boostSound.volume, Time.deltaTime * _transitionSpeed);
            _engineIdleAudioSource.volume = Mathf.Lerp(_engineIdleAudioSource.volume, 0, Time.deltaTime * _transitionSpeed);
            _engineDrivingAudioSource.volume = Mathf.Lerp(_engineDrivingAudioSource.volume, 0, Time.deltaTime * _transitionSpeed);
        }
        else
        {
            _boostAudioSource.volume = Mathf.Lerp(_boostAudioSource.volume, 0, Time.deltaTime * _boostFadeOutSpeed);
            _engineIdleAudioSource.volume = Mathf.Lerp(_engineIdleAudioSource.volume, Mathf.Clamp(1 - carSpeedRatio, 0, 1) * _engineIdleSound.volume, Time.deltaTime * _transitionSpeed);
            _engineDrivingAudioSource.volume = Mathf.Lerp(_engineDrivingAudioSource.volume, carSpeedRatio * _engineDrivingSound.volume, Time.deltaTime * _transitionSpeed);
            _engineDrivingAudioSource.pitch = 1.0f + carSpeedRatio * _pitchRange;
        }
    }

    private void HandleTireSound()
    {
        float steerInput = _carController.GetSteerInput();
        float carSpeedRatio = _carController.GetCarVelocityRatio();

        if (Mathf.Abs(steerInput) > 0.1f && carSpeedRatio > 0.1f)
        {
            _skidTime += Time.deltaTime;
            _isSkidding = true;

            if (_skidTime > _minSkidTime)
            {
                if (!_tireAudioSource.isPlaying || _tireAudioSource.clip != _tireSkidSound.clip)
                {
                    PlayTireSound(_tireSkidSound, true);
                }
                _tireAudioSource.volume = Mathf.Lerp(_tireAudioSource.volume, _tireSkidSound.volume * carSpeedRatio, Time.deltaTime * _transitionSpeed);
            }
            else
            {
                PlayTireSoundOneShot();
            }
        }
        else
        {
            if (_isSkidding)
            {
                if (_skidTime <= _minSkidTime)
                {
                    PlayTireSoundOneShot();
                }
                _isSkidding = false;
                _skidTime = 0f;
            }

            _tireAudioSource.volume = Mathf.Lerp(_tireAudioSource.volume, 0, Time.deltaTime * _transitionSpeed);
            if (_tireAudioSource.volume < 0.01f)
            {
                _tireAudioSource.Stop();
            }
        }
    }

    private void PlayTireSound(SoundSettings sound, bool loop)
    {
        _tireAudioSource.clip = sound.clip;
        _tireAudioSource.volume = sound.volume;
        _tireAudioSource.loop = loop;
        _tireAudioSource.Play();
    }

    private void PlayTireSoundOneShot()
    {
        _tireAudioSource.PlayOneShot(_tireSkidOneShotSound.clip, _tireSkidOneShotSound.volume);
    }
}
