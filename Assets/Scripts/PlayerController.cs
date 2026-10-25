using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();

        if (IsOwner)
        {
            var spawns = GameObject.FindGameObjectsWithTag("Spawn");
            System.Array.Sort(spawns, (a, b) => a.name.CompareTo(b.name));
            transform.position = spawns[(int)(OwnerClientId % 2)].transform.position;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            enabled = false;
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
        if (grounded && Input.GetButtonDown("Jump"))
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}