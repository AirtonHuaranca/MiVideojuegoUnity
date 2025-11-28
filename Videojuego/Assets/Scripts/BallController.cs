using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movimiento aleatorio")]
    public float baseSpeed = 3f;
    public float changeTargetInterval = 3f;
    public Vector2 fieldXLimits = new Vector2(-20f, 20f);
    public Vector2 fieldZLimits = new Vector2(-30f, 30f);

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float speedMultiplier = 1f;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        PickNewTarget();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeTargetInterval)
        {
            timer = 0f;
            PickNewTarget();
        }
    }

    private void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    private void PickNewTarget()
    {
        float x = Random.Range(fieldXLimits.x, fieldXLimits.y);
        float z = Random.Range(fieldZLimits.x, fieldZLimits.y);
        currentTarget = new Vector3(x, transform.position.y, z);
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = (currentTarget - transform.position).normalized;
        float speed = baseSpeed * speedMultiplier;
        Vector3 velocity = new Vector3(dir.x * speed, rb.velocity.y, dir.z * speed);
        rb.velocity = velocity;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}
