using Unity.Netcode;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] NetworkObject ammoPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float interval = 15f;

    float timer;
    NetworkObject current;

    void Update()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer) return;

        if (!MatchManager.Active)
        {
            timer = 0f;
            Clear();
            return;
        }

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            Clear();
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            current = Instantiate(ammoPrefab, point.position, Quaternion.identity);
            current.Spawn();
        }
    }

    void Clear()
    {
        if (current != null && current.IsSpawned) current.Despawn();
        current = null;
    }
}