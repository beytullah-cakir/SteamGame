using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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

    private PlayerClimb playerClimb;
    
    TwoBoneIKConstraint rightHand, leftHand;

    private void Awake()
    {
        if (targetZip != null)
        {
            cable.SetPosition(0, zipTransform.position);
            cable.SetPosition(1, targetZip.zipTransform.position);
        }
        else cable.enabled = false;

    }

    protected void Update()
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
        
        playerClimb=player.GetComponent<PlayerClimb>();
        //playerClimb.isZipped=true;
        SetPLayerIK(player);
        localZip=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        localZip.transform.position = zipTransform.position;
        localZip.transform.localScale = Vector3.one * zipScale;
        localZip.AddComponent<Rigidbody>().useGravity = false;
        localZip.GetComponent<Collider>().isTrigger = true;

        player.GetComponent<Rigidbody>().useGravity = false;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        
        player.transform.parent=localZip.transform;
        isZipping = true;
    }

    public void ResetZipline()
    {
        if(!isZipping) return;
        GameObject player = localZip.transform.GetChild(0).gameObject;
        ResetPlayerIK();
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponent<PlayerMovement>().enabled = true;
        
        player.transform.parent = null;
        Destroy(localZip);
        //playerClimb.isZipped=false;
        isZipping = false;
        localZip = null;

    }

    void SetPLayerIK(GameObject player)
    {
        rightHand = player.GetComponent<PlayerMovement>().rightHand;
        leftHand= player.GetComponent<PlayerMovement>().leftHand;
        leftHand.weight = 1;
        rightHand.weight = 1;
        rightHand.data.target.position = zipTransform.position;
        rightHand.data.target.rotation = zipTransform.rotation;
        leftHand.data.target.position = zipTransform.position;
        leftHand.data.target.rotation = zipTransform.rotation;
    }

    void ResetPlayerIK()
    {
        rightHand.weight = 0;
        leftHand.weight = 0;
        leftHand = null;
        rightHand = null;
    }
}
