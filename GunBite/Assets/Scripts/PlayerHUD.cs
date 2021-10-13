using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    private GameObject healthBar;
    private List<Image> hearts = new List<Image>();
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    private GameObject armorBar;
    private Image[] shields;
    [SerializeField] private Sprite shieldFull;
    [SerializeField] private Sprite shieldEmpty;
    private GameObject staminaBar;
    private TextMeshProUGUI clipSize;
    private TextMeshProUGUI ammo;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI expText;
    private Image expBar;
    private TextMeshProUGUI moneyText;
    private GameObject vignette;
    private TextMeshProUGUI deadText;
    private Transform weaponParent;
    public GameObject reloading;

    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private float fadeOutTime = 4f;

    private void Awake()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        healthBar = GameObject.Find("Canvas/GameHUD/BottomLeftCorner/HealthBar2");
        hearts.AddRange(GameObject.Find("Canvas/GameHUD/BottomLeftCorner/HealthBar1").GetComponentsInChildren<Image>());
        hearts.AddRange(GameObject.Find("Canvas/GameHUD/BottomLeftCorner/HealthBar2").GetComponentsInChildren<Image>());
        armorBar = GameObject.Find("Canvas/GameHUD/BottomLeftCorner/ArmorBar");
        shields = GameObject.Find("Canvas/GameHUD/BottomLeftCorner/ArmorBar").GetComponentsInChildren<Image>();
        staminaBar = GameObject.Find("Canvas/GameHUD/StaminaBar/StaminaBarMask/StaminaBar");
        ammo = GameObject.Find("Canvas/GameHUD/BottomRightCorner/Ammo/Ammo").GetComponent<TextMeshProUGUI>();
        clipSize = GameObject.Find("Canvas/GameHUD/BottomRightCorner/Ammo/ClipSize").GetComponent<TextMeshProUGUI>();
        levelText = GameObject.Find("Canvas/GameHUD/Level/LevelText").GetComponent<TextMeshProUGUI>();
        expText = GameObject.Find("Canvas/GameHUD/ExpBar/ExpText").GetComponent<TextMeshProUGUI>();
        expBar = GameObject.Find("Canvas/GameHUD/ExpBar/ExpBarMask/ExpBar").GetComponent<Image>();
        moneyText = GameObject.Find("Canvas/GameHUD/Money").GetComponent<TextMeshProUGUI>();
        vignette = GameObject.Find("Canvas/GameHUD/Vignette").gameObject;
        deadText = GameObject.Find("Canvas/GameHUD/DeadText").GetComponent<TextMeshProUGUI>();
        weaponParent = GameObject.Find("Canvas/GameHUD/BottomRightCorner/Weapons").transform;
        reloading.SetActive(false);
    }

    private IEnumerator FadeToZeroAlpha()
    {
        vignette.GetComponent<CanvasGroup>().alpha = 0.5f;

        while (vignette.GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            vignette.GetComponent<CanvasGroup>().alpha -= (Time.deltaTime / fadeOutTime);
            yield return null;
        }
    }

    public void RefreshBars(int currentHealth, int maxHealth, int currentArmor)
    {
        //health
        //show heart containers
        for (int i = 0; i < hearts.Count; i++)
        {
            if(i < maxHealth)
            {
                hearts[i].gameObject.SetActive(true);
            }
            else
            {
                hearts[i].gameObject.SetActive(false);
            }
        }

        //set right sprite
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = heartFull;
            }
            else
            {
                hearts[i].sprite = heartEmpty;
            }
        }

        //if second health bar
        if (maxHealth >= 11)
        {
            healthBar.SetActive(true);
        }

        //armor
        if(currentArmor > 0)
        {
            armorBar.SetActive(true);
        }
        else
        {
            armorBar.SetActive(false);
        }

        for (int i = 0; i < shields.Length; i++)
        {
            if(i < currentArmor)
            {
                shields[i].sprite = shieldFull;
            }
            else
            {
                shields[i].sprite = shieldEmpty;
            }
        }
    }

    public void RefreshAmmo(int clip, int currentAmmo)
    {
        clipSize.text = clip.ToString();
        ammo.text = currentAmmo.ToString();
    }

    public void UpdateLevel(int level, int exp, int requireExp)
    {
        levelText.text = $"Level: {level}";
        expText.text = $"{exp}/{requireExp}";
        float percentage = (float)exp / (float)requireExp;
        expBar.fillAmount = percentage;
    }

    public void UpdateMoney(int money)
    {
        moneyText.text = $"${money}";
    }

    public void ShowVignette()
    {
        StartCoroutine(FadeToZeroAlpha());
    }

    public void ShowDeadText()
    {
        vignette.GetComponent<CanvasGroup>().alpha = 1f;
        deadText.text = "YOU ARE DEAD!";
    }

    public void RefreshWeapon(Weapon[] weapons)
    {
        foreach (Transform weapon in weaponParent)
        {
            Image icon = weapon.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = weapon.GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = emptyIcon;
            name.text = "";
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null) continue;

            Image icon = weaponParent.GetChild(i).GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = weaponParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = weapons[i].icon;
            name.text = weapons[i].name;
        }
    }

    public void SelectWeapon(int index)
    {
        foreach (Transform weapon in weaponParent)
        {
            Image background = weapon.GetComponent<Image>();
            background.color = new Color32(0, 0, 0, 102);
        }

        weaponParent.GetChild(index).GetComponent<Image>().color = new Color32(255, 255, 255, 102);
    }

    public IEnumerator StaminaRestore(float cooldown)
    {
        Image stamina = staminaBar.GetComponent<Image>();
        stamina.fillAmount = 0;
        while (stamina.fillAmount != 1)
        {
            stamina.fillAmount += (Time.deltaTime / cooldown);
            yield return null;
        }
    }
}