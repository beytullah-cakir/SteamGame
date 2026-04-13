using UnityEngine;

public enum BoxColor { None, Red, Blue, Green, Yellow }

[RequireComponent(typeof(Rigidbody))]
public class BoxPullable : MonoBehaviour
{
    [Tooltip("Kutunun bulmacalarda kullanılacak rengi")]
    public BoxColor boxColor = BoxColor.None;

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
