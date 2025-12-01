using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Balones")]
    public BallController ballPrefab;
    public int numberOfBalls = 10;

    [Header("Área del campo (BoxCollider)")]
    public BoxCollider fieldArea;   // arrastra aquí CampoArea

    [Header("Velocidad")]
    public float extraSpeedPerMissingBall = 0.3f;

    [Header("Escena siguiente")]
    public string nextSceneName = "Nivel2";

    [Header("UI Contador de Balones")]
    public GameObject ballCounterRoot;   // panel / objeto que contiene el texto
    public TMP_Text ballCounterText;     // texto "Balones: X / Y"

    [Header("UI Paneles Tutorial")]
    public GameObject panelWelcome;      // Panel de bienvenida al iniciar el juego
    public GameObject panelLinea1;       // Panel al cruzar la primera línea
    public GameObject panelLinea2;       // Panel al cruzar la segunda línea

    private List<BallController> balls = new List<BallController>();
    private int kickedCount = 0;

    private float spawnY;
    private Bounds fieldBounds;

    private void Start()
    {
        if (fieldArea == null)
        {
            Debug.LogError("No se asignó el fieldArea (BoxCollider) en el GameManager.");
            return;
        }

        // Guardamos los bounds del área una sola vez
        fieldBounds = fieldArea.bounds;

        // altura un poco encima del piso del área
        spawnY = fieldBounds.min.y + 0.5f;

        kickedCount = 0;

        SpawnBalls();
        UpdateBallsSpeed();
        UpdateBallCounter();   // inicializamos el texto del contador

        // El contador empieza oculto hasta el panel 2
        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(false);

        // Mostrar panel de bienvenida y pausar el juego
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

            // le pasamos el GameManager y el área del campo
            newBall.Init(this, fieldArea);

            balls.Add(newBall);
        }
    }

    // Llamada desde BallController cuando se patea la pelota
    public void RegisterBallKicked(BallController ball)
    {
        // por seguridad, evitar doble conteo
        if (!balls.Contains(ball))
            return;

        kickedCount++;
        balls.Remove(ball);

        UpdateBallsSpeed();
        UpdateBallCounter();

        Debug.Log($"Balones pateados: {kickedCount}/{numberOfBalls}");

        // cuando se patean todos los balones → siguiente escena
        if (kickedCount >= numberOfBalls)
        {
            SceneManager.LoadScene(nextSceneName);
        }
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

    // ==== CONTADOR DE BALONES ====

    private void UpdateBallCounter()
    {
        if (ballCounterText != null)
        {
            ballCounterText.text = $"Balones: {kickedCount} / {numberOfBalls}";
        }
    }

    public void ShowBallCounter()
    {
        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(true);

        UpdateBallCounter();
    }

    // ==== PAUSA / REANUDAR ====

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // ==== PANELES DEL TUTORIAL ====

    // botón del panel de bienvenida
    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        ResumeGame();
    }

    // llamado desde Linea1Trigger
    public void ShowPanelLinea1()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(true);

        PauseGame();
    }

    // botón dentro del panel linea 1
    public void OnLinea1Button()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(false);

        ResumeGame();
    }

    // llamado desde Linea2Trigger
    public void ShowPanelLinea2()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(true);

        // al mismo tiempo mostramos el contador
        ShowBallCounter();

        PauseGame();
    }

    // botón dentro del panel linea 2
    public void OnLinea2Button()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(false);

        ResumeGame();
    }

    // (opcional) por si quieres saber cuántos faltan
    public int RemainingBalls => numberOfBalls - kickedCount;
}
