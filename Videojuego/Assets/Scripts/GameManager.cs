using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Balones")]
    public BallController ballPrefab;
    public int numberOfBalls = 10;

    [Header("Área del campo (BoxCollider)")]
    public BoxCollider fieldArea;   // 👉 arrastra aquí el objeto CampoArea

    [Header("Velocidad")]
    public float extraSpeedPerMissingBall = 0.3f;

    [Header("Escena siguiente")]
    public string nextSceneName = "Nivel2";

    private List<BallController> balls = new List<BallController>();
    private int kickedCount = 0;

    // límites calculados automáticamente
    private Vector2 fieldXLimits;
    private Vector2 fieldZLimits;
    private float spawnY;
    private float fieldMinY;   // piso real del área

    private void Start()
    {
        if (fieldArea == null)
        {
            Debug.LogError("No se asignó el fieldArea (BoxCollider) en el GameManager.");
            return;
        }

        // 1️⃣ Calcular límites a partir del BoxCollider
        Bounds b = fieldArea.bounds;
        fieldXLimits = new Vector2(b.min.x, b.max.x);
        fieldZLimits = new Vector2(b.min.z, b.max.z);

        // piso del área
        fieldMinY = b.min.y;

        // 👉 altura donde aparecerán los balones (un poco encima del piso)
        spawnY = fieldMinY + 0.5f;

        // 2️⃣ Crear los balones dentro de esa área
        SpawnBalls();
        UpdateBallsSpeed();
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            float x = Random.Range(fieldXLimits.x, fieldXLimits.y);
            float z = Random.Range(fieldZLimits.x, fieldZLimits.y);

            Vector3 spawnPos = new Vector3(x, spawnY, z);

            BallController newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            // ⬇️ ahora le pasamos también la altura mínima del campo
            newBall.Init(this, fieldXLimits, fieldZLimits, fieldMinY);

            balls.Add(newBall);
        }
    }

    public void RegisterBallKicked(BallController ball)
    {
        kickedCount++;
        balls.Remove(ball);

        UpdateBallsSpeed();

        Debug.Log($"Balones pateados: {kickedCount}/{numberOfBalls}");

        if (kickedCount >= 10)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void UpdateBallsSpeed()
    {
        int pateados = kickedCount;
        float speedFactor = 1f + pateados * extraSpeedPerMissingBall;

        foreach (var b in balls)
        {
            if (b != null)
                b.SetSpeedMultiplier(speedFactor);
        }
    }
}
