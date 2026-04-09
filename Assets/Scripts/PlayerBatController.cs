using UnityEngine;

/// <summary>
/// Attached to PingPongBat (player).
/// - Mouse X moves the bat along the table width.
/// - LMB click while WaitingToServe → serve.
/// - LMB press + upward screen swipe + release → hit ball toward AI side.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerBatController : MonoBehaviour
{
    [Header("Bat Position")]
    public float batY = 0.23f;   // height just above table surface
    public float batZ = 0.56f;   // player's fixed Z (near side)
    public float xClamp = 1.20f;

    [Header("Hit")]
    public float hitRange      = 0.30f;  // 3-D distance to ball
    public float minSwipePx    = 45f;    // minimum upward pixel drag to count as hit
    public float hitForceMin   = 2.0f;
    public float hitForceMax   = 4.8f;

    private Rigidbody    rb;
    private Camera       mainCam;
    private Vector3      mousePressPos;
    private Vector3      mouseCurrentPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic  = true;
        rb.useGravity   = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        mainCam = Camera.main;
    }

    void Update()
    {
        MoveBatWithMouse();
        HandleInput();
        HoldBallWhileServing();
    }

    // -------------------------------------------------------------------
    void MoveBatWithMouse()
    {
        if (!mainCam) return;
        Ray   ray   = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, batY, 0f));
        if (plane.Raycast(ray, out float dist))
        {
            Vector3 wp = ray.GetPoint(dist);
            float   x  = Mathf.Clamp(wp.x, -xClamp, xClamp);
            rb.MovePosition(new Vector3(x, batY, batZ));
        }
    }

    // -------------------------------------------------------------------
    void HoldBallWhileServing()
    {
        var mgr = TableTennisManager.Instance;
        if (!mgr || !mgr.ball) return;
        if (mgr.State != TTGameState.WaitingToServe) return;

        // Keep ball floating just in front of the bat
        mgr.ball.isKinematic = true;
        mgr.ball.position    = new Vector3(
            transform.position.x,
            TableTennisManager.TableTopY + 0.09f,
            0.46f);
    }

    // -------------------------------------------------------------------
    void HandleInput()
    {
        var mgr = TableTennisManager.Instance;
        if (!mgr) return;

        if (Input.GetMouseButtonDown(0))
        {
            mousePressPos   = Input.mousePosition;
            mouseCurrentPos = Input.mousePosition;

            if (mgr.State == TTGameState.WaitingToServe)
            {
                mgr.ball.isKinematic = false;
                mgr.Serve();
            }
        }

        if (Input.GetMouseButton(0))
            mouseCurrentPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0) && mgr.State == TTGameState.InPlay)
        {
            Vector3 swipe = mouseCurrentPos - mousePressPos;
            if (swipe.y >= minSwipePx)
                TryHitBall(swipe);
        }
    }

    // -------------------------------------------------------------------
    void TryHitBall(Vector3 swipe)
    {
        var mgr = TableTennisManager.Instance;
        if (!mgr || !mgr.ball) return;

        float dist = Vector3.Distance(mgr.ball.position, transform.position);
        if (dist > hitRange) return;

        float t     = Mathf.Clamp01(swipe.y / 280f);
        float speed = Mathf.Lerp(hitForceMin, hitForceMax, t);
        float fy    = Mathf.Lerp(1.2f, 2.4f, t);

        // Forward direction = racket local X axis (flattened to horizontal)
        Vector3 forward = transform.right;
        forward.y = 0f;
        forward   = forward.normalized;

        // Small lateral nudge from horizontal mouse drag
        Vector3 lateral = transform.up;
        lateral.y = 0f;
        float sideStrength = (swipe.x / Screen.width) * 1.6f;

        mgr.ball.linearVelocity = forward * speed
                                + Vector3.up * fy
                                + lateral * sideStrength;
    }
}
