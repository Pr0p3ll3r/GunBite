using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Enemy
{
	public string name;
	public int amount;
	public GameObject prefab;
	public ZombieInfo info;
}

[System.Serializable]
public class Wave
{
	public string name = "Wave";
	public Enemy[] enemies;
	public float spawnDelay;
}

public class WaveManager : MonoBehaviour
{
	public enum SpawnState 
	{
		SPAWNING, 
		WAITING, 
		COUNTING,
		SHOPTIME
	};

	[Header("Spawn Management")]
	public Wave[] waves;
	private int currentWave = 1;
	public Transform[] spawnPoints;
	public Transform[] spawnPointsBossPlant;

	[Header("Start Values")]
	public int startHealth = 100;
	public float startMoveSpeed = 1f;
	public int startEXP = 3;
	public int startMoney = 4;

	[Header("Current Wave")]
	[SerializeField]
	private int currentHealth;
	[SerializeField]
	private float currentMoveSpeed;
	[SerializeField]
	private int currentDamage;
	[SerializeField]
	private int currentEXP;
	[SerializeField]
	private int currentMoney;

	private TextMeshProUGUI zombieQuantity;
	private TextMeshProUGUI wave;
	private GameManager gm;

	private int currentZombies;
	private float makeSound;

	private SpawnState state = SpawnState.COUNTING;

	void Start()
	{
		gm = GetComponent<GameManager>();
		currentHealth = startHealth;
		currentMoveSpeed = startMoveSpeed;
		currentEXP = startEXP;
		currentMoney = startMoney;
		zombieQuantity = GameObject.Find("GameHUD/ZombieQuantity/Quantity").GetComponent<TextMeshProUGUI>();
		wave = GameObject.Find("GameHUD/Wave/Number").GetComponent<TextMeshProUGUI>();
		wave.text = $"{currentWave}/{waves.Length}";
	}

	void Update()
	{
		if (state == SpawnState.SHOPTIME) return;

        if (state == SpawnState.WAITING)
        {
			if (currentZombies == 0)
			{
				StartCoroutine(WaveCompleted()); 
			}
			else
			{
				//Zombie Sound
				if (makeSound <= 0)
				{
					if (PlayerPrefs.GetInt("Extra") == 1)
						SoundManager.Instance.Play("ZombieExtra");
					else
						SoundManager.Instance.Play("Zombie");
					makeSound = 5f;
				}

				makeSound -= Time.deltaTime;
			}
			return;
		}

		if (state != SpawnState.SPAWNING)
		{
			StartCoroutine(NewWave(waves[currentWave-1]));
		}
	}

	IEnumerator NewWave(Wave _wave)
	{
		state = SpawnState.SPAWNING;
		
		if(currentWave == waves.Length)
        {
			SoundManager.Instance.Play("BossApear");
			foreach (Transform spawnPoint in spawnPointsBossPlant)
			{
				Instantiate(_wave.enemies[0].prefab, spawnPoint.position, spawnPoint.rotation);

				ZombieQuantity(1);
			}
		}
		else
        {
			foreach (Enemy enemy in _wave.enemies)
			{
				for (int i = 0; i < enemy.amount; i++)
				{
					SpawnEnemy(enemy.prefab);
					yield return new WaitForSeconds(_wave.spawnDelay);
				}
			}
		}

		state = SpawnState.WAITING;

		yield break;
	}

	IEnumerator WaveCompleted()
    {
		Debug.Log("Wave Completed!");

		state = SpawnState.SHOPTIME;

		if(currentWave >= waves.Length)
        {
			gm.Gameover();
			yield break;
        }
		else
        {
			if (Player.Instance.GetComponent<LevelSystem>().TimeToUpgrade())
			{
				yield return new WaitUntil(() => GameManager.Instance.isShopTime = true);
			}
			else
			{
				GameManager.Instance.isShopTime = true;
				Shop.Instance.shopText.text = "Press F to open the Shop";
			}		
		}

		while(gm.isShopTime)
		{
			yield return null;
		} 
		
		if(!gm.isShopTime)
        {
			currentWave++;
			state = SpawnState.COUNTING;
			Shop.Instance.shopText.text = "";
			Shop.Instance.CloseShop();
			wave.text = $"{currentWave}/{waves.Length}";
			gm.shopTime = 15;
			UpgradeEnemy();
			Player.Instance.RefillHealth();
			SoundManager.Instance.Play("StartGame");
		}
		yield break;
	}

	void SpawnEnemy(GameObject enemy)
    {
		Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

		GameObject zombieGO = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);

		Zombie zombie = zombieGO.GetComponent<Zombie>();
		zombie.maxHealth = currentHealth;
		zombie.damage = currentDamage;
		zombie.exp = currentEXP;
		zombie.money = currentMoney;
		zombie.speed = currentMoveSpeed;

		ZombieQuantity(1);
	}

	void UpgradeEnemy()
	{
		Debug.Log("ENEMY UPGRADED");

		currentHealth += 5;

		if (currentMoveSpeed < 6.0f)
		{
			currentMoveSpeed += 0.4f;
		}

		currentEXP++;
		currentMoney += 5;
	}

	public void ZombieQuantity(int amount)
    {
		currentZombies += amount;
		zombieQuantity.text = currentZombies.ToString();
    }
}
