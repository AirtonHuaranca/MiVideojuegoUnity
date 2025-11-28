using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Balones")]
    public int totalBalls = 10;        // cantidad total de balones en el campo
    [HideInInspector] public int ballsRemaining;
    [HideInInspector] public int ballsKicked = 0;

    [Header("Dificultad")]
    [Tooltip("Qué tanto aumenta la velocidad cuando quedan pocos balones")]
    public float difficultyFactor = 2f;

    private void Awake()
    {
        // Patrón Singleton básico
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ballsRemaining = totalBalls;
    }

    // Llamado por cada balón cuando es pateado
    public void OnBallKicked()
    {
        ballsKicked++;
        ballsRemaining = Mathf.Max(0, ballsRemaining - 1);

        Debug.Log($"Balones pateados: {ballsKicked}, Balones restantes: {ballsRemaining}");

        // Actualizar la velocidad de los balones
        UpdateBallsSpeed();

        // Si ya pateó 10 balones → pasar al siguiente nivel
        if (ballsKicked >= 10)
        {
            LoadNextLevel();
        }
    }

    private void UpdateBallsSpeed()
    {
        if (totalBalls <= 0) return;

        // ratio = 1 cuando están todos, 0 cuando no queda ninguno
        float ratio = (float)ballsRemaining / totalBalls;

        // Mientras menos balones → mayor multiplicador
        float speedMultiplier = 1f + difficultyFactor * (1f - ratio);

        BallController[] balls = FindObjectsOfType<BallController>();
        foreach (var ball in balls)
        {
            ball.SetSpeedMultiplier(speedMultiplier);
        }
    }

    private void LoadNextLevel()
    {
        Debug.Log("¡Completado! Cargando Nivel2...");
        // Asegúrate que la escena "Nivel2" exista en Build Settings
        SceneManager.LoadScene("Nivel2");
    }
}
