using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;          // Adelante / atrás
    public float rotationSpeed = 10f;    // Más velocidad de giro

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

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

        rb.freezeRotation = true;
    }

    private void Update()
    {
        // 1) LEER INPUTS
        inputV = Input.GetAxisRaw("Vertical");   // W (1) / S (-1)
        inputH = Input.GetAxisRaw("Horizontal"); // A (-1) / D (1)

        // 2) CHEQUEAR SUELO
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
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

        // 5) SALTO (Space)
        if (Input.GetKeyDown(KeyCode.Space))
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
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
    }

    private void Kick()
    {
        animator.SetTrigger("Kick");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
