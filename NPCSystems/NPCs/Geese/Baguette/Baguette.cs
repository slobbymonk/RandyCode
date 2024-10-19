using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Baguette : Damaging
{
    [HideInInspector] public GameObject instantiator;

    [SerializeField] private GameObject _effect;

    [Tooltip("If null then nothing will play")]
    [SerializeField] private string _effectSound = "DefaultImpact";

    [SerializeField] private GameObject _audioPlayer;

    [SerializeField] private AudioClip _impactSound;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject != instantiator)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<CarHealth>().Damage(1);
            }
            HandleEffect(collision);
            HandleAudio();

            Kill(collision.gameObject);

            Destroy(gameObject);
        }
    }

    private void HandleEffect(Collision collision)
    {
        if (_effect != null)
            Instantiate(_effect, collision.contacts[0].point, Quaternion.identity);
    }

    private void HandleAudio()
    {
        var audio = Instantiate(_audioPlayer, transform.position, Quaternion.identity);

        audio.GetComponent<AudioSource>().clip = _impactSound;
        audio.GetComponent<AudioSource>().Play();
    }
}
