using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movimiento aleatorio")]
    public float baseSpeed = 3f;
    public float changeTargetInterval = 3f;

    [Header("Suavizado de movimiento")]
    public float directionSmooth = 3f;
    public float maxMoveSpeed = 10f;

    [Header("Área del campo")]
    [HideInInspector]
    public BoxCollider fieldArea;

    [Header("Desaparición")]
    public float disappearDelay = 2f;

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timer;
    private float speedMultiplier = 1f;

    private bool isActive = true;
    private bool hasBeenKicked = false;

    private GameManager gameManager;

    private float baseY;
    private const float edgeMargin = 0.5f;

    private Vector3 smoothDir = Vector3.forward;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        baseY = transform.position.y;
        smoothDir = transform.forward;
        PickNewTarget();
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        if (timer >= changeTargetInterval)
        {
            timer = 0f;
            PickNewTarget();
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        MoveTowardsTarget();
        ClampToField();
    }

    public void Init(GameManager manager, BoxCollider area)
    {
        gameManager = manager;
        fieldArea = area;
    }

    void PickNewTarget()
    {
        if (fieldArea == null) return;

        Bounds b = fieldArea.bounds;

        float x = Random.Range(b.min.x + edgeMargin, b.max.x - edgeMargin);
        float z = Random.Range(b.min.z + edgeMargin, b.max.z - edgeMargin);

        currentTarget = new Vector3(x, baseY, z);
    }

    void MoveTowardsTarget()
    {
        if (fieldArea == null) return;

        Vector3 toTarget = currentTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.25f)
        {
            PickNewTarget();
            toTarget = currentTarget - transform.position;
            toTarget.y = 0f;
        }

        Vector3 rawDir = toTarget.normalized;
        smoothDir = Vector3.Lerp(smoothDir, rawDir, Time.fixedDeltaTime * directionSmooth);

        float targetSpeed = Mathf.Min(baseSpeed * speedMultiplier, maxMoveSpeed);

        Vector3 desiredVelocity = new Vector3(smoothDir.x * targetSpeed, 0f, smoothDir.z * targetSpeed);

        Vector3 nextPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);

        if (desiredVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(desiredVelocity.x, 0f, desiredVelocity.z));
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f);
            rb.MoveRotation(newRot);
        }
    }

    void ClampToField()
    {
        if (fieldArea == null) return;

        Bounds b = fieldArea.bounds;
        Vector3 pos = rb.position;

        float minX = b.min.x + edgeMargin;
        float maxX = b.max.x - edgeMargin;
        float minZ = b.min.z + edgeMargin;
        float maxZ = b.max.z - edgeMargin;

        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);

        bool clamped = (!Mathf.Approximately(pos.x, clampedX) || !Mathf.Approximately(pos.z, clampedZ));

        pos.x = clampedX;
        pos.z = clampedZ;
        pos.y = baseY;

        rb.position = pos;

        if (clamped)
        {
            PickNewTarget();
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void OnKicked(Vector3 kickDirection, float kickForce)
    {
        if (hasBeenKicked) return;
        hasBeenKicked = true;

        isActive = false;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);

        TutorialManager tm = FindObjectOfType<TutorialManager>();
        if (tm != null)
        {
            tm.RegisterBallKicked();
        }

        Destroy(gameObject, disappearDelay);
    }
}
