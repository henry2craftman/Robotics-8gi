using UnityEngine;

public class Handle : MonoBehaviour
{
    public Pinball pinballManager;
    public float forcePower = 3;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ball")
        {
            if(transform.parent.name.Contains("Left") && pinballManager.isLeftHandleMoving)
            {
                Vector3 contact = collision.contacts[0].point;
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                rb.AddForce(contact * forcePower, ForceMode.Impulse);
            }
            else if(transform.parent.name.Contains("Right") && pinballManager.isRightHandleMoving)
            {
                Vector3 contact = collision.contacts[0].point;
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                rb.AddForce(contact * forcePower, ForceMode.Impulse);
            }
        }
    }
}
