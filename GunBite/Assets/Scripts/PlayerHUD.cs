using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    private GameObject healthBar;
    private List<Image> hearts = new List<Image>();
    public Sprite heartFull;
    public Sprite heartEmpty;
    private GameObject armorBar;
    private Image[] shields;
    public Sprite shieldFull;
    public Sprite shieldEmpty;
    private GameObject staminaBar;
    private TextMeshProUGUI clipSize;
    private TextMeshProUGUI ammo;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI expText;
    private Image expBar;
    private TextMeshProUGUI moneyText;
    private GameObject vignette;
    private TextMeshProUGUI deadText;
    private GameObject primaryWeapon;
    private GameObject secondaryWeapon;
    private GameObject meleeWeapon;
    public GameObject reloading;

    public float fadeOutTime = 4f;

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
        primaryWeapon = GameObject.Find("Canvas/GameHUD/BottomRightCorner/PrimaryWeapon").gameObject;
        secondaryWeapon = GameObject.Find("Canvas/GameHUD/BottomRightCorner/SecondaryWeapon").gameObject;
        meleeWeapon = GameObject.Find("Canvas/GameHUD/BottomRightCorner/MeleeWeapon").gameObject;
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
        if (weapons[1] != null)
        {
            Image icon = primaryWeapon.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = primaryWeapon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = weapons[1].icon;
            name.text = weapons[1].name;
            primaryWeapon.SetActive(true);
        }      
        else if (weapons[1] == null)
            primaryWeapon.SetActive(false);

        if (weapons[0] != null)
        {
            Image icon = secondaryWeapon.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = secondaryWeapon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = weapons[0].icon;
            name.text = weapons[0].name;
            secondaryWeapon.SetActive(true);
        }
        else if (weapons[0] == null)
            secondaryWeapon.SetActive(false);
    }

    public void SelectWeapon(int index)
    {
        Image primary = primaryWeapon.GetComponent<Image>();
        Image secondary = secondaryWeapon.GetComponent<Image>();
        Image melee = meleeWeapon.GetComponent<Image>();

        if (index == 0)
        {
            melee.color = new Color32(0, 0, 0, 102);
            primary.color = new Color32(0, 0, 0, 102);
            secondary.color = new Color32(255, 255, 255, 102);
        }
        else if (index == 1)
        {
            melee.color = new Color32(0, 0, 0, 102);
            primary.color = new Color32(255, 255, 255, 102);
            secondary.color = new Color32(0, 0, 0, 102);
        }
        else if (index == 2)
        {
            melee.color = new Color32(255, 255, 255, 102);
            primary.color = new Color32(0, 0, 0, 102);
            secondary.color = new Color32(0, 0, 0, 102);
        }
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