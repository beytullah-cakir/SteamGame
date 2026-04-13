using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoxPullable : MonoBehaviour
{
    
    public bool isHooked;

    [HideInInspector] public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnReleased()
    {
        isHooked = false;
        rb.linearDamping = 1f;
    }

    public void OnGrabbed()
    {
        isHooked = true;
        rb.linearDamping = 5f;
    }
}
