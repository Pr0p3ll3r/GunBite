using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class WeaponManager : MonoBehaviour
{
    public Weapon[] loadout;
    public GameObject currentWeapon;
    public Transform weaponHolder;
    public int selectedWeapon = 0;
    public Weapon currentWeaponData;
   
    public GameObject emptyCase;
    public AudioSource sfx;
    public AudioSource weaponSound;
    public bool isReloading = false;
    public bool isEquipping = false;

    private float currentCooldown;
    private PlayerHUD hud;
    private Vector3 weaponPosition;

    private Coroutine muzzle;
    private Coroutine equip;
    private Coroutine reloadHud;
    private Coroutine reload;
    private bool burst;

    private void Start()
    {
        hud = GetComponent<PlayerHUD>();
        foreach (Weapon weapon in loadout)
        {
            if(weapon != null)
                weapon.Initialize();
        }

        equip = StartCoroutine(Equip(0));
        hud.RefreshWeapon(loadout);
    }

    void Update()
    {
        if (Pause.paused || GetComponent<Player>().isDead) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && loadout[0] != null && selectedWeapon != 0)
            equip = StartCoroutine(Equip(0));

        if (Input.GetKeyDown(KeyCode.Alpha2) && loadout[1] != null && selectedWeapon != 1)
            equip = StartCoroutine(Equip(1));

        //if (Input.GetKeyDown(KeyCode.Alpha3) && loadout[2] != null && selectedWeapon != 2 )
        //    equip = StartCoroutine(Equip(2));

        if (currentWeapon != null)
        {
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon.transform.localPosition != weaponPosition)
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, weaponPosition, Time.deltaTime * 4f);

            if (!isEquipping)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    if (currentCooldown <= 0)
                    {
                        StartCoroutine(Attack());
                    }
                }
                else
                {
                    if (loadout[selectedWeapon].firingMode == 0)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBullet()) Shoot();
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }
                    else if(loadout[selectedWeapon].firingMode == 2)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBurst()) StartCoroutine(Burst());
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBullet()) Shoot();
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R)) if (currentWeaponData.OutOfAmmo() && isReloading == false) reload = StartCoroutine(Reload());
                }              
            }
        }
    }

    IEnumerator Equip(int index)
    {
        if (burst) yield break;

        isEquipping = true;

        selectedWeapon = index;
        currentWeaponData = loadout[selectedWeapon];

        SoundManager.Instance.Play("Equip");

        if (currentWeapon != null)
        {
            if (isReloading)
            {
                StopCoroutine(reload);
                StopCoroutine(reloadHud);
                hud.reloading.SetActive(false);
            }
            if (muzzle != null) StopCoroutine(muzzle);
            if (equip != null) StopCoroutine(equip);
            weaponSound.Stop();
            isReloading = false;
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(loadout[index].prefab, weaponHolder) as GameObject;
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[index].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        if (currentWeaponData.equipSound != null)
        {
            weaponSound.clip = currentWeaponData.equipSound;
            weaponSound.Play();
        }

        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
        if (currentWeapon.GetComponent<Animator>() != null)
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Equip"))) 
                currentWeapon.GetComponent<Animator>().Play("Equip", 0, 0);

        hud.SelectWeapon(selectedWeapon);

        yield return new WaitForSeconds(1f);
        isEquipping = false;
    }

    void Shoot()
    {
        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());

        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.pitch = 1 - currentWeaponData.pitchRandom + Random.Range(-currentWeaponData.pitchRandom, currentWeaponData.pitchRandom);
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //slide sound
        if (currentWeaponData.slideSound != null)
        {
            sfx.clip = currentWeaponData.slideSound;
            sfx.PlayOneShot(sfx.clip);
        }

        //muzzle
        SpriteRenderer muzzleFlash = currentWeapon.transform.Find("Anchor/Resources/MuzzleFlash").GetComponent<SpriteRenderer>();
        muzzle = StartCoroutine(MuzzleFlash(muzzleFlash));

        //firepoint
        Transform firePoint = currentWeapon.transform.Find("Anchor/Resources/FirePoint").transform;

        //animation
        if(currentWeapon.GetComponent<Animator>() != null)
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Shoot")))
                currentWeapon.GetComponent<Animator>().Play("Shoot", 0, 0);

        for (int i = 0; i < Mathf.Max(1, currentWeaponData.pellets); i++)
        {
            if (currentWeaponData.pellets == 0)
            {
                GameObject bullet = Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage());
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                rb.AddForce(firePoint.right * currentWeaponData.bulletForce, ForceMode2D.Impulse);
            }
            else
            {
                float maxSpread = currentWeaponData.pelletsSpread;

                Vector3 direction = firePoint.right + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread));
                GameObject bullet = Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage());
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                rb.AddForce(direction * currentWeaponData.bulletForce, ForceMode2D.Impulse);
            }
        }

        //caseOut
        //OnCaseOut();

        //gun fx
        currentWeapon.transform.position -= currentWeapon.transform.right * currentWeaponData.kickback;

        //cooldown
        currentCooldown = currentWeaponData.fireRate;
    }

    IEnumerator Attack()
    {
        Destroy(currentWeapon);

        hud.SelectWeapon(2);

        currentWeaponData = loadout[2];
        GameObject newWeapon = Instantiate(loadout[2].prefab, weaponHolder) as GameObject;
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[2].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //animation
        currentWeapon.GetComponent<Animator>().Play("Attack", 0, 0);

        //attackpoint
        Transform attackPoint = currentWeapon.transform.Find("Anchor/Resources/AttackPoint").transform;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.range);
        foreach(Collider2D enemy in enemies)
        {
            if (enemy.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
            {
                Zombie z = enemy.gameObject.transform.root.GetComponent<Zombie>();
                BossPlant bp = enemy.gameObject.transform.root.GetComponent<BossPlant>();
                BossHead bh = enemy.gameObject.transform.root.GetComponent<BossHead>();
                if (z != null) z.TakeDamage(currentWeaponData.damage);
                if (bp != null) bp.TakeDamage(currentWeaponData.damage);
                if (bh != null) bh.TakeDamage(currentWeaponData.damage);
            }
        }

        //cooldown
        currentCooldown = currentWeaponData.fireRate;

        yield return new WaitForSeconds(0.5f);

        equip = StartCoroutine(Equip(selectedWeapon));
    }

    IEnumerator Reload()
    {
        weaponSound.Stop();

        isReloading = true;

        if (currentWeapon.GetComponent<Animator>() != null)
        {
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Reload")))
                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
        }

        if (reloadHud != null) StopCoroutine(reloadHud);

        if (currentWeaponData.insert)
        {
            do
            {
                if (reloadHud != null) StopCoroutine(reloadHud);
                if (!currentWeaponData.OutOfAmmo())
                {
                    isReloading = false;
                    StopCoroutine(reload);
                }   
                reloadHud = StartCoroutine(ReloadingCircle(currentWeaponData.insertTime));
                weaponSound.PlayOneShot(currentWeaponData.reloadSound);
                yield return new WaitForSeconds(currentWeaponData.insertTime);
                currentWeaponData.Reload();
                hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
            }
            while (currentWeaponData.GetClip() != currentWeaponData.clipSize);
        }
        else
        {

            reloadHud = StartCoroutine(ReloadingCircle(currentWeaponData.reloadTime));
            weaponSound.clip = currentWeaponData.reloadSound;
            weaponSound.Play();
            yield return new WaitForSeconds(currentWeaponData.reloadTime);
            currentWeaponData.Reload();
            hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
            yield return new WaitForSeconds(0.2f);
        }

        isReloading = false;
    }

    void OnCaseOut()
    {
        Transform caseSpawnPoint = currentWeapon.transform.Find("Anchor/Resources/CaseSpawn");

        if (caseSpawnPoint == null) return;

        GameObject ejectedCase = Instantiate(emptyCase, caseSpawnPoint.position, Quaternion.identity);
        Rigidbody2D caseRigidbody = ejectedCase.GetComponent<Rigidbody2D>();
        caseRigidbody.velocity = caseSpawnPoint.TransformDirection(-Vector3.left * 5f);
        caseRigidbody.AddTorque(Random.Range(-0.2f, 0.2f));
        caseRigidbody.AddForce(new Vector2(0, Random.Range(2.0f, 4.0f)), ForceMode2D.Impulse);

        Destroy(ejectedCase, 5f);
    }

    IEnumerator MuzzleFlash(SpriteRenderer muzzle)
    {
        muzzle.enabled = true;

        for(int i = 0; i < 1; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        muzzle.enabled = false;
    }

    public void SwitchToSecondary()
    {
        selectedWeapon = 0;
        currentWeaponData = loadout[selectedWeapon];

        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine(reload);
            if (muzzle != null) StopCoroutine(muzzle);
            if (equip != null) StopCoroutine(equip);
            isReloading = false;
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(loadout[0].prefab, weaponHolder) as GameObject;
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[0].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());

        hud.SelectWeapon(selectedWeapon);

        isEquipping = false;
    }

    IEnumerator Burst()
    {
        burst = true;

        for (int i = 0; i < 3; i++)
        {
            Shoot();

            yield return new WaitForSeconds(0.11f);
        }

        burst = false;
    }

    IEnumerator ReloadingCircle(float time)
    {
        GameObject reloading = hud.reloading;
        float reloadTime = time;
        Image reloadingCircle = reloading.GetComponentInChildren<Image>();
        TextMeshProUGUI reloadTimeText = reloading.GetComponentInChildren<TextMeshProUGUI>();

        reloading.SetActive(true);

        while (time > 0)
        {
            float percent = time / reloadTime;
            reloadingCircle.fillAmount = percent;

            reloadTimeText.text = time.ToString("0.0", CultureInfo.InvariantCulture);

            time -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        reloading.SetActive(false);
    }
}