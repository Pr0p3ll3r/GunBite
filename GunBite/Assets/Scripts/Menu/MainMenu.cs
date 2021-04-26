using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ProfileData
{
    public string nickname;
    public int level;
    public int exp;

    public ProfileData()
    {
        this.nickname = "NICKNAME";
        this.level = 1;
        this.exp = 0;
    }

    public ProfileData(string u, int l, int e)
    {
        this.nickname = u;
        this.level = l;
        this.exp = e;
    }
}

public class MainMenu : MonoBehaviour, IPointerEnterHandler
{
    public static ProfileData myProfile = new ProfileData();

    [Header("Profile")]
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    [Header("Tabs")]
    public GameObject mainTab;
    public GameObject optionsTab;

    private void Start()
    {
        Pause.paused = false;
        myProfile = Data.LoadProfile();
        SoundManager.Instance.Play("Theme");
        mainTab.SetActive(true);
        optionsTab.SetActive(false);
    }

    public void PlayGame()
    {
        LevelLoader.Instance.LoadScene(1);
    }

    public void OpenTab(GameObject tab)
    {
        CloseTabs();
        tab.SetActive(true);
        ClickSound();
    }

    private void CloseTabs()
    {
        mainTab.SetActive(false);
        optionsTab.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        Data.SaveProfile(myProfile);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayOneShot("Hover");
    }

    public void ClickSound()
    {
        SoundManager.Instance.PlayOneShot("Click");
    }
}
