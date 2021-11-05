using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isShopTime = false;
    public int shopTime = 30;

    [SerializeField] private float startTimestamp = 0;

    private TextMeshProUGUI timer;
    private TextMeshProUGUI shopTimer;
    private int currentGameTime;
    private Coroutine timerCoroutine;
    private GameObject gameOver;
    [SerializeField] private GameObject wonEffect;

    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI timeSurvivedScoreText;
    [SerializeField] private TextMeshProUGUI zombieKilledText;
    [SerializeField] private TextMeshProUGUI zombieKilledScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [HideInInspector] public WaveManager waveManager;

    private Coroutine countingCo;
    private bool started = false;
    private bool shopOpened = false;

    public ObjectPooler acidPooler;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameOver = GameObject.Find("Canvas").transform.Find("GameOver").gameObject;
        timer = GameObject.Find("GameHUD/Timer/Time").GetComponent<TextMeshProUGUI>();
        shopTimer = GameObject.Find("Canvas").transform.Find("ShopUI/INFO/Text").GetComponent<TextMeshProUGUI>();
        waveManager = GetComponent<WaveManager>();
    }

    private void Update()
    {
        if (started == false)
        {
            if (countingCo == null) countingCo = StartCoroutine(CountingSound());
            timer.text = "Prepare to fight..." + (1 + (int)(startTimestamp - Time.timeSinceLevelLoad));
            if (Time.timeSinceLevelLoad >= startTimestamp) StartGame();
            return;
        }       

        if(isShopTime && Player.Instance.GetComponent<WeaponManager>().isReloading == false)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if(shopOpened)
                    Shop.Instance.CloseShop();
                else
                    Shop.Instance.OpenShop();

                shopOpened = !shopOpened;
            }
        }
    }

    IEnumerator CountingSound()
    {
        for (int i = (int)startTimestamp; i > 0; i--)
        {
            SoundManager.Instance.Play("CountingSound");
            yield return new WaitForSeconds(1f);
        }
    }

    void StartGame()
    {
        Debug.Log("Game Started");
        started = true;
        InitializeTimer();
        waveManager.enabled = true;
        SoundManager.Instance.Play("StartGame");
    }

    public void Gameover()
    {
        waveManager.enabled = false;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        //end game // won game
        //if (!Player.Instance.isDead)
        //{
        //    Player.Instance.ls.GetExp(100);
        //    Player.Instance.Control(false);
        //    gameOver.SetActive(true);
        //    Instantiate(wonEffect, Vector3.zero, wonEffect.transform.rotation);
        //}
            
        StartCoroutine(Wait(3f));
    }

    private void InitializeTimer()
    {
        currentGameTime = 0;
        RefreshTimerUI();

        StartCoroutine(Timer());
    }

    private void RefreshTimerUI()
    {
        string hours = (currentGameTime / 3600).ToString("00");
        int m = currentGameTime % 3600;
        string minutes = (m / 60).ToString("00");
        string seconds = (m % 60).ToString("00");
        timer.text = $"{hours}:{minutes}:{seconds}";

        string minutesShop = (shopTime / 60).ToString("00");
        string secondsShop = (shopTime % 60).ToString("00");
        shopTimer.text = $"Shop closes in {minutesShop}:{secondsShop}";
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        gameOver.SetActive(true);
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentGameTime += 1;
        if (isShopTime)
            shopTime -= 1;
        if (shopTime == 0)
            isShopTime = false;

        RefreshTimerUI();
        timerCoroutine = StartCoroutine(Timer());
    }

    public void TimerStatus(bool status)
    {
        if (status) timerCoroutine = StartCoroutine(Timer());
        else StopCoroutine(timerCoroutine);
    }

    public void SetGameOverScreen(int zombieKilled)
    {
        string hours = (currentGameTime / 3600).ToString("00");
        int m = currentGameTime % 3600;
        string minutes = (m / 60).ToString("00");
        string seconds = (m % 60).ToString("00");
        timeSurvivedText.text = $"{hours}:{minutes}:{seconds}";
        timeSurvivedScoreText.text = $"+{currentGameTime}";

        zombieKilledText.text = zombieKilled.ToString();
        int zombieScore = 10 * zombieKilled;
        zombieKilledScoreText.text = $"+{zombieScore}";

        int totalScore = zombieScore + currentGameTime;
        currentScoreText.text = totalScore.ToString();

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = highScore.ToString();

        if (totalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", totalScore);
            highScoreText.text = totalScore.ToString();
        }
    }
}