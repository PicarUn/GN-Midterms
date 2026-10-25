using Unity.Netcode;
using UnityEngine;

public class AmmoPickup : NetworkBehaviour
{
    [SerializeField] int ammoAmount = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.TryGetComponent<PlayerCombat>(out var player)) return;

        player.AddAmmo(ammoAmount);
        if (NetworkObject.IsSpawned) NetworkObject.Despawn();
    }
}