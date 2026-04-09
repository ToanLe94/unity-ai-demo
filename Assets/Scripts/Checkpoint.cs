using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            LapManager.Instance?.RegisterCheckpoint(index);
    }
}
