using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        SpawnBalls();
        UpdateBallsSpeed();
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            float x = Random.Range(fieldBounds.min.x, fieldBounds.max.x);
            float z = Random.Range(fieldBounds.min.z, fieldBounds.max.z);

            Vector3 spawnPos = new Vector3(x, spawnY, z);

            BallController newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            // 👇 le pasamos el GameManager y el área del campo
            newBall.Init(this, fieldArea);

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
