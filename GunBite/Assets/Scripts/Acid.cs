using System;
using UnityEngine;

public class Acid : MonoBehaviour
{
    public float lifeTime;
    public float forcePower = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.right * forcePower, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Hitbox"))
        {
            collision.gameObject.transform.root.GetComponent<Player>().TakeDamage();
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
