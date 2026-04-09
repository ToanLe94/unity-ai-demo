using UnityEngine;

public class KartCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 7f;
    public float height = 3f;
    public float lookAhead = 4f;
    public float positionSmoothTime = 0.12f;
    public float rotationSmoothSpeed = 8f;

    Vector3 _velRef;
    Quaternion _currentRot;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position
            - target.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _velRef, positionSmoothTime);

        Vector3 lookTarget = target.position + target.forward * lookAhead + Vector3.up * 0.5f;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationSmoothSpeed);
    }
}
