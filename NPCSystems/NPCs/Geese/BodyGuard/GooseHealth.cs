using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooseHealth : Health
{
    [SerializeField] private GameObject _ragdollVersion;


    [SerializeField] private GameObject _effect;

    [Tooltip("If null then nothing will play")]
    [SerializeField] private string _effectSound = "DefaultImpact";

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private GeeseManager geeseManager;

    private bool isInvulnerable;

    private void Awake()
    {
        geeseManager = FindObjectOfType<GeeseManager>();

        Invoke("SetSpawnPosition", 1);
    }
    void SetSpawnPosition()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isInvulnerable)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                HandleEffect(collision.contacts[0].point);
                Die();
            }
        }
        HandleDeadlySurfaceCollision(collision);
    }

    private void HandleDeadlySurfaceCollision(Collision collision)
    {
        if (collision.gameObject.GetComponent<DeadlySurface>() != null)
        {
            var armature = transform.parent.GetChild(1);
            armature.parent = transform;

            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            armature.parent = transform.parent;

            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public override void Die()
    {
        HandleRagdollSpawning();

        //Communicate hit to goosemanager
        geeseManager.HitAGoose(gameObject.GetComponent<Goose>());

        HandleDestroying();
    }

    private void HandleRagdollSpawning()
    {
        var ragdoll = Instantiate(_ragdollVersion, transform.position, transform.rotation);
        ragdoll.transform.eulerAngles += new Vector3(0, -90, 0);
    }

    private void HandleDestroying()
    {
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandleEffect(Vector3 effectSpawningPosition)
    {
        if (_effect != null)
            Instantiate(_effect, effectSpawningPosition, Quaternion.identity);
    }

    #region Handle Invulnerable
    public void SetInvulnerableState(bool state)
    {
        isInvulnerable = state;
    }
    public void ToggleInvulnerable()
    {
        isInvulnerable = !isInvulnerable;
    }
    public bool GetInvulnerableState()
    {
        return isInvulnerable;
    }
    #endregion
}
