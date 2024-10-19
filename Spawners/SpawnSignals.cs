using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSignals : MonoBehaviour
{
    [SerializeField] private int _signalsToSpawn;
    [SerializeField] private GameObject _signal;

    [SerializeField] private Transform[] _spawnPoints;

    private List<int> _usedSpawnPoints = new List<int>();

    private List<GameObject> spawnedSignals = new List<GameObject>();

    [SerializeField] private bool spawnOnAwake = true;

    private void Awake()
    {
        if(spawnOnAwake)
            TriggerSpawnSignals();
    }

    public void TriggerSpawnSignals()
    {
        for (int i = 0; i < _signalsToSpawn; i++)
        {
            int spawnIndex = GetSpawnPoint();
            if (spawnIndex != -1)
            {
                SpawnSignal(spawnIndex);
            }
            else
            {
                Debug.LogWarning("Not enough unique spawn points for the number of signals to spawn.");
                break;
            }
        }
    }
    public void RemoveAllSignals()
    {
        _usedSpawnPoints.Clear();
        foreach (var signal in spawnedSignals)
        {
            if(signal != null)
            {
                Destroy(signal);
            }
        }
    }

    int GetSpawnPoint()
    {
        if (_usedSpawnPoints.Count >= _spawnPoints.Length)
        {
            return -1; // All spawn points have been used
        }

        int randomInt;
        do
        {
            randomInt = Random.Range(0, _spawnPoints.Length);
        } while (_usedSpawnPoints.Contains(randomInt));

        _usedSpawnPoints.Add(randomInt);
        return randomInt;
    }

    void SpawnSignal(int spawnIndex)
    {
        RaycastHit hit;
        Transform spawnPoint = _spawnPoints[spawnIndex];

        // Cast a ray downwards from the spawn point
        if (Physics.Raycast(spawnPoint.position, Vector3.down, out hit))
        {
            var signal = Instantiate(_signal, hit.point, Quaternion.identity);
            signal.transform.position += new Vector3(0, transform.localScale.y / 2, 0);
            spawnedSignals.Add(signal);

        }
        else
        {
            // If the raycast doesn't hit anything, spawn the signal at the spawn point's position
            Instantiate(_signal, spawnPoint.position, Quaternion.identity);
        }
    }
}
