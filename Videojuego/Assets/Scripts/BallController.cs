using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movimiento aleatorio")]
    public float baseSpeed = 3f;
    public float changeTargetInterval = 3f;

    [Header("Área del campo")]
    [HideInInspector]              // lo asigna el GameManager por código
    public BoxCollider fieldArea;   // CampoArea

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timer;
    private float speedMultiplier = 1f;
    private bool isActive = true;

    private GameManager gameManager;

    private float baseY;            // altura fija mientras está en el campo
    private const float edgeMargin = 0.2f;  // margen para que no llegue justo al borde

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Mientras está en el campo: sin gravedad, solo desliza en X/Z
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        // Guardamos la altura inicial donde fue instanciado
        baseY = transform.position.y;
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

    // ✅ Esto lo llama el GameManager al crear el balón
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

        Vector3 dir = (currentTarget - transform.position).normalized;
        float speed = baseSpeed * speedMultiplier;

        // solo movemos en X/Z, Y = 0 para que no se hunda
        Vector3 velocity = new Vector3(dir.x * speed, 0f, dir.z * speed);
        rb.velocity = velocity;
    }

    private void ClampToField()
    {
        if (fieldArea == null) return;

        Bounds b = fieldArea.bounds;
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, b.min.x + edgeMargin, b.max.x - edgeMargin);
        pos.z = Mathf.Clamp(pos.z, b.min.z + edgeMargin, b.max.z - edgeMargin);

        // Y fija, no dejamos que se hunda ni suba
        pos.y = baseY;

        transform.position = pos;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // 🔥 Cuando el jugador patea el balón
    public void OnKicked(Vector3 kickDirection, float kickForce)
    {
        if (!isActive) return;
        isActive = false;

        // Ahora sí queremos física real
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);

        if (gameManager != null)
        {
            gameManager.RegisterBallKicked(this);
        }

        Destroy(gameObject, 2f);
    }
}
