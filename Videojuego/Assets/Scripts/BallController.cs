using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movimiento aleatorio")]
    public float baseSpeed = 3f;
    public float changeTargetInterval = 3f;

    [Header("Suavizado de movimiento")]
    public float directionSmooth = 3f;      // qué tan suave gira hacia el nuevo destino
    public float maxMoveSpeed = 10f;        // velocidad máxima para que no se vuelva loco

    [Header("Área del campo")]
    [HideInInspector]
    public BoxCollider fieldArea;           // CampoArea

    [Header("Desaparición")]
    public float disappearDelay = 2f;       // tiempo antes de destruir el balón tras la patada

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timer;
    private float speedMultiplier = 1f;

    private bool isActive = true;           // se mueve en el campo
    private bool hasBeenKicked = false;     // ya fue pateado (para no contar 2 veces)

    private GameManager gameManager;

    private float baseY;                    // altura fija mientras está en el campo
    private const float edgeMargin = 0.5f;  // un poco más grande para alejarse del borde

    // suavizado de dirección
    private Vector3 smoothDir = Vector3.forward;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Mientras está en el campo: sin gravedad, solo desliza en X/Z
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Mejor interpolación para movimiento suave
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        baseY = transform.position.y;
        smoothDir = transform.forward;
        PickNewTarget();
    }

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        if (timer >= changeTargetInterval)
        {
            timer = 0f;
            PickNewTarget();
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        MoveTowardsTarget();
        ClampToField();
    }

    // Esto lo llama el GameManager al crear el balón
    public void Init(GameManager manager, BoxCollider area)
    {
        gameManager = manager;
        fieldArea = area;
    }

    private void PickNewTarget()
    {
        if (fieldArea == null) return;

        Bounds b = fieldArea.bounds;

        float x = Random.Range(b.min.x + edgeMargin, b.max.x - edgeMargin);
        float z = Random.Range(b.min.z + edgeMargin, b.max.z - edgeMargin);

        currentTarget = new Vector3(x, baseY, z);
    }

    private void MoveTowardsTarget()
    {
        if (fieldArea == null) return;

        // dirección hacia el target, solo en X/Z
        Vector3 toTarget = currentTarget - transform.position;
        toTarget.y = 0f;

        // si ya está cerca del objetivo, buscar otro
        if (toTarget.sqrMagnitude < 0.5f * 0.5f)
        {
            PickNewTarget();
            toTarget = currentTarget - transform.position;
            toTarget.y = 0f;
        }

        Vector3 rawDir = toTarget.normalized;

        // suavizar el cambio de dirección para evitar tirones
        smoothDir = Vector3.Lerp(smoothDir, rawDir, Time.fixedDeltaTime * directionSmooth);

        float targetSpeed = baseSpeed * speedMultiplier;

        // limitar velocidad máxima para evitar tambaleo extremo
        targetSpeed = Mathf.Min(targetSpeed, maxMoveSpeed);

        Vector3 desiredVelocity = new Vector3(smoothDir.x * targetSpeed, 0f, smoothDir.z * targetSpeed);

        // mover con MovePosition para más estabilidad
        Vector3 nextPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);

        // rotación suave hacia la dirección de movimiento
        if (desiredVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(desiredVelocity.x, 0f, desiredVelocity.z));
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f);
            rb.MoveRotation(newRot);
        }
    }

    private void ClampToField()
    {
        if (fieldArea == null) return;

        Bounds b = fieldArea.bounds;
        Vector3 pos = rb.position;

        bool clamped = false;

        float minX = b.min.x + edgeMargin;
        float maxX = b.max.x - edgeMargin;
        float minZ = b.min.z + edgeMargin;
        float maxZ = b.max.z - edgeMargin;

        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);

        if (!Mathf.Approximately(pos.x, clampedX) || !Mathf.Approximately(pos.z, clampedZ))
        {
            clamped = true;
        }

        pos.x = clampedX;
        pos.z = clampedZ;
        pos.y = baseY;

        rb.position = pos;

        // si tocó borde, buscar un nuevo objetivo hacia el centro para evitar vibración ahí
        if (clamped)
        {
            PickNewTarget();
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // Cuando el jugador patea el balón
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

        if (gameManager != null)
        {
            gameManager.RegisterBallKicked(this);
        }

        Destroy(gameObject, disappearDelay);
    }
}
