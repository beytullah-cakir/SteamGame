using UnityEngine;

public class ShimmyController : MonoBehaviour
{
    PlayerClimb playerClimbScript;
    

    public float sphereRadius;
    public float sphereGap;

    public float upPos = 1.6f;
    public float forwardPos = 1.0f;
    public float radius = .5f;

    public bool canMoveRight;
    public bool canMoveLeft;
    public bool canMove;

    public RaycastHit ledgeHit;
    

    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
    }
    
    private Vector3 climbPoint;
    private Collider ledge;
    private Collider[] hits;
    private Vector3 center;
    private void Update()
    {
        while (playerClimbScript.isClimbing)
        {
            center=transform.position+transform.forward*forwardPos+Vector3.up*upPos;
            hits = Physics.OverlapSphere(center, radius,playerClimbScript.ledgeLayer);
            if (hits.Length > 0)
            {
                canMove = true;
                ledge = hits[0];
                climbPoint = ledge.ClosestPoint(transform.position);
            }
            

            CheckSphere();

            break;
        }
    }

    public bool leftBtn;
    public bool rightBtn;
    public float ledgeMoveSpeed = 0.5f;
    float horizontalInp;
    float horizontalValue;

    void CheckSphere()
    {
        if (canMove)
        {
            
            //Right Hand Sphere check if it still ledge to move
            if (Physics.CheckSphere(climbPoint + transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer))
            {
                canMoveRight = true;
            
                rightBtn = Input.GetKey(KeyCode.D);
            }
            else 
            {
                rightBtn = false;
            
                leftBtn = Input.GetKey(KeyCode.A);
                canMoveRight = false;
            }
            
            // Left Hand Sphere check if it still ledge to move
            if (Physics.CheckSphere(climbPoint - transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer))
            {
                canMoveLeft = true;
            
                leftBtn = Input.GetKey(KeyCode.A);
            }
            else
            {
                leftBtn = false;
                canMoveLeft = false;
                rightBtn = Input.GetKey(KeyCode.D);
            }
        }

        // Horizontal Value
        if (leftBtn)
        {
            horizontalValue = -1;
        }
        else if (rightBtn)
        {
            horizontalValue = 1;
        }
        else
        {
            horizontalValue = 0;
        }
        playerClimbScript.animator.SetFloat("Speed", horizontalValue, 0.05f, Time.deltaTime);
        transform.position += transform.right * horizontalValue * ledgeMoveSpeed * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, radius);
        if (hits != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(climbPoint + transform.right * sphereGap, sphereRadius);
            Gizmos.DrawSphere(climbPoint - transform.right * sphereGap, sphereRadius);
            
            
        }
    }
}