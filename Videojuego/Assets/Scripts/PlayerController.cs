using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Correr")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    private bool isRunning = false;

    [Header("Movimiento")]
    public float rotationSpeed = 10f;

    [Header("Cámara")]
    public Transform cameraTransform;

    [Header("Salto")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Patear")]
    public Transform kickPoint;
    public float kickRadius = 1.5f;
    public float kickForce = 20f;
    public LayerMask ballLayer;
    public float kickDetectionWindow = 1f;

    [Header("Animación")]
    public Animator animator;

    private Rigidbody rb;
    private float inputV;
    private float inputH;
    private bool isGrounded;

    private bool isKickingWindow = false;
    private float kickTimer = 0f;

    private bool isKicking = false;
    private float kickLockTimer = 0f;
    public float kickLockTime = 0.6f;

    public bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        inputV = Input.GetAxisRaw("Vertical");
        inputH = Input.GetAxisRaw("Horizontal");

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

            float targetMoveZ = isKicking ? 0f : inputV;
            float smoothZ = Mathf.Lerp(animator.GetFloat("MoveZ"), targetMoveZ, Time.deltaTime * 10f);
            animator.SetFloat("MoveZ", smoothZ);

            float targetTurn = isKicking ? 0f : inputH;
            float smoothTurn = Mathf.Lerp(animator.GetFloat("Turn"), targetTurn, Time.deltaTime * 10f);
            animator.SetFloat("Turn", smoothTurn);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isKicking)
            Jump();

        if (Input.GetMouseButtonDown(0) && !isKicking)
            StartKick();

        if (isKickingWindow)
        {
            kickTimer += Time.deltaTime;
            bool hit = TryKickHit();
            if (hit || kickTimer >= kickDetectionWindow)
                isKickingWindow = false;
        }

        if (isKicking)
        {
            kickLockTimer += Time.deltaTime;
            if (kickLockTimer >= kickLockTime)
                isKicking = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = !isRunning;
    }

    private void FixedUpdate()
    {
        if (isKicking)
        {
            Vector3 vel = rb.velocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;
            return;
        }

        if (cameraTransform == null)
        {
            Vector3 fallback = transform.forward * (isRunning ? runSpeed : walkSpeed);
            rb.velocity = new Vector3(fallback.x, rb.velocity.y, fallback.z);
            return;
        }

        Vector3 camF = cameraTransform.forward;
        Vector3 camR = cameraTransform.right;

        camF.y = 0;
        camR.y = 0;

        camF.Normalize();
        camR.Normalize();

        Vector3 moveDir = camF * inputV + camR * inputH;
        float finalSpeed = isRunning ? runSpeed : walkSpeed;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();
            Vector3 targetVel = moveDir * finalSpeed;
            rb.velocity = new Vector3(targetVel.x, rb.velocity.y, targetVel.z);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    private void Jump()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    private void StartKick()
    {
        if (animator != null)
            animator.SetTrigger("Kick");

        isKickingWindow = true;
        kickTimer = 0f;

        isKicking = true;
        kickLockTimer = 0f;

        Vector3 vel = rb.velocity;
        vel.x = 0;
        vel.z = 0;
        rb.velocity = vel;
    }

    private bool TryKickHit()
    {
        if (kickPoint == null)
            return false;

        Collider[] hits = Physics.OverlapSphere(kickPoint.position, kickRadius, ballLayer);

        foreach (Collider hit in hits)
        {
            BallController ball = hit.GetComponent<BallController>();
            if (ball != null)
            {
                Vector3 dir = (hit.transform.position - kickPoint.position).normalized;
                dir.y = 0.4f;

                ball.OnKicked(dir, kickForce);
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
