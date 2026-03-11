using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] GameObject player;
    [SerializeField] float aggroRange;
    [SerializeField] float speed;

    [Header("Combat")]
    [SerializeField] int maxHealth = 2;
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float invincibilityDuration = 1f;

    private Animator animator;

    private int currentHealth;
    private bool isInvincible;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [SerializeField] float damageCooldown = 1f;
    float lastDamageTime;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance < aggroRange)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;

            transform.Translate(new Vector2(direction.x, 0) * speed * Time.deltaTime);

            animator.SetFloat("Speed", Mathf.Abs(direction.x));

            Flip(direction.x);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        // Knockback
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
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();

                if (player != null)
                {
                    player.TakeDamage(transform.position);
                    lastDamageTime = Time.time;
                }
            }
        }
    }

    void Flip(float directionX)
    {
        if (directionX > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (directionX < 0)
        {
            spriteRenderer.flipX = false;
        }
    }
}
