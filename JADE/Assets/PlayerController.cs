using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("獲得したキー（能力）")]
    // ▼▼▼ ここが今回のミソ！初期値は全部 false (ロック状態) ▼▼▼
    public bool hasKeyA = false; // Attack
    public bool hasKeyJ = false; // Jump
    public bool hasKeyD = false; // Dash
    // ▲▲▲ ここまで ▲▲▲

    [Header("移動・ジャンプ")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("ダッシュ設定")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("攻撃設定")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("体力設定")]
    [SerializeField] private int maxHP = 3;
    private int currentHP;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;

    // 状態管理
    private bool isDashing = false;
    private bool canDash = true;
    private float lastDirection = 1f;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHP = maxHP;
        if (GameManager.instance != null) GameManager.instance.UpdateHP(currentHP);
    }

    void Update()
    {
        if (isDashing) return;

        // --- 1. 移動 (矢印キーは基本機能として最初から開放) ---
        float x = 0;
        if (Input.GetKey(KeyCode.RightArrow)) x = 1;
        if (Input.GetKey(KeyCode.LeftArrow))  x = -1;

        if (x != 0) lastDirection = x;
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // --- 2. ジャンプ (J) ---
        // 「地面にいる」かつ「Jキー所持」なら飛べる
        if (Input.GetKeyDown(KeyCode.J) && isGrounded)
        {
            if (hasKeyJ)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else
            {
                Debug.Log("Jキーがない！ジャンプできない！");
            }
        }

        // --- 3. ダッシュ (D) ---
        if (Input.GetKeyDown(KeyCode.D) && canDash)
        {
            if (hasKeyD)
            {
                StartCoroutine(Dash());
            }
            else
            {
                Debug.Log("Dキーがない！ダッシュできない！");
            }
        }

        // --- 4. 攻撃 (A) ---
        if (Input.GetKeyDown(KeyCode.A) && Time.time >= nextAttackTime)
        {
            if (hasKeyA)
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }
            else
            {
                Debug.Log("Aキーがない！攻撃できない！");
            }
        }
    }

    void Attack()
    {
        StartCoroutine(FlashRed());
        Vector2 attackPos = (Vector2)transform.position + (Vector2.right * lastDirection * 0.5f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (GameManager.instance != null) GameManager.instance.UpdateHP(currentHP);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (GameManager.instance != null) GameManager.instance.GameOver();
        Destroy(gameObject);
    }

    IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(lastDirection * dashSpeed, 0f);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }
    
    // 外部からキーを渡すための関数（後でアイテム取得時に使います）
    public void UnlockKey(string keyName)
    {
        if (keyName == "A") hasKeyA = true;
        if (keyName == "J") hasKeyJ = true;
        if (keyName == "D") hasKeyD = true;
        Debug.Log("キー [" + keyName + "] を取り戻した！");
    }

    void OnDrawGizmosSelected()
    {
        Vector2 attackPos = (Vector2)transform.position + (Vector2.right * lastDirection * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}