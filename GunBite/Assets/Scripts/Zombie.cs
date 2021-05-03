using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Zombie : ZombieInfo
{
    private int currentHealth;

    private Transform player;
    public GameObject deathEffect;
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private SpriteRenderer sprite;
    private GameObject hitbox;
    private Rigidbody2D rb;

    public GameObject spitPoint;
    public GameObject acidPrefab;

    public GameObject explodeEffect;
    public int explosionDamage;
    public float radius;

    private float lastAttackTime = 0;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        moneyReward = transform.GetChild(0).transform.Find("Money").GetComponent<TextMeshProUGUI>();
        hitbox = transform.GetChild(1).gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead || player == null)
        {
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -90 && angle <= 90)
        {
            sprite.flipX = false;
        }
        else
        {
            sprite.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            Attack();

            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;

            animator.SetBool("Attack", false);

            Vector3 movePosition = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.fixedDeltaTime);

            rb.MovePosition(movePosition);
        }

    }

    void Attack()
    {
        if(Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;

            if (type == Type.Spitter)
            {
                Transform spitPoint = transform.Find("SpitPoint");

                Vector2 direction = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Instantiate(acidPrefab, spitPoint.position, Quaternion.Euler(new Vector3(0, 0, angle)));
            }
            else
            {
                player.GetComponent<Player>().TakeDamage();
            }
        }         
    }

    public void TakeDamage(int damage)
    {
        if (isDead == false)
        {
            currentHealth -= damage;
            SoundManager.Instance.Play("ZombieHurt");
            SetHealthBar();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public virtual void Die()
    {
        isDead = true;
        player.gameObject.GetComponent<Player>().Reward(exp, money);
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        moneyReward.text = $"+{money}$";
        moneyReward.GetComponent<Animator>().Play("FadeOut");
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        hitbox.GetComponent<Collider2D>().enabled = false;
        if (GameManager.Instance != null) GameManager.Instance.waveManager.ZombieQuantity(-1);
        if (type == Type.Boomer) Explode();
        else StartCoroutine(Destroy());
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void SetHealthBar()
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount = (float)currentHealth / maxHealth;
        if (healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount <= 0)
            healthBar.gameObject.SetActive(false);
    }

    void Explode()
    {
        Instantiate(explodeEffect, transform.position, Quaternion.identity);
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.tag.Equals("Player"))
            {
                col.gameObject.transform.root.GetComponent<Player>().TakeDamage();
            }
        }
        Destroy(gameObject);
    }
}
