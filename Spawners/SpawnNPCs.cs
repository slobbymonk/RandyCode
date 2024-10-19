using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class NPCProbability
{
    public GameObject npcPrefab;
    public float probability; // Chance of this NPC being spawned (percentage)
}

public class SpawnNPCs : MonoBehaviour
{
    [SerializeField] private int _npcsToSpawn;
    [SerializeField] private NPCProbability[] _npcProbabilities;
    [SerializeField] private float _spawnRadius = 10f;
    [SerializeField] private Transform _centerPoint;
    [SerializeField] private float _maxNavMeshDistance = 5f;

    private List<Vector3> _usedSpawnPositions = new List<Vector3>();

    private void Awake()
    {
        if (_centerPoint == null) _centerPoint = transform;

        for (int i = 0; i < _npcsToSpawn; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                SpawnNPC(spawnPosition);
            }
            else
            {
                Debug.LogWarning("Failed to find a valid NavMesh position for NPC spawn.");
            }
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        NavMeshHit hit;

        for (int attempt = 0; attempt < 30; attempt++) // Try up to 30 times to find a valid position
        {
            randomPosition = _centerPoint.position + Random.insideUnitSphere * _spawnRadius;
            randomPosition.y = _centerPoint.position.y;

            if (NavMesh.SamplePosition(randomPosition, out hit, _maxNavMeshDistance, NavMesh.AllAreas))
            {
                if (!_usedSpawnPositions.Contains(hit.position))
                {
                    _usedSpawnPositions.Add(hit.position);
                    return hit.position;
                }
            }
        }
        return Vector3.zero; // Return zero vector if no valid position found after 30 attempts
    }

    void SpawnNPC(Vector3 spawnPosition)
    {
        GameObject selectedNPC = SelectNPC();
        Instantiate(selectedNPC, spawnPosition, Quaternion.identity);
    }

    GameObject SelectNPC()
    {
        float totalProbability = 0f;
        foreach (var npc in _npcProbabilities)
        {
            totalProbability += npc.probability;
        }

        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        foreach (var npc in _npcProbabilities)
        {
            cumulativeProbability += npc.probability;
            if (randomValue < cumulativeProbability)
            {
                return npc.npcPrefab;
            }
        }

        return _npcProbabilities[0].npcPrefab; // Fallback in case something goes wrong
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _spawnRadius);
    }
}