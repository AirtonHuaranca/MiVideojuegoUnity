using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Patear")]
    public Transform kickPoint;
    public float kickRadius = 1f;
    public float kickForce = 10f;

    [Header("Animación")]
    public Animator animator;

    private Rigidbody rb;
    private Vector3 inputDir;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // INPUT
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        inputDir = new Vector3(h, 0f, v).normalized;

        // Actualizar Blend Tree
        animator.SetFloat("MoveX", h);
        animator.SetFloat("MoveZ", v);

        // Comprobar suelo
        CheckGround();
        animator.SetBool("IsGrounded", isGrounded);

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Patada (ej: tecla K)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Kick();
        }
    }

    void FixedUpdate()
    {
        // Movimiento
        Vector3 velocity = inputDir * moveSpeed;
        Vector3 move = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        rb.velocity = move;

        // Rotar hacia la dirección de movimiento
        Vector3 lookDir = new Vector3(inputDir.x, 0f, inputDir.z);
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = true;
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        animator.SetTrigger("Jump");
    }

    void Kick()
    {
        animator.SetTrigger("Kick");

        // Detectar balones cerca del punto de patada
        Collider[] hits = Physics.OverlapSphere(kickPoint.position, kickRadius);
        foreach (Collider col in hits)
        {
            Ball ball = col.GetComponent<Ball>();
            if (ball != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                ball.Kick(dir, kickForce);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
