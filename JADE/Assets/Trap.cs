using UnityEngine;

public class Trap : MonoBehaviour
{
    // 即死させるか、ダメージだけか選べるようにする
    [SerializeField] private bool isInstantDeath = true;
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ぶつかったのがプレイヤーだったら
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                if (isInstantDeath)
                {
                    // HP以上のダメージを与えて即死させる（999ダメージ！）
                    player.TakeDamage(999);
                }
                else
                {
                    // 普通のダメージ
                    player.TakeDamage(damage);
                }
            }
        }
    }
}