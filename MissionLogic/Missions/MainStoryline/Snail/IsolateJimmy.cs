using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IsolateJimmy : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private GameObject _explosionEffect;

    [SerializeField] private float _countDownTime;
    private float timedDown;

    [SerializeField] private GameObject _snail;

    [SerializeField] private GameManager _gameManager;

    private Vector3 _snailSpawnPosition;
    private Vector3 _playerSpawnPosition;

    [SerializeField] private TMP_Text _countDownText;
    [SerializeField] private GameObject _spedometer, _countdown;

    private bool explodeOnce;

    [SerializeField] private GameObject _gooseNPC;
    [SerializeField] private BoxAttachment _boxAttachment;

    public override void WhenStartingMission()
    {
        timedDown = Time.time;

        _snailSpawnPosition = _snail.transform.position;
        _playerSpawnPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

        _countdown.SetActive(true);
        _spedometer.SetActive(false);
    }
    private void Update()
    {
        if(timedDown != 0)
        {
            _countDownText.text = ((timedDown + _countDownTime) - Time.time).ToString("F1");

            if (timedDown + _countDownTime <= Time.time && !explodeOnce)
            {
                Explode(_snail);

                Invoke("ResetMission", 2);

                explodeOnce = true;
            }
        }
    }
    void ResetMission()
    {
        explodeOnce = false;
        timedDown = Time.time; //Reset time

        //Reset Positions
        _snail.transform.position = _snailSpawnPosition; 
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        player.position = _playerSpawnPosition;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        _snail.SetActive(true);

        _gameManager.ResetMission(this); //Reset Mission
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<Towable>() != null)
        {
            if (_missionIsActive)
            {
                MissionCompleted?.Invoke(this, new MissionEventArgs(this));
                _snail = other.gameObject;
                Explode(_snail);

                _countdown.SetActive(false);
                _spedometer.SetActive(true);

                _gooseNPC.SetActive(true);

                Destroy(this);
            }
        }
    }

    private void Explode(GameObject other)
    {
        Instantiate(_explosionEffect, other.transform.position, Quaternion.identity);
        if (AudioManager.instance != null) AudioManager.instance.Play("Explosion");

        _boxAttachment.DetachBox();

        other.transform.gameObject.SetActive(false);
    }
}
