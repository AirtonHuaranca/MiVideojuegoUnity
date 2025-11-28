using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;          // Adelante / atrás
    public float rotationSpeed = 220f;    // Grados por segundo (más rápido)

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Patear")]
    public Transform kickPoint;       // Asignar en el inspector (un Empty en el pie)
    public float kickRadius = 1.2f;   // Radio de la esfera de impacto
    public float kickForce = 12f;     // Fuerza que se aplica al balón

    [Header("Animación")]
    public Animator animator;

    private Rigidbody rb;
    private float inputV;  // W / S
    private float inputH;  // A / D
    private bool isGrounded;

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

        // 2) CHEQUEAR SUELO (si no hay groundCheck, asumimos grounded)
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

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);

            // 3) BLEND TREE: MoveZ (adelante/atrás)
            float targetMoveZ = inputV;
            float currentMoveZ = animator.GetFloat("MoveZ");
            float smoothMoveZ = Mathf.Lerp(currentMoveZ, targetMoveZ, Time.deltaTime * 10f);
            animator.SetFloat("MoveZ", smoothMoveZ);

            // 4) BLEND TREE: Turn (giro en el sitio)
            float targetTurn = inputH;  // A = -1, D = 1
            float currentTurn = animator.GetFloat("Turn");
            float smoothTurn = Mathf.Lerp(currentTurn, targetTurn, Time.deltaTime * 10f);
            animator.SetFloat("Turn", smoothTurn);
        }

        // 5) SALTO (Space) – solo si está en el piso
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // 6) PATEAR (K)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Kick();
        }
    }

    private void FixedUpdate()
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

    private void Kick()
    {
        if (animator != null)
            animator.SetTrigger("Kick");

        if (kickPoint == null)
        {
            Debug.LogWarning("Kick: kickPoint NO asignado en el inspector.");
            return;
        }

        // Buscar balones cerca del pie
        Collider[] hits = Physics.OverlapSphere(
            kickPoint.position,
            kickRadius
        );

        Debug.Log("Kick: colliders detectados = " + hits.Length);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Ball"))
                continue;

            Rigidbody ballRb = hit.attachedRigidbody;
            if (ballRb == null)
                continue;

            // Dirección desde el pie hacia el balón
            Vector3 dir = (hit.transform.position - kickPoint.position).normalized;
            dir.y = 0.2f; // un poco hacia arriba

            ballRb.AddForce(dir * kickForce, ForceMode.Impulse);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBallKicked();
            }

            // Que desaparezca del campo tras la patada
            Destroy(hit.gameObject, 0.5f);
        }
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
