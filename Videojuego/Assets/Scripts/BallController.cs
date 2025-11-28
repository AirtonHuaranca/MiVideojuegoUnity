using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movimiento aleatorio")]
    public float baseSpeed = 3f;
    public float changeTargetInterval = 3f;

    [Header("Límites del campo (X y Z)")]
    public Vector2 fieldXLimits = new Vector2(-20f, 20f);
    public Vector2 fieldZLimits = new Vector2(-30f, 30f);

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timer;
    private float speedMultiplier = 1f;
    private bool isActive = true;        // si está en el campo o ya fue pateado

    private GameManager gameManager;     // referencia al GameManager

    // 👉 altura fija donde "vive" el balón mientras se mueve en el campo
    private float baseY = 0f;           

    // opcional: seguimos recibiendo minY por si luego quieres usarlo
    private float minY = 0f;             

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Mientras el balón está en el campo NO queremos que la gravedad lo hunda
        rb.useGravity = false;

        // Evitar vuelcos raros mientras se mueve
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        // Guardamos la altura inicial como referencia
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

    // ✅ Llamado por el GameManager al instanciar
    public void Init(GameManager manager, Vector2 xLimits, Vector2 zLimits, float minHeight)
    {
        gameManager = manager;
        fieldXLimits = xLimits;
        fieldZLimits = zLimits;
        minY = minHeight;   // por si lo necesitas luego, ahora usamos baseY
    }

    private void PickNewTarget()
    {
        float x = Random.Range(fieldXLimits.x, fieldXLimits.y);
        float z = Random.Range(fieldZLimits.x, fieldZLimits.y);
        currentTarget = new Vector3(x, baseY, z);
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = (currentTarget - transform.position).normalized;
        float speed = baseSpeed * speedMultiplier;

        // 👇 NO tocamos la Y del Rigidbody, la dejamos en 0 para que no vaya hacia abajo
        Vector3 velocity = new Vector3(dir.x * speed, 0f, dir.z * speed);
        rb.velocity = velocity;
    }

    private void ClampToField()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, fieldXLimits.x, fieldXLimits.y);
        pos.z = Mathf.Clamp(pos.z, fieldZLimits.x, fieldZLimits.y);

        // 🔥 Y SIEMPRE FIJA: no dejamos que se hunda ni suba
        pos.y = baseY;

        transform.position = pos;
    }

    // 👉 El GameManager utilizará esto para subir la velocidad a los que quedan
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // 🔥 Esta función se llama cuando el jugador patea el balón
    public void OnKicked(Vector3 kickDirection, float kickForce)
    {
        if (!isActive) return;
        isActive = false;

        // Ahora sí queremos física real
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        // deja de moverse por IA en X/Z
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // lo hacemos volar
        rb.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);

        // avisar al GameManager que este balón fue pateado
        if (gameManager != null)
        {
            gameManager.RegisterBallKicked(this);
        }

        // destruir después de 2 segundos (tiempo de vuelo)
        Destroy(gameObject, 2f);
    }
}
