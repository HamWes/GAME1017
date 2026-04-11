using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.21f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Game Over")]
    [SerializeField] private float fallGameOverY = -5f;
    [SerializeField] private float gameOverFlashDuration = 0.6f;
    [SerializeField] private float gameOverFlashInterval = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Color originalColor;

    private InputSystem_Actions input;
    private bool isGrounded;
    private bool isLosing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        input = new InputSystem_Actions();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Initialize()
    {
        startPosition = transform.position;
        isLosing = false;

        rb.simulated = true;
        rb.gravityScale = 3f;
        RestoreSpriteColor();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.InGame) return;

        if (transform.position.y < fallGameOverY)
        {
            GameManager.Instance.GameOver();
            return;
        }

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
        isLosing = false;
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        RestoreSpriteColor();
    }

    public IEnumerator PlayGameOverFeedback()
    {
        if (isLosing)
        {
            yield break;
        }

        isLosing = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (spriteRenderer == null)
        {
            yield return new WaitForSeconds(gameOverFlashDuration);
            yield break;
        }

        Color[] flashColors =
        {
            Color.green,
            Color.yellow,
            new Color(0f, 0.75f, 0.75f, 1f)
        };

        float elapsed = 0f;
        int colorIndex = 0;

        while (elapsed < gameOverFlashDuration)
        {
            spriteRenderer.color = flashColors[colorIndex];
            yield return new WaitForSeconds(gameOverFlashInterval);
            elapsed += gameOverFlashInterval;
            colorIndex = (colorIndex + 1) % flashColors.Length;
        }

        RestoreSpriteColor();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Obstacle>())
        {
            GameManager.Instance.GameOver();
        }
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

    private void RestoreSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
