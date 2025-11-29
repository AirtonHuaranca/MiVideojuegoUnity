using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;          // Adelante / atrás
    public float rotationSpeed = 220f;    // Grados por segundo

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Patear")]
    public Transform kickPoint;      // Empty en el pie
    public float kickRadius = 1.5f;  // radio de detección (un poco más grande)
    public float kickForce = 20f;    // fuerza de la patada
    public LayerMask ballLayer;      // capa de los balones

    // ⏱ Ventana de tiempo para detectar el balón
    public float kickDetectionWindow = 1f;

    [Header("Animación")]
    public Animator animator;

    private Rigidbody rb;
    private float inputV;  // W / S
    private float inputH;  // A / D
    private bool isGrounded;

    // control de la ventana de patada
    private bool isKickingWindow = false;
    private float kickTimer = 0f;

    // 👇 NUEVO: bloqueo de movimiento mientras patea
    public float kickLockTime = 0.6f;   // ponlo parecido a la duración de tu animación
    private bool isKicking = false;
    private float kickLockTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Para que la física no lo tumbe
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // 1) INPUTS
        inputV = Input.GetAxisRaw("Vertical");   // W (1) / S (-1)
        inputH = Input.GetAxisRaw("Horizontal"); // A (-1) / D (1)

        // 2) CHEQUEAR SUELO
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
        else
        {
            isGrounded = true;
        }

        // Animaciones
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);

            // BLEND TREE: MoveZ (adelante/atrás)
            float targetMoveZ = isKicking ? 0f : inputV;   // 👈 quieto mientras patea
            float currentMoveZ = animator.GetFloat("MoveZ");
            float smoothMoveZ = Mathf.Lerp(currentMoveZ, targetMoveZ, Time.deltaTime * 10f);
            animator.SetFloat("MoveZ", smoothMoveZ);

            // BLEND TREE: Turn (giro en el sitio)
            float targetTurn = isKicking ? 0f : inputH;    // 👈 opcional: sin giro mientras patea
            float currentTurn = animator.GetFloat("Turn");
            float smoothTurn = Mathf.Lerp(currentTurn, targetTurn, Time.deltaTime * 10f);
            animator.SetFloat("Turn", smoothTurn);
        }

        // 3) SALTO (Space)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isKicking)
        {
            Jump();
        }

        // 4) PATEAR (K)
        if (Input.GetKeyDown(KeyCode.K) && !isKicking)
        {
            StartKick();   // arranca animación + ventana de detección
        }

        // 5) Mientras la ventana de patada está activa, seguimos buscando balones
        if (isKickingWindow)
        {
            kickTimer += Time.deltaTime;

            // Intentar detectar y patear un balón
            bool hitSomething = TryKickHit();

            // Si ya pateamos algo o se acabó el tiempo, cerramos la ventana
            if (hitSomething || kickTimer >= kickDetectionWindow)
            {
                isKickingWindow = false;
            }
        }

        // 6) Contar el tiempo que dura bloqueado por la patada
        if (isKicking)
        {
            kickLockTimer += Time.deltaTime;
            if (kickLockTimer >= kickLockTime)
            {
                isKicking = false;          // 👈 aquí vuelve a poder moverse
                // Debug.Log("Fin de bloqueo de patada, puede moverse");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isKicking)
        {
            // 👇 mientras está pateando, no avanzamos ni giramos
            Vector3 vel = rb.velocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;
        }
        else
        {
            // MOVIMIENTO ADELANTE / ATRÁS
            Vector3 moveDir = transform.forward * inputV * moveSpeed;
            Vector3 velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
            rb.velocity = velocity;

            // ROTACIÓN EN EL SITIO (A / D)
            if (Mathf.Abs(inputH) > 0.01f)
            {
                float rotationAmount = inputH * rotationSpeed * Time.fixedDeltaTime;
                transform.Rotate(0f, rotationAmount, 0f);
            }
        }
    }

    private void Jump()
    {
        // Limpia velocidad vertical antes de saltar
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    // 👉 Se llama cuando presionas K
    private void StartKick()
    {
        if (animator != null)
        {
            animator.SetTrigger("Kick");
        }

        // activar ventana de detección
        isKickingWindow = true;
        kickTimer = 0f;

        // 👇 bloquear movimiento mientras dure la patada
        isKicking = true;
        kickLockTimer = 0f;

        // limpiar velocidad horizontal para que no se deslice
        Vector3 vel = rb.velocity;
        vel.x = 0f;
        vel.z = 0f;
        rb.velocity = vel;

        Debug.Log("K presionada, ventana de detección abierta y movimiento bloqueado...");
    }

    // 👉 Se llama cada frame mientras la ventana está activa
    private bool TryKickHit()
    {
        if (kickPoint == null)
        {
            Debug.LogWarning("KickPoint no asignado");
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(kickPoint.position, kickRadius, ballLayer);
        // Collider[] hits = Physics.OverlapSphere(kickPoint.position, kickRadius); // <- para pruebas

        Debug.Log("Chequeando balones... detectados: " + hits.Length);

        foreach (Collider hit in hits)
        {
            BallController ball = hit.GetComponent<BallController>();
            if (ball != null)
            {
                Vector3 dir = (hit.transform.position - kickPoint.position).normalized;
                dir.y = 0.4f;

                ball.OnKicked(dir, kickForce);
                Debug.Log("Balón pateado!");
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Esfera de patada (roja)
        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRadius);
        }

        // Esfera de groundCheck (amarilla)
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
