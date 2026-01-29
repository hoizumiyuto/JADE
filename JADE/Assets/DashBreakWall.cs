using UnityEngine;

public class DashBreakWall : MonoBehaviour
{
    // ぶつかった瞬間の処理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ぶつかったのが「プレイヤー」か確認
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. プレイヤーのスクリプトを取得
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // 3. プレイヤーが存在して、かつ「ダッシュ中」なら
            if (player != null && player.IsDashing())
            {
                // ▼▼▼ これを追加（当たり判定を即座に消す） ▼▼▼
                GetComponent<Collider2D>().enabled = false;
                // 壁を破壊！
                Destroy(gameObject);
                
                // ※ここに「ドカーン！」という音やエフェクトを入れると気持ちいいです
                Debug.Log("ダッシュで壁を破壊した！");
            }
        }
    }
}