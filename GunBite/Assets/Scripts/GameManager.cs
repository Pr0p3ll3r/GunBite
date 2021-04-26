using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isShopTime = false;
    public int shopTime = 30;

    public float startTimestamp = 0;

    private TextMeshProUGUI timer;
    private TextMeshProUGUI shopTimer;
    private int currentGameTime;
    private Coroutine timerCoroutine;
    private GameObject endGame;
    public GameObject wonEffect;

    [HideInInspector] public WaveManager waveManager;

    private Coroutine countingCo;
    private bool started = false;
    private bool shopOpened = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        endGame = GameObject.Find("Canvas").transform.Find("GameOver").gameObject;
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

        if (!Player.Instance.isDead)
        {
            Player.Instance.ls.GetExp(100);
            Player.Instance.Control(false);
            endGame.SetActive(true);
            Instantiate(wonEffect, Vector3.zero, wonEffect.transform.rotation);
        }
            
        StartCoroutine(Wait(5f));
    }

    private void InitializeTimer()
    {
        currentGameTime = 0;
        RefreshTimerUI();

        StartCoroutine(Timer());
    }

    private void RefreshTimerUI()
    {
        string minutes = (currentGameTime / 60).ToString("00");
        string seconds = (currentGameTime % 60).ToString("00");
        timer.text = $"{minutes}:{seconds}";

        string minutesShop = (shopTime / 60).ToString("00");
        string secondsShop = (shopTime % 60).ToString("00");
        shopTimer.text = $"Shop closes in {minutesShop}:{secondsShop}";
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        #if UNITY_EDITOR
            LevelLoader.Instance.LoadScene(1);
        #else
            LevelLoader.Instance.LoadScene(0);
        #endif
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
}