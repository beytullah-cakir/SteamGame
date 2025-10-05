using UnityEngine;

public class RopeSwing : MonoBehaviour
{
    public GameObject rope;
    public bool isSwing;
    public float force;
    private Rigidbody rb;
    private HingeJoint hingeJoint;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isSwing)
        {
            StartSwing();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            StopSwing();
        }

        if (isSwing)
        {
            Swinging();
        }
        
    }

    private void Swinging()
    {
        if(Input.GetKey(KeyCode.W)) rb.AddForce(rope.transform.right * force, ForceMode.Acceleration);
        if(Input.GetKey(KeyCode.S)) rb.AddForce(-rope.transform.right * force, ForceMode.Acceleration);
        
            
    }

    private void StartSwing()
    {
        if(rope==null) return;
        transform.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        transform.SetParent(rope.transform);
        transform.rotation = Quaternion.LookRotation(rope.transform.right, Vector3.up);
        transform.SetLocalPositionAndRotation(new Vector3(0,-13,0), Quaternion.identity);
        hingeJoint=gameObject.AddComponent<HingeJoint>();
        isSwing = true;
        hingeJoint.connectedBody = rope.transform.GetChild(10).gameObject.GetComponent<Rigidbody>();
       
        
    }

    private void  StopSwing()
    {
        transform.gameObject.GetComponent<BoxCollider>().isTrigger = false;
        Destroy(hingeJoint);
        isSwing = false;
        transform.SetParent(null);
        
        
    }
}
