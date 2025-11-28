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

            // le pasamos el GameManager y el área del campo
            newBall.Init(this, fieldArea);

            balls.Add(newBall);
        }
    }

    // 👇 Llamada desde BallController cuando se patea la pelota
    public void RegisterBallKicked(BallController ball)
    {
        // por seguridad, evitar doble conteo
        if (!balls.Contains(ball))
            return;

        kickedCount++;
        balls.Remove(ball);

        UpdateBallsSpeed();

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

    // (opcional) por si quieres saber cuántos faltan
    public int RemainingBalls => numberOfBalls - kickedCount;
}
