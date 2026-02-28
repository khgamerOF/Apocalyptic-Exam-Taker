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

    private int currentHealth;
    private bool isInvincible;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            transform.Translate(new Vector2(direction.x, 0) * speed * Time.deltaTime);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(transform.position);
            }
        }
    }
}
