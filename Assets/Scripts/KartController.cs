using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 28f;
    public float acceleration = 18f;
    public float brakeForce = 35f;
    public float reverseMaxSpeed = 8f;

    [Header("Steering")]
    public float maxSteerAngle = 45f;
    public float steerSpeed = 6f;

    [Header("Drift")]
    public float driftLateralGrip = 2f;
    public float normalLateralGrip = 10f;
    public float driftSteerBonus = 25f;

    [Header("Physics")]
    public float downForce = 60f;
    public float extraGravity = 20f;
    public LayerMask groundMask = ~0;

    [Header("Wheels (visual)")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    Rigidbody _rb;
    float _accelInput;
    float _steerInput;
    bool _isDrifting;
    float _smoothSteer;
    bool _isGrounded;

    public float CurrentSpeedKmh => Vector3.Dot(_rb.linearVelocity, transform.forward) * 3.6f;
    public bool IsDrifting => _isDrifting;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = 500f;
        _rb.linearDamping = 0.3f;
        _rb.angularDamping = 5f;
        _rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
    }

    void Update()
    {
        _accelInput = Input.GetAxis("Vertical");
        _steerInput = Input.GetAxis("Horizontal");
        _isDrifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space);

        SpinWheels();
    }

    void FixedUpdate()
    {
        _isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, -transform.up, 0.5f, groundMask);

        if (_isGrounded)
        {
            ApplyThrottle();
            ApplySteering();
            ApplyLateralGrip();
            _rb.AddForce(-transform.up * downForce * _rb.linearVelocity.magnitude);
        }

        _rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    void ApplyThrottle()
    {
        float forwardSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);

        if (_accelInput > 0f && forwardSpeed < maxSpeed)
            _rb.AddForce(transform.forward * _accelInput * acceleration, ForceMode.Acceleration);
        else if (_accelInput < 0f && forwardSpeed > -reverseMaxSpeed)
            _rb.AddForce(transform.forward * _accelInput * brakeForce, ForceMode.Acceleration);

        // Natural drag when no input
        if (Mathf.Approximately(_accelInput, 0f))
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, 0.015f);
    }

    void ApplySteering()
    {
        float forwardSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);
        float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);

        float targetSteer = _steerInput * maxSteerAngle * speedRatio;
        if (_isDrifting) targetSteer += _steerInput * driftSteerBonus * speedRatio;

        _smoothSteer = Mathf.Lerp(_smoothSteer, targetSteer, Time.fixedDeltaTime * steerSpeed);

        float direction = forwardSpeed >= 0f ? 1f : -1f;
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, _smoothSteer * direction * Time.fixedDeltaTime, 0f));
    }

    void ApplyLateralGrip()
    {
        Vector3 right = transform.right;
        float lateralSpeed = Vector3.Dot(_rb.linearVelocity, right);
        float grip = _isDrifting ? driftLateralGrip : normalLateralGrip;
        _rb.AddForce(-right * lateralSpeed * grip, ForceMode.Acceleration);
    }

    void SpinWheels()
    {
        float forwardSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);
        float spinDeg = forwardSpeed * 360f / (2f * Mathf.PI * 0.3f) * Time.deltaTime;

        RotateWheel(wheelFL, spinDeg, _smoothSteer);
        RotateWheel(wheelFR, spinDeg, _smoothSteer);
        RotateWheel(wheelRL, spinDeg, 0f);
        RotateWheel(wheelRR, spinDeg, 0f);
    }

    void RotateWheel(Transform wheel, float spin, float steer)
    {
        if (wheel == null) return;
        wheel.localRotation = Quaternion.Euler(
            wheel.localRotation.eulerAngles.x + spin,
            steer,
            wheel.localRotation.eulerAngles.z);
    }
}
