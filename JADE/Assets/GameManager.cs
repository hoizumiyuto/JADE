using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // リトライ機能用

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UIパーツ")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;       // 追加
    public GameObject gameOverTextObj;   // 追加

    private int score = 0;
    private bool isGameOver = false;     // ゲームオーバー状態フラグ

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Update()
    {
        // ゲームオーバー時にRキーを押すとリトライ（シーン再読み込み）
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void AddScore()
    {
        if (isGameOver) return; // 死んでたらスコア増えない
        score++;
        scoreText.text = "Score: " + score;
    }

    // ▼▼▼ HP表示を更新する機能 ▼▼▼
    public void UpdateHP(int currentHP)
    {
        hpText.text = "HP: " + currentHP;
    }

    // ▼▼▼ ゲームオーバー処理 ▼▼▼
    public void GameOver()
    {
        isGameOver = true;
        gameOverTextObj.SetActive(true); // "GAME OVER"の文字を表示
        // Time.timeScale = 0f; // これを外すと時間を止められます（今回はスローモーション演出はなしで）
    }
}