using System.Collections.Generic;
using UnityEngine;

public class SpawnGoose : MonoBehaviour
{
    [SerializeField] private int _amountOfGeese;
    [SerializeField] private GameObject _goose;

    [SerializeField] private Transform[] _spawnPoints;

    private List<int> _usedSpawnPoints = new List<int>();

    private List<GameObject> spawnedGeese = new List<GameObject>();

    [SerializeField] private bool spawnOnAwake;

    private void Awake()
    {
        if (spawnOnAwake)
            TriggerSpawnGeese();
    }

    public void TriggerSpawnGeese()
    {
        for (int i = 0; i < _amountOfGeese; i++)
        {
            int spawnIndex = GetSpawnPoint();
            if (spawnIndex != -1)
            {
                Spawn(spawnIndex);
            }
            else
            {
                Debug.LogWarning("Not enough unique spawn points for the number of geese to spawn.");
                break;
            }
        }
    }
    public void RemoveAllGeese()
    {
        _usedSpawnPoints.Clear();
        foreach (var goose in spawnedGeese)
        {
            if (goose != null)
            {
                spawnedGeese.Remove(goose);
                Destroy(goose);
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

    void Spawn(int spawnIndex)
    {
        RaycastHit hit;
        Transform spawnPoint = _spawnPoints[spawnIndex];

        // Cast a ray downwards from the spawn point
        if (Physics.Raycast(spawnPoint.position, Vector3.down * 999, out hit))
        {
            var spawnPosition = hit.point + new Vector3(0, transform.localScale.y / 2, 0);
            var signal = Instantiate(_goose, spawnPosition, Quaternion.identity);

            spawnedGeese.Add(signal);
        }
        else
        {
            // If the raycast doesn't hit anything, spawn the signal at the spawn point's position
            Instantiate(_goose, spawnPoint.position, Quaternion.identity);
        }
    }
}
