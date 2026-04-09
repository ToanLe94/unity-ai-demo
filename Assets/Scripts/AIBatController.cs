using UnityEngine;

/// <summary>
/// Attached to PingPongBat (1) (AI opponent).
/// - Tracks the ball when it is on the AI's side of the table.
/// - Automatically hits the ball back toward the player.
/// - Stops hitting after aiMaxHits (15-20, set by TableTennisManager).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AIBatController : MonoBehaviour
{
    [Header("Bat Position")]
    public float defaultBatZ  = -0.52f;  // default Z rest position
    public float batY         = 0.25f;
    public float xClamp       = 1.15f;

    [Header("Movement")]
    public float moveSpeed    = 3.2f;

    [Header("Hit")]
    public float hitRange     = 0.22f;   // 3-D distance trigger
    public float hitForceZ    = 2.55f;
    public float hitForceY    = 1.9f;
    public float hitRandX     = 0.45f;

    private Rigidbody rb;
    private bool      hitRegistered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic   = true;
        rb.useGravity    = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Place AI bat at its default position
        transform.position = new Vector3(0f, batY, defaultBatZ);
    }

    void FixedUpdate()
    {
        var mgr = TableTennisManager.Instance;
        if (!mgr || mgr.State != TTGameState.InPlay || !mgr.ball) return;

        var  ball   = mgr.ball;
        bool canHit = mgr.aiHitCount < mgr.aiMaxHits;

        if (ball.position.z < 0f && canHit)
        {
            // ---- Chase ball -----------------------------------------------
            // Follow ball X and close in on its Z from behind
            float targetX = Mathf.Clamp(ball.position.x, -xClamp, xClamp);
            // Stay slightly behind the ball (further from net than ball)
            float targetZ = Mathf.Clamp(ball.position.z - 0.07f,
                                         -TableTennisManager.TableHalfZ + 0.06f,
                                         -0.18f);

            float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.fixedDeltaTime);
            float newZ = Mathf.MoveTowards(transform.position.z, targetZ, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newX, batY, newZ));

            // ---- Hit detection --------------------------------------------
            // Ball must be moving toward AI (vz < 0) and within hit range
            float dist = Vector3.Distance(
                new Vector3(transform.position.x, batY, transform.position.z),
                ball.position);

            // Ball is approaching if it moves in the same direction as the racket's forward (local X)
            bool ballApproaching = Vector3.Dot(ball.linearVelocity, transform.right) > 0.2f;

            if (dist < hitRange && ballApproaching && !hitRegistered)
                HitBall(ball, mgr);
        }
        else
        {
            // ---- Return to center -----------------------------------------
            float newX = Mathf.MoveTowards(transform.position.x, 0f,          moveSpeed * Time.fixedDeltaTime);
            float newZ = Mathf.MoveTowards(transform.position.z, defaultBatZ, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newX, batY, newZ));

            // Reset hit flag once ball is back on player side
            if (ball.position.z >= 0f)
                hitRegistered = false;
        }
    }

    void HitBall(Rigidbody ball, TableTennisManager mgr)
    {
        hitRegistered = true;

        // Forward direction = racket local X axis (flattened to horizontal)
        Vector3 forward = transform.right;
        forward.y = 0f;
        forward   = forward.normalized;

        // Small random lateral spread to make rallies interesting
        Vector3 lateral = Vector3.Cross(forward, Vector3.up);
        float   rx      = Random.Range(-hitRandX, hitRandX);

        ball.linearVelocity = forward * hitForceZ
                            + Vector3.up * hitForceY
                            + lateral * rx;

        mgr.OnAIHit();
        Debug.Log($"[AI] Hit #{mgr.aiHitCount} of {mgr.aiMaxHits}");
    }
}
