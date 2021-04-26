using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossPlant : MonoBehaviour
{
    public ZombieInfo info;

    private Transform player;
    public GameObject deathEffect;
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private GameObject hitbox;

    public GameObject spitPoint;
    public GameObject acidPrefab;

    public float radius;

    private int currentHealth;
    private float lastAttackTime = 0;
    private bool isDead;
    private bool appear;

    void Start()
    {
        currentHealth = info.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        moneyReward = transform.GetChild(0).transform.Find("Money").GetComponent<TextMeshProUGUI>();
        hitbox = transform.GetChild(1).gameObject;
    }

    private void Update()
    {
        if (!appear || isDead || player == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= info.attackDistance)
        {
            Attack();
        }
        else
        {
            Spit();
            animator.SetBool("Attack", false);
        }
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= info.attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;
        }
    }

    public void GiveDamage()
    {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.tag.Equals("Player"))
            {
                col.gameObject.transform.root.GetComponent<Player>().TakeDamage();
            }
        }
    }

    public void Appear()
    {
        appear = true;
    }

    void Spit()
    {
        if (Time.time - lastAttackTime >= info.attackCooldown)
        {
            lastAttackTime = Time.time;
            Transform spitPoint = transform.Find("SpitPoint");

            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Instantiate(acidPrefab, spitPoint.position, Quaternion.Euler(new Vector3(0, 0, angle)));
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
                isDead = true;
                player.gameObject.GetComponent<Player>().Reward(info.exp, info.money);
                Instantiate(deathEffect, transform.position, Quaternion.identity);
                moneyReward.text = $"+{info.money}$";
                moneyReward.GetComponent<Animator>().Play("FadeOut");
                GetComponent<SpriteRenderer>().enabled = false;
                transform.GetChild(2).gameObject.SetActive(false);
                GetComponent<Collider2D>().enabled = false;
                hitbox.GetComponent<Collider2D>().enabled = false;
                if (GameManager.Instance != null) GameManager.Instance.waveManager.ZombieQuantity(-1);
                else StartCoroutine(Destroy());
            }
        }
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void SetHealthBar()
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount = (float)currentHealth / info.maxHealth;
        if (healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount <= 0)
            healthBar.gameObject.SetActive(false);
    }
}
