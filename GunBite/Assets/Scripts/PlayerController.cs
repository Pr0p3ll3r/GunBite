﻿using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;

    public Camera cam;
    public Transform weaponParent;
    public float stepRate;
    public AudioSource footstepSource;

    public float dashDistance = 10f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 5f;
    private bool isDashing;
    public bool canDash = true;

    private Vector2 movement;
    private Vector2 lastMove;
    private Vector3 mousePos;
    private float stepCoolDown;

    private WeaponManager wm;
    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private PlayerHUD hud;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        wm = GetComponent<WeaponManager>();
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hud = GetComponent<PlayerHUD>();
    }

    private void Update()
    {
        if (Pause.paused || player.isDead) return;

        //input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        if (movement != Vector2.zero)
            lastMove = movement;

        //dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(Dash(lastMove));   
            
        //animations
        if(movement != Vector2.zero)
            animator.SetBool("Move", true);     
        else
            animator.SetBool("Move", false);

        //player flip
        if (movement.x < 0)
            spriteRenderer.flipX = true;
        else if (movement.x > 0)
            spriteRenderer.flipX = false;

        //weapon look at mouse position
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePos - weaponParent.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        weaponParent.eulerAngles = new Vector3(0, 0, angle);

        //weapon flip
        if (angle >= -90 && angle <= 90)
            weaponParent.rotation *= Quaternion.Euler(0, 0, 0);
        else
            weaponParent.rotation *= Quaternion.Euler(180, 0, 0);

        //footsteps
        stepCoolDown -= Time.deltaTime;

        if (movement != Vector2.zero && stepCoolDown < 0f)
        {
            PlayFootStepAudio();
            stepCoolDown = stepRate;
        }
    }

    private void FixedUpdate()
    {
        float speed;
        speed = moveSpeed;
        if(wm.currentWeaponData != null) speed += wm.currentWeaponData.movementSpeed;
        if(!isDashing)
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void PlayFootStepAudio()
    {
        footstepSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        footstepSource.Play();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            collision.rigidbody.velocity = Vector3.zero;
            collision.otherRigidbody.velocity = Vector3.zero;
        }   
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((isDashing || player.invincible) && collision.gameObject.tag == "Enemy")
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, false);
        }
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        player.invincible = true;
        SoundManager.Instance.Play("Dash");
        rb.AddForce(direction * dashDistance, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashDuration);
        hud.StartCoroutine(hud.StaminaRestore(dashCooldown));
        isDashing = false;
        player.invincible = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}