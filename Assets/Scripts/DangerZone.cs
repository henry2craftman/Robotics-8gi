using UnityEngine;


public class DangerZone : MonoBehaviour
{
    public bool isDetected;

    private void OnTriggerEnter(Collider other)
    {
        isDetected = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isDetected = false;
    }
}
