using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("獲得したキー（能力）")]
    public bool hasKeyA = false; // Attack
    public bool hasKeyJ = false; // Jet (旧 Jump から変更！)
    public bool hasKeyD = false; // Dash
    
    private bool isAttacking = false;
    
    // 外部から状態を確認するための関数
    public bool IsDashing() { return isDashing; }
    public bool IsAttacking() { return isAttacking; }

    [Header("移動・ジャンプ")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("ジェット（J）設定")]
    [SerializeField] private float jetUpSpeed = 5f; // 上昇力
    // ▼▼▼ 追加：燃料の設定 ▼▼▼
    [SerializeField] private float maxJetDuration = 1.0f; // 何秒飛べるか
    private float currentJetFuel; // 今の残り燃料
    // ▲▲▲ ▲▲▲

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
        // ▼▼▼ 追加：最初は燃料満タン ▼▼▼
        currentJetFuel = maxJetDuration;
    }

    void Update()
    {
        // 1. 攻撃 (A)
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

        // ダッシュ中は移動・ジャンプ・ジェット操作を受け付けない
        if (isDashing) return;

        // 2. 移動 (矢印キー)
        float x = 0;
        if (Input.GetKey(KeyCode.RightArrow)) x = 1;
        if (Input.GetKey(KeyCode.LeftArrow))  x = -1;

        if (x != 0) lastDirection = x;
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // 3. ジャンプ (Space または ↑キー)
        // ※これは基本アクションとして、キー制限なしにしています（必要なら制限を追加可能）
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

       // ▼▼▼ 4. ジェット (J) 燃料制限付き ▼▼▼
        // Jキーを押していて、かつ「燃料が残っている」なら飛べる
        if (Input.GetKey(KeyCode.J) && currentJetFuel > 0)
        {
            if (hasKeyJ)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jetUpSpeed);
                
                // 燃料（残り時間）を減らす
                currentJetFuel -= Time.deltaTime; 
            }
        }

        // 地面にいるときは、燃料を急速チャージ（満タンに戻す）
        if (isGrounded)
        {
            currentJetFuel = maxJetDuration;
        }
        // ▲▲▲ ▲▲▲

        // 5. ダッシュ (D)
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
    }

    // --- 以下、変更なし ---

    void Attack()
    {
        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + attackRate;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        StartCoroutine(FlashRed());

        bool isDashAttack = isDashing; 
        
        float timer = 0f;
        float normalAttackDuration = 0.1f;

        while (true) 
        {
            if (isDashAttack)
            {
                if (!isDashing) break;
            }
            else
            {
                if (timer > normalAttackDuration) break;
            }

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

            timer += Time.deltaTime;
            yield return null;
        }
        isAttacking = false;
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

        float startTime = Time.time; 

        while (Time.time < startTime + dashTime)
        {
            rb.linearVelocity = new Vector2(lastDirection * dashSpeed, 0f);
            yield return null; 
        }

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
    
    public void UnlockKey(string keyName)
    {
        if (keyName == "A") hasKeyA = true;
        if (keyName == "J") hasKeyJ = true; // Jet 解放
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