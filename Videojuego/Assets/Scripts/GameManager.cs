using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Balones")]
    public int ballsToGoal = 10;
    public float extraSpeedPerMissingBall = 0.5f;

    [Header("UI")]
    public Text ballsText;

    private int totalBalls = 0;
    private int ballsKicked = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void RegisterBall(Ball ball)
    {
        totalBalls++;
        UpdateUI();
    }

    public void BallKicked(Ball ball)
    {
        ballsKicked++;
        UpdateUI();

        if (ballsKicked >= ballsToGoal)
        {
            NextLevel();
        }
    }

    void UpdateUI()
    {
        if (ballsText != null)
        {
            ballsText.text = "Balones: " + ballsKicked + " / " + ballsToGoal;
        }
    }

    public float GetCurrentBallSpeed(float baseSpeed)
    {
        // Mientras menos balones queden, más rápido se mueven
        int ballsRemaining = Mathf.Max(totalBalls - ballsKicked, 0);
        int missing = totalBalls - ballsRemaining; // los que ya pateaste

        float extra = missing * extraSpeedPerMissingBall;
        return baseSpeed + extra;
    }

    void NextLevel()
    {
        // Cambia "NextScene" por el nombre de tu siguiente escena
        SceneManager.LoadScene("NextScene");
    }
}
