using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component Refrences")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float flashInterval = 0.1f;
    [SerializeField] Animator animator;

    [Header("Player Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpingPower = 8f;
    [SerializeField] float sprintMultiplier = 1.5f;
    private bool isSprinting;
    [SerializeField] float doubleJumpMultiplier = 0.5f;
    private bool canDoubleJump;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;

    [Header("Combat")]
    [SerializeField] int maxHealth = 3;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] float invincibilityDuration = 3f;
    private int currentHealth;
    private bool isInvincible;

    private float horizontal;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverText;
    [SerializeField] float gameOverDelay = 3f;

    [Header("Attack")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] int attackDamage = 1;
    [SerializeField] float attackCooldown = 0.3f;
    [SerializeField] GameObject slashPrefab;

    [Header("HUD")]
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] GameObject healthUI;

    private bool canAttack = true;

    private bool isFacingRight = true;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

    }

    private void FixedUpdate()
    {
        float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
        if (isGrounded())
        {
            canDoubleJump = true;
        }
    }

    private void Update()
    {
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsGrounded", isGrounded());
        animator.SetBool("IsSprinting", isSprinting);
    }

    #region PLAYER_CONTROLS
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;

        if (horizontal > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontal < 0 && isFacingRight)
        {
            Flip();
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // Ground jump
        if (isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            canDoubleJump = true;
        }
        // Double jump
        else if (canDoubleJump)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpingPower * doubleJumpMultiplier
            );
            canDoubleJump = false;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1f, 0.1f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started && canAttack)
        {
            Debug.Log("ATTACK BUTTON PRESSED");
            animator.SetTrigger("Attack");
            // Spawn slash
            GameObject slash = Instantiate(
                slashPrefab,
                attackPoint.position,
                Quaternion.identity
            );

            // Flip slash if facing left
            if (!isFacingRight)
            {
                Vector3 scale = slash.transform.localScale;
                scale.x *= -1;
                slash.transform.localScale = scale;
            }

            StartCoroutine(AttackCoroutine());
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        spriteRenderer.flipX = !spriteRenderer.flipX;

        Vector3 attackPos = attackPoint.localPosition;
        attackPos.x *= -1;
        attackPoint.localPosition = attackPos;
    }
    #endregion

    public void TakeDamage(Vector2 damageSourcePosition)
    {
        if (isInvincible) return;

        currentHealth--;
        UpdateHealthUI();

        Debug.Log("Player Hit! Health: " + currentHealth);

        // Apply Knockback
        Vector2 knockbackDirection = new Vector2(
        transform.position.x > damageSourcePosition.x ? 1 : -1,
            1f
        ).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        healthUI.SetActive(false);
        Debug.Log("Player Died!");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // stop physics

        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        // Show Game Over text
        gameOverText.SetActive(true);

        // Wait 3 seconds
        yield return new WaitForSeconds(gameOverDelay);

        // Load Main Menu scene
        SceneManager.LoadScene("main menu");
    }

    IEnumerator AttackCoroutine()
    {
        canAttack = false;

        // Detect enemies in front
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(
                attackDamage,
                transform.position
            );
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void UpdateHealthUI()
    {
        healthText.text = currentHealth.ToString();
    }
}
