using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    public NetworkVariable<int> Health = new(100);
    public NetworkVariable<int> Ammo = new(20);
    public NetworkVariable<int> Kills = new(0);

    [SerializeField] NetworkObject projectilePrefab;
    [SerializeField] float fireCooldown = 0.25f;
    float nextFire, facing = 1;

    void Update()
    {
        if (!IsOwner) return;
        float x = Input.GetAxisRaw("Horizontal");
        if (x != 0) facing = Mathf.Sign(x);
        if (Input.GetKey(KeyCode.J)) ShootRpc(facing);
    }

    [Rpc(SendTo.Server)]
    void ShootRpc(float dir)
    {
        if (Ammo.Value <= 0 || Time.time < nextFire) return;
        nextFire = Time.time + fireCooldown;
        Ammo.Value--;
        var p = Instantiate(projectilePrefab, transform.position + Vector3.right * dir * 0.8f, Quaternion.identity);
        p.GetComponent<Projectile>().Init(dir, OwnerClientId);
        p.Spawn();
    }

    public void TakeDamage(int dmg, ulong attackerId)
    {
        Health.Value -= dmg;
        if (Health.Value > 0) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerId, out var c))
            c.PlayerObject.GetComponent<PlayerCombat>().Kills.Value++;

        Health.Value = 100;
        Ammo.Value = 20;
        var spawns = GameObject.FindGameObjectsWithTag("Spawn");
        RespawnRpc(spawns[Random.Range(0, spawns.Length)].transform.position);
    }

    [Rpc(SendTo.Owner)]
    void RespawnRpc(Vector3 pos)
    {
        transform.position = pos;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    public void ResetForNewMatch() // server only
    {
        Health.Value = 100;
        Ammo.Value = 20;
        Kills.Value = 0;
        var spawns = GameObject.FindGameObjectsWithTag("Spawn");
        System.Array.Sort(spawns, (a, b) => a.name.CompareTo(b.name));
        RespawnRpc(spawns[(int)(OwnerClientId % 2)].transform.position);
    }
    public void AddAmmo(int amount) // server only
    {
        Ammo.Value = Mathf.Min(Ammo.Value + amount, 40);
    }

}