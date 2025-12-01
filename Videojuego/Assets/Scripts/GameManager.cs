using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Balones")]
    public BallController ballPrefab;
    public int numberOfBalls = 10;

    [Header("Área del campo")]
    public BoxCollider fieldArea;

    [Header("Velocidad")]
    public float extraSpeedPerMissingBall = 0.3f;

    [Header("Escena siguiente")]
    public string nextSceneName = "Nivel2";

    [Header("UI Contador")]
    public GameObject ballCounterRoot;
    public TMP_Text ballCounterText;

    [Header("UI Paneles Tutorial")]
    public GameObject panelWelcome;
    public GameObject panelLinea1;
    public GameObject panelLinea2;

    private List<BallController> balls = new List<BallController>();
    private int kickedCount = 0;

    private float spawnY;
    private Bounds fieldBounds;

    private void Start()
    {
        if (fieldArea == null)
        {
            Debug.LogError("No se asignó fieldArea en el GameManager.");
            return;
        }

        fieldBounds = fieldArea.bounds;
        spawnY = fieldBounds.min.y + 0.5f;

        kickedCount = 0;

        SpawnBalls();
        UpdateBallsSpeed();
        UpdateBallCounter();

        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(false);

        if (panelWelcome != null)
        {
            panelWelcome.SetActive(true);
            PauseGame();
        }
    }

    private void SpawnBalls()
    {
        balls.Clear();

        for (int i = 0; i < numberOfBalls; i++)
        {
            float x = Random.Range(fieldBounds.min.x, fieldBounds.max.x);
            float z = Random.Range(fieldBounds.min.z, fieldBounds.max.z);

            Vector3 spawnPos = new Vector3(x, spawnY, z);
            BallController newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            newBall.Init(this, fieldArea);
            balls.Add(newBall);
        }
    }

    public void RegisterBallKicked(BallController ball)
    {
        if (!balls.Contains(ball))
            return;

        kickedCount++;
        balls.Remove(ball);

        UpdateBallsSpeed();
        UpdateBallCounter();

        if (kickedCount >= numberOfBalls)
            SceneManager.LoadScene(nextSceneName);
    }

    private void UpdateBallsSpeed()
    {
        float speedFactor = 1f + kickedCount * extraSpeedPerMissingBall;

        foreach (var b in balls)
        {
            if (b != null)
                b.SetSpeedMultiplier(speedFactor);
        }
    }

    private void UpdateBallCounter()
    {
        if (ballCounterText != null)
            ballCounterText.text = $"Balones: {kickedCount} / {numberOfBalls}";
    }

    public void ShowBallCounter()
    {
        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(true);

        UpdateBallCounter();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        ResumeGame();
    }

    public void ShowPanelLinea1()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(true);

        PauseGame();
    }

    public void OnLinea1Button()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(false);

        ResumeGame();
    }

    public void ShowPanelLinea2()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(true);

        ShowBallCounter();
        PauseGame();
    }

    public void OnLinea2Button()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(false);

        ResumeGame();
    }

    public int RemainingBalls => numberOfBalls - kickedCount;
}
