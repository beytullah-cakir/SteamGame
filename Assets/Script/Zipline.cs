using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    [SerializeField] private Zipline targetZip;
    [SerializeField] private float zipSpeed;
    [SerializeField] private float zipScale;

    [SerializeField] private float arrivalTreshold=0.4f;
    [SerializeField] private LineRenderer cable;

    public Transform zipTransform;

    private bool isZipping = false;
    private GameObject localZip;

    private void Awake()
    {
        if (targetZip != null)
        {
            cable.SetPosition(0, zipTransform.position);
            cable.SetPosition(1, targetZip.zipTransform.position);
        }
        else cable.enabled = false;

    }

    private void Update()
    {
        if (!isZipping || localZip == null || targetZip==null) return;
        localZip.GetComponent<Rigidbody>().AddForce((targetZip.zipTransform.position - zipTransform.position).normalized * zipSpeed * Time.deltaTime, ForceMode.Acceleration);
        if (Vector3.Distance(localZip.transform.position, targetZip.zipTransform.position) <= arrivalTreshold)
        {
            ResetZipline();
        }
    }

    public void StartZipline(GameObject player)
    {
        if (isZipping || targetZip == null) return;
        localZip=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        localZip.transform.position = zipTransform.position;
        localZip.transform.localScale = Vector3.one * zipScale;
        localZip.AddComponent<Rigidbody>().useGravity = false;
        localZip.GetComponent<Collider>().isTrigger = true;

        player.GetComponent<Rigidbody>().useGravity = false;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<Grappling>().enabled = false;
        player.transform.parent=localZip.transform;
        isZipping = true;
    }

    public void ResetZipline()
    {
        if(!isZipping) return;
        GameObject player = localZip.transform.GetChild(0).gameObject;
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<Grappling>().enabled = true;
        player.transform.parent = null;
        Destroy(localZip);
        isZipping = false;
        localZip = null;

    }
}
