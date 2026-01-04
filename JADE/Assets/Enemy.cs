using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f; // 敵の移動速度
    private Transform player; // プレイヤーの位置情報
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // シーンの中から "Player" という名前のオブジェクトを探して記憶する
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // プレイヤーが見つからなかったら（死んでたりしたら）何もしない
        if (player == null) return;

        // 1. 方向を決める（偏差 = 目標 - 現在地）
        // プレイヤーが右にいれば +、左にいれば - の値になる
        float xDifference = player.position.x - transform.position.x;

        // 2. 移動方向を -1 か 1 に正規化する（0に近いときは0にする）
        // Mathf.Sign は正なら1、負なら-1を返す数学関数です
        float direction = 0;
        if (Mathf.Abs(xDifference) > 0.1f) // 完全に重なってなければ動く
        {
            direction = Mathf.Sign(xDifference);
        }

        // 3. 速度を更新（Y軸は重力に任せるので今のまま）
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

// ぶつかった瞬間の判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ぶつかった相手が "Player" タグを持っていたら
        if (collision.gameObject.CompareTag("Player"))
        {
            // 相手のスクリプトを取得
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            
            if (player != null)
            {
                player.TakeDamage(1); // 1ダメージ与える
                
                // 敵自身も消滅する（自爆特攻）
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage()
    {
        // ▼▼▼ ここを追加 ▼▼▼
        // 「管理者（instance）」にアクセスして、AddScoreを実行してもらう
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore();
        }
        // ▲▲▲ ここまで ▲▲▲
        Debug.Log("やられたー！");
        Destroy(gameObject);
    }
}