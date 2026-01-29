using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("獲得したキー（能力）")]
    // ▼▼▼ ここが今回のミソ！初期値は全部 false (ロック状態) ▼▼▼
    public bool hasKeyA = false; // Attack
    public bool hasKeyJ = false; // Jump
    public bool hasKeyD = false; // Dash
    private bool isAttacking = false; // ▼▼▼ 追加：攻撃中かどうかを管理するフラグ
    public bool IsDashing()
    {
        return isDashing;
    }
    // ▲▲▲ ここまで ▲▲▲
    // ▼▼▼ 追加：壁が「攻撃中？」と聞けるようにする
    public bool IsAttacking()
    {
        return isAttacking;
    }

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
        // ▼▼▼ 1. 攻撃 (A) を一番上に移動！ ▼▼▼
        // これでダッシュ中でも攻撃ボタンが反応するようになります
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

        // ▼▼▼ 2. ここでダッシュ中のチェックを入れる ▼▼▼
        // ダッシュ中なら、ここから下（移動やジャンプ）は実行させない
        if (isDashing) return;

        // --- 以下、ダッシュしていない時だけできること ---

        // 3. 移動 (矢印キー)
        float x = 0;
        if (Input.GetKey(KeyCode.RightArrow)) x = 1;
        if (Input.GetKey(KeyCode.LeftArrow))  x = -1;

        if (x != 0) lastDirection = x;
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // 4. ジャンプ (J)
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
    // ▼▼▼ Attack関数をこれに書き換え ▼▼▼
    void Attack()
    {
        // コルーチン（時間差処理）を開始する
        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + attackRate;
    }

    // ▼▼▼ 新しく追加する関数（攻撃の実体） ▼▼▼
    IEnumerator PerformAttack()
    {
        isAttacking = true; // ▼▼▼ 追加：攻撃開始！フラグを立てる
        StartCoroutine(FlashRed());

        // 「攻撃ボタンを押した瞬間にダッシュしていたか？」を記憶
        bool isDashAttack = isDashing; 
        
        float timer = 0f;
        float normalAttackDuration = 0.1f; // 通常攻撃の持続時間（一瞬）

        // 無限ループ（中で break して抜ける）
        while (true) 
        {
            // ▼▼▼ 終了条件のチェック ▼▼▼
            if (isDashAttack)
            {
                // ダッシュ攻撃の場合：ダッシュが終わったらループ終了
                if (!isDashing) break;
            }
            else
            {
                // 通常攻撃の場合：一定時間経ったらループ終了
                if (timer > normalAttackDuration) break;
            }
            // ▲▲▲ ▲▲▲


            // --- 攻撃判定（いつものやつ） ---
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
            // -----------------------------

            timer += Time.deltaTime;
            yield return null;
        }
        isAttacking = false; // ▼▼▼ 追加：攻撃終了！フラグを降ろす
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

        // ▼▼▼ ここから変更（whileループにする） ▼▼▼
        float startTime = Time.time; // 開始時間を記録

        // 「ダッシュ時間が経過するまで」ずっとループして速度を強制し続ける
        while (Time.time < startTime + dashTime)
        {
            // 毎フレーム「この速度で進め！」と命令し続ける
            // これにより、壁にぶつかって物理演算で止まりそうになっても、無理やり進みます
            rb.linearVelocity = new Vector2(lastDirection * dashSpeed, 0f);

            yield return null; // 1フレーム待つ
        }
        // ▲▲▲ ここまで変更 ▲▲▲

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; // ダッシュ後はピタッと止まる
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