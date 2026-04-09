using UnityEngine;

public enum TTGameState { WaitingToServe, InPlay, Resetting }

public class TableTennisManager : MonoBehaviour
{
    public static TableTennisManager Instance { get; private set; }

    [HideInInspector] public TTGameState State = TTGameState.WaitingToServe;
    [HideInInspector] public int aiHitCount = 0;
    [HideInInspector] public int aiMaxHits = 18;

    // Table world-space constants (derived from scene inspection)
    public const float TableTopY  = 0.17f;
    public const float TableHalfX = 1.37f;
    public const float TableHalfZ = 0.76f;

    [HideInInspector] public Rigidbody ball;

    void Awake()
    {
        Instance = this;
        var ballObj = GameObject.Find("Ball");
        if (ballObj) ball = ballObj.GetComponent<Rigidbody>();
    }

    void Start()
    {
        SetupPhysics();
        ResetBall();
    }

    void SetupPhysics()
    {
        if (!ball) return;

        // Ball: high bounciness (MaxCombine wins), low friction
        var ballCol = ball.GetComponent<SphereCollider>();
        if (ballCol)
        {
            ballCol.material = new PhysicsMaterial("BallMat")
            {
                bounciness      = 0.78f,
                dynamicFriction = 0.2f,
                staticFriction  = 0.2f,
                bounceCombine   = PhysicsMaterialCombine.Maximum,
                frictionCombine = PhysicsMaterialCombine.Average
            };
        }
        ball.linearDamping        = 0.04f;
        ball.angularDamping = 0.08f;

        // Table: moderate bounciness
        var tableObj = GameObject.Find("PingPongTable");
        if (tableObj)
        {
            var col = tableObj.GetComponent<Collider>();
            if (col)
            {
                col.material = new PhysicsMaterial("TableMat")
                {
                    bounciness      = 0.62f,
                    dynamicFriction = 0.35f,
                    staticFriction  = 0.35f,
                    bounceCombine   = PhysicsMaterialCombine.Maximum,
                    frictionCombine = PhysicsMaterialCombine.Average
                };
            }
        }

        // Bat colliders: zero bounce so physics doesn't interfere with scripted hits
        PhysicsMaterial batMat = new PhysicsMaterial("BatMat")
        {
            bounciness      = 0f,
            dynamicFriction = 0f,
            staticFriction  = 0f,
            bounceCombine   = PhysicsMaterialCombine.Minimum,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        foreach (string n in new[] { "PingPongBat", "PingPongBat (1)" })
        {
            var obj = GameObject.Find(n);
            if (!obj) continue;
            foreach (var col in obj.GetComponents<Collider>())
                col.material = batMat;
        }
    }

    public void ResetBall()
    {
        if (!ball) return;
        State       = TTGameState.WaitingToServe;
        aiHitCount  = 0;
        aiMaxHits   = Random.Range(15, 21);

        ball.isKinematic       = true;
        ball.linearVelocity          = Vector3.zero;
        ball.angularVelocity   = Vector3.zero;
        // Ball will be positioned each frame by PlayerBatController
    }

    // Called by PlayerBatController on LMB click
    public void Serve()
    {
        if (State != TTGameState.WaitingToServe) return;
        State = TTGameState.InPlay;

        ball.isKinematic = false;
        float rx = Random.Range(-0.06f, 0.06f);
        // Arc that bounces once on player side then crosses to AI side
        ball.linearVelocity = new Vector3(rx, 1.5f, -1.35f);
    }

    public void OnAIHit()
    {
        aiHitCount++;
    }

    void Update()
    {
        if (State != TTGameState.InPlay || !ball) return;

        var p = ball.position;
        bool outOfBounds = p.y < TableTopY - 0.9f
                        || Mathf.Abs(p.x) > TableHalfX + 0.8f
                        || Mathf.Abs(p.z) > TableHalfZ + 1.4f;

        if (outOfBounds)
        {
            State = TTGameState.Resetting;
            Invoke(nameof(ResetBall), 1.8f);
        }
    }
}
