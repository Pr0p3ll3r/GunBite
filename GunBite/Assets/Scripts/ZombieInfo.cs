using UnityEngine;

public enum Type
{
    Normal,
    Spitter,
    Boomer,
    BossPlant
}

[CreateAssetMenu(fileName = "New Zombie", menuName = "ScriptableObjects/Zombie")]
public class ZombieInfo : ScriptableObject
{
    public Type type;

    [Header("Stats")]
    public int maxHealth;
    private int currentHealth;
    public float attackDistance;
    public float speed;
    public float attackCooldown;

    [Header("Rewards")]
    public int exp;
    public int money;

    public void Init()
    {
        currentHealth = maxHealth;
    }

    public int GetCurrentHealth() 
    { 
        return currentHealth; 
    }
}
