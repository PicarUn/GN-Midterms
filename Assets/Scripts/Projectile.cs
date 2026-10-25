using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] float speed = 14f;
    [SerializeField] int damage = 20;
    float dir; ulong shooterId;

    public void Init(float d, ulong shooter) { dir = d; shooterId = shooter; }

    public override void OnNetworkSpawn()
    {
        if (IsServer) Invoke(nameof(Kill), 3f);
    }

    void Update()
    {
        if (IsServer) transform.position += Vector3.right * dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.TryGetComponent<PlayerCombat>(out var target))
        {
            if (target.OwnerClientId == shooterId) return;
            target.TakeDamage(damage, shooterId);
            Kill();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) Kill();
    }

    void Kill() { if (NetworkObject.IsSpawned) NetworkObject.Despawn(); }
}