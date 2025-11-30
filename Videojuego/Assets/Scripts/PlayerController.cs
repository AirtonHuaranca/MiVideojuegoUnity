using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 4f;          // velocidad base al caminar
    public float runMultiplier = 1.7f;    // multiplicador al correr (Shift)
    public float rotationSpeed = 10f;     // qué tan rápido gira hacia la dirección de movimiento

    [Header("Cámara (tipo Valorant)")]
    public Transform cameraTransform;     // arrastra aquí la Main Camera (la que tiene el CinemachineBrain)

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Patear")]
    public Transform kickPoint;      // Empty en el pie
    public float kickRadius = 1.5f;  // radio de detección
    public float kickForce = 20f;    // fuerza de la patada
    public LayerMask ballLayer;      // capa de los balones

    // ventana de tiempo para detectar el balón
    public float kickDetectionWindow = 1f;

    [Header("Animación")]
    public Animator animator;

    private Rigidbody rb;
    private float inputV;  // W / S
    private float inputH;  // A / D
    private bool isGrounded;

    // control de ventana de patada
    private bool isKickingWindow = false;
    private float kickTimer = 0f;

    // bloqueo de movimiento mientras patea
    public float kickLockTime = 0.6f;
    private bool isKicking = false;
    private float kickLockTimer = 0f;

    public bool canMove = true;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;

        // si no asignas la cámara a mano, intenta usar la Main Camera
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
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

        // 3) ANIMACIONES (manteniendo tus parámetros)
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);

            // MoveZ → adelante / atrás (W/S)
            float targetMoveZ = isKicking ? 0f : inputV;
            float currentMoveZ = animator.GetFloat("MoveZ");
            float smoothMoveZ = Mathf.Lerp(currentMoveZ, targetMoveZ, Time.deltaTime * 10f);
            animator.SetFloat("MoveZ", smoothMoveZ);

            // Turn → izquierda / derecha (A/D) como strafe
            float targetTurn = isKicking ? 0f : inputH;
            float currentTurn = animator.GetFloat("Turn");
            float smoothTurn = Mathf.Lerp(currentTurn, targetTurn, Time.deltaTime * 10f);
            animator.SetFloat("Turn", smoothTurn);
        }

        // 4) SALTO (Espacio)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isKicking)
        {
            Jump();
        }

        // 5) PATEAR (CLICK IZQUIERDO)
        if (Input.GetMouseButtonDown(0) && !isKicking)
        {
            StartKick();   // arranca animación + ventana de detección
        }

        // 6) Ventana de detección de patada
        if (isKickingWindow)
        {
            kickTimer += Time.deltaTime;

            bool hitSomething = TryKickHit();

            if (hitSomething || kickTimer >= kickDetectionWindow)
            {
                isKickingWindow = false;
            }
        }

        // 7) Tiempo de bloqueo mientras patea
        if (isKicking)
        {
            kickLockTimer += Time.deltaTime;
            if (kickLockTimer >= kickLockTime)
            {
                isKicking = false;  // ya puede volver a moverse
            }
        }
    }

    private void FixedUpdate()
    {
        if (isKicking)
        {
            // mientras está pateando, no se mueve (solo gravedad)
            Vector3 vel = rb.velocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;
            return;
        }

        if (cameraTransform == null)
        {
            // por seguridad, si no hay cámara, no hacemos movimiento tipo Valorant
            Vector3 fallbackMove = transform.forward * inputV * moveSpeed;
            Vector3 velocityFallback = new Vector3(fallbackMove.x, rb.velocity.y, fallbackMove.z);
            rb.velocity = velocityFallback;
            return;
        }

        // 🎮 MOVIMIENTO ESTILO VALORANT (relativo a la cámara)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // quitar componente vertical para que no camine en pendiente rara
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // dirección de movimiento según WASD y cámara
        Vector3 moveDir = camForward * inputV + camRight * inputH;

        // ¿corriendo?
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= runMultiplier;
        }

        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();
            Vector3 targetVel = moveDir * currentSpeed;
            rb.velocity = new Vector3(targetVel.x, rb.velocity.y, targetVel.z);

            // ROTACIÓN SUAVE HACIA LA DIRECCIÓN DE MOVIMIENTO
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // sin input → solo gravedad
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    private void Jump()
    {
        // limpiar velocidad vertical antes de saltar
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    private void StartKick()
    {
        if (animator != null)
        {
            animator.SetTrigger("Kick");
        }

        // activar ventana de detección
        isKickingWindow = true;
        kickTimer = 0f;

        // bloquear movimiento mientras dure la patada
        isKicking = true;
        kickLockTimer = 0f;

        // limpiar velocidad horizontal
        Vector3 vel = rb.velocity;
        vel.x = 0f;
        vel.z = 0f;
        rb.velocity = vel;

        Debug.Log("Click izquierdo, patada iniciada (ventana de detección activa).");
    }

    private bool TryKickHit()
    {
        if (kickPoint == null)
        {
            Debug.LogWarning("KickPoint no asignado");
            return false;
    }

        Collider[] hits = Physics.OverlapSphere(kickPoint.position, kickRadius, ballLayer);

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
