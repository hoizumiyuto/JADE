using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("このアイテムで解除されるキー")]
    public string keyName = "J"; // インスペクターで "A" や "D" に書き換える

    // 触れた瞬間の処理（IsTriggerがONのとき）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーが触れたら
        if (collision.CompareTag("Player"))
        {
            // 相手（プレイヤー）のスクリプトを取得
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // ロック解除関数を呼び出す
                player.UnlockKey(keyName);
                
                // 自分自身は消滅する
                Destroy(gameObject);
            }
        }
    }
}