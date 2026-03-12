using UnityEngine;

public class CylinderSensor : MonoBehaviour
{
    public bool isDetected;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "금속")
            isDetected = true;
    }
}
