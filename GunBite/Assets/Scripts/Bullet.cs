using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime;
    public GameObject explosion;
    public float radius = 10f;

    private int damage;
    private Vector3 mousePos;
    bool isGrenade;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (gameObject.tag.Equals("Grenade")) isGrenade = true;
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Enemy") && !isGrenade)
        {
            Zombie z = collision.gameObject.transform.root.GetComponent<Zombie>();
            BossPlant bp = collision.gameObject.transform.root.GetComponent<BossPlant>();
            if (z != null) z.TakeDamage(damage);
            if (bp != null) bp.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag.Equals("Enemy") && isGrenade)
        {
            Detonate();
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isGrenade)
        {
            transform.position = Vector2.MoveTowards(transform.position, mousePos, Time.deltaTime * WeaponLibrary.FindGun("Grenade Launcher").bulletForce);

            if (transform.position == mousePos)
            {
                Detonate();
            }
        }
    }

    private void Detonate()
    {
        Debug.Log("Explosion");
        Instantiate(explosion, transform.position, Quaternion.identity);
        SoundManager.Instance.PlayOneShot("Explosion");
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
            {
                Vector2 closestPoint = col.ClosestPoint(transform.position);
                float distance = Vector3.Distance(closestPoint, transform.position);

                float damagePercent = Mathf.InverseLerp(radius, 0, distance);
                int damageToApply = damage;

                if (damagePercent < 0.95) damageToApply = (int)(damageToApply * damagePercent);
                //Debug.Log("Damage: " + damageToApply);
                col.gameObject.transform.root.GetComponent<Zombie>().TakeDamage(damageToApply);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
