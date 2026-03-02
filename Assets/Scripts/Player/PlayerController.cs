using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector3 startPosition;

    private InputSystem_Actions input;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = new InputSystem_Actions();
    }

    public void Initialize()
    {
        startPosition = transform.position;

        rb.simulated = true;
        rb.gravityScale = 3f;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.InGame) return;

        if (groundCheck != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

            isGrounded = hit.collider != null;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameState.InGame) return;

        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    private void OnJump(InputValue value)
    {
        if (GameManager.Instance.CurrentGameState != GameState.InGame) return;
        if (!value.isPressed) return;
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckDistance
        );
    }
}
