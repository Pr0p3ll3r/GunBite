using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance;

    public int maxHealth = 3;
    public int currentHealth;
    public int currentArmor = 0;
    public bool isDead;
    public bool invincible;
    public ProfileData playerProfile;
    public GameObject deathEffect;
    private PlayerHUD hud;
    [HideInInspector] public LevelSystem ls;
    private MoneySystem ms;
    private Animator animator;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        hud = GetComponent<PlayerHUD>();
        ls = GetComponent<LevelSystem>();
        ms = GetComponent<MoneySystem>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        hud.RefreshBars(currentHealth, maxHealth, currentArmor);
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1);
        }
        #endif
    }

    public void TakeDamage(int damage)
    {
        if (isDead == false && invincible == false)
        {
            invincible = true;
            SoundManager.Instance.Play("PlayerHurt");
            animator.SetTrigger("GetHit");

            hud.ShowVignette();

            if (currentArmor > 0)
            {
                currentArmor -= 1;
            }
            else
            {
                currentHealth -= 1;
            }

            hud.RefreshBars(currentHealth, maxHealth, currentArmor);

            if (currentHealth <= 0)
            {
                Die();
            }
        }   
    }
    
    public void Die()
    {
        if (PlayerPrefs.GetInt("Extra") == 1)
            SoundManager.Instance.Play("PlayerDeathExtra");
        else
            SoundManager.Instance.Play("PlayerDeath");
        isDead = true;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        hud.ShowDeadText();
        GameManager.Instance.Gameover();
        Destroy(gameObject);
    }

    public void Reward(int exp, int money)
    {
        ls.GetExp(exp);
        ms.GetMoney(money);
    }

    public void RefillHealth()
    {
        currentHealth = maxHealth;
        hud.RefreshBars(currentHealth, maxHealth, currentArmor);
    }

    public void Control(bool status)
    {
        GetComponent<PlayerController>().enabled = status;
        GetComponent<WeaponManager>().enabled = status;
    }

    public void DisableInvincible()
    {
        invincible = false;
    }
}