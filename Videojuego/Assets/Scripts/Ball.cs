using UnityEngine;

public class Ball : MonoBehaviour
{
    public float baseSpeed = 2f;
    public float changeDirTime = 2f;

    public GameObject explosionPrefab;

    private Rigidbody rb;
    private float timer;
    private Vector3 moveDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameManager.Instance.RegisterBall(this);

        PickNewDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PickNewDirection();
        }
    }

    void FixedUpdate()
    {
        float speed = GameManager.Instance.GetCurrentBallSpeed(baseSpeed);
        Vector3 vel = moveDir * speed;
        vel.y = rb.velocity.y; // mantiene gravedad
        rb.velocity = vel;
    }

    void PickNewDirection()
    {
        // Dirección aleatoria en plano XZ
        moveDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        timer = changeDirTime;
    }

    public void Kick(Vector3 dir, float force)
    {
        dir.y = 0.3f; // un poco hacia arriba
        dir.Normalize();

        rb.velocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        // Explosión y sumar al contador
        Explode();
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        GameManager.Instance.BallKicked(this);
        Destroy(gameObject);
    }
}
