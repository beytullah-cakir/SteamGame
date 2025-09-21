using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LadderClimb : MonoBehaviour
{
    private Animator animator;

    public float climbRate;

    private float climbTimer;

    public bool isClimbing;

    public bool isLadder;

    private PlayerMovement playerMovement;

    public TwoBoneIKConstraint leftHandTwoBonesConstraint;

    public TwoBoneIKConstraint rightHandTwoBonesConstraint;

    public int currentIndex;

    public List<Transform> ladderClimbPoints;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        leftHandTwoBonesConstraint.weight = 0;
        rightHandTwoBonesConstraint.weight = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LadderStart"))
        {
            // leftHandTwoBonesConstraint.data.target.SetParent(other.transform.parent);
            // rightHandTwoBonesConstraint.data.target.SetParent(other.transform.parent);
            isLadder = true;
            transform.rotation=other.transform.rotation;
        }

        if (other.CompareTag("LadderEnd"))
        {
            //StartCoroutine(EndClimb());
        }
    }

    void StartClimb()
    {
        Climb();
        isClimbing = true;
        animator.applyRootMotion = true;
        playerMovement.rb.useGravity = false;
        
        leftHandTwoBonesConstraint.weight = 1;
       
        rightHandTwoBonesConstraint.weight = 1;
    }

    private void Climb()
    {
        if (currentIndex + 1 >= ladderClimbPoints.Count) return;
        // world pozisyonunu koruyarak merdivene bağla
        rightHandTwoBonesConstraint.data.target.SetParent(ladderClimbPoints[currentIndex], false);
        leftHandTwoBonesConstraint.data.target.SetParent(ladderClimbPoints[currentIndex + 1], false);




        Vector3 handsMidPoint = (ladderClimbPoints[currentIndex].position +ladderClimbPoints[currentIndex + 1].position) / 2f;
                                 


        Vector3 newCharacterPos = new Vector3(transform.position.x,handsMidPoint.y - 1.0f, transform.position.z);
           
                
        
        transform.position = Vector3.Lerp(transform.position, newCharacterPos, 0.5f);

        currentIndex += 2;
    }

    IEnumerator EndClimb()
    {
        leftHandTwoBonesConstraint.weight = 0;
        rightHandTwoBonesConstraint.weight = 0;
        animator.SetTrigger("EndClimb");
        yield return new WaitForSeconds(4f);
        isClimbing = false;
        animator.applyRootMotion = false;
        playerMovement.rb.useGravity = true;
       
        currentIndex = 0;
    }

    private void Update()
    {
        if (isLadder && Input.GetKeyDown(KeyCode.E)) StartClimb();
        
        
        if (isClimbing && Input.GetKey(KeyCode.W))
        {
            climbTimer += Time.deltaTime;
            if (climbTimer >= climbRate)
            {
                Climb();
                climbTimer = 0;
            }
        }
    }

   
}