using UnityEngine;
using UnityEngine.Rendering;

public class HookSystem : MonoBehaviour
{
    public LineRenderer lr;

    private Vector3 swingingPoint;

    public LayerMask whatIsGrappleable;

    public Transform gunTip, player;

    public float maxDistance;

    private SpringJoint joint;    

    private Vector3 currentGrapplePosition;  
    
    public GameObject grappleIndicator;

    public bool swinging;

    public float spring;

    public float damping;

    public float massScale;

    void Update()
    {

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;
        bool found = Physics.SphereCast(ray, 1f, out hit, maxDistance, whatIsGrappleable);

        if (found && hit.collider.CompareTag("Swingable"))
        {
            // UI g�stergesini aktif et ve ekran pozisyonuna yerle�tir
            grappleIndicator.SetActive(true);
            grappleIndicator.transform.position = Camera.main.WorldToScreenPoint(hit.point);            
            if (Input.GetMouseButtonDown(0))
            {
                StartGrapple(hit.point);
            }
        }
        else
        {
            grappleIndicator.SetActive(false);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }


    void StartGrapple(Vector3 targetPoint)
    {
        swinging = true;
        swingingPoint = targetPoint;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = spring;
        joint.damper = damping;
        joint.massScale = massScale;
       
        currentGrapplePosition = gunTip.position;
    }

    void StopGrapple()
    {      
        swinging = false;
        if (joint != null)
        {
            Destroy(joint);
        }
    }

    public bool IsSwinging()
    {
        return swinging;
    }

    public Vector3 GetSwingingPoint()
    {
        return swingingPoint;
    }

}
