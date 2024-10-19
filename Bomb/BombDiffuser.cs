using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDiffuser : MonoBehaviour
{
    [SerializeField] private BombManager bombManager;

    [SerializeField] private GameObject _deactivationEffect;

    [SerializeField] private AudioManager _audioManager;

    private void Awake()
    {
        bombManager = FindAnyObjectByType<BombManager>();

        _audioManager = FindAnyObjectByType<AudioManager>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            bombManager.DiffusedPoint(this);

            HandleExplosionEffects();

            //Debug.Log("Removed: " + gameObject.name + "- BombDiffuser");
            Destroy(gameObject);
        }
    }

    private void HandleExplosionEffects()
    {
        for (int i = 0; i < 5; i++)
        {
            var random = new Vector3(Random.Range(.1f, .5f), Random.Range(.1f, .5f), Random.Range(.1f, .5f));
            Instantiate(_deactivationEffect, transform.position + random, transform.rotation);
        }

        if (_audioManager != null) _audioManager.Play("Explosion");
    }
}
