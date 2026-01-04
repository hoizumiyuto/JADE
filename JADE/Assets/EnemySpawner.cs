using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("設定")]
    public GameObject enemyPrefab; // 生産する「設計図」
    public float interval = 3.0f;  // 生産間隔（秒）

    void Start()
    {
        // ループ処理を開始
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        // 無限ループ（while(true)はゲーム開発ではよく使います）
        while (true)
        {
            // 1. 敵を生成（Instantiate = 実体化）
            // Instantiate(何を, どこに, どんな向きで)
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            // 2. 指定した時間待つ
            yield return new WaitForSeconds(interval);
        }
    }
}