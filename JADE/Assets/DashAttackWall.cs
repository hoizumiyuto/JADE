using UnityEngine;

public class DashAttackWall : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                // ▼▼▼ ここが重要！「ダッシュ中」かつ「攻撃中」なら破壊 ▼▼▼
                if (player.IsDashing() && player.IsAttacking())
                {
                    // 当たり判定を消してヌルっと抜ける（前回のテクニック）
                    GetComponent<Collider2D>().enabled = false;
                    Destroy(gameObject);
                    Debug.Log("完全破壊！");
                }
                else
                {
                    // 条件を満たしていない場合
                    // 「トゲの壁」なので、弾き飛ばしたりダメージを与えてもいいですね
                    Debug.Log("硬い！ダッシュ攻撃じゃないと壊れないぞ！");
                }
            }
        }
    }
}