using System.Collections;
using UnityEngine;

public class LedgeToRoofClimb : MonoBehaviour
{
    PlayerClimb playerClimb;
    ShimmyController shimmyController;
    RoofLedgeDetection roofLedgeDetection;
    RaycastHit ledgeToClimbHit;

    public bool foundLedgeToRoofClimb;
    private void Start()
    {
        playerClimb = GetComponent<PlayerClimb>();
        shimmyController = GetComponent<ShimmyController>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
    }

    private void Update()
    {
        if (playerClimb.isClimbing && !roofLedgeDetection.isDropingFromRoof && foundLedgeToRoofClimb)
        {
            if(Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(LedgeToClimb());
        }
       
        if (playerClimb.animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang To Crouch") && !playerClimb.animator.IsInTransition(0))
        {
            playerClimb.animator.MatchTarget(shimmyController.climbPoint , transform.rotation, AvatarTarget.RightFoot, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.41f, 0.87f);
        }
    }


    IEnumerator LedgeToClimb()
    {
        playerClimb.animator.CrossFade("Braced Hang To Crouch", 0);
        GetComponent<BoxCollider>().isTrigger = true;
        yield return new WaitForSeconds(playerClimb.animator.GetCurrentAnimatorStateInfo(0).length);
        playerClimb.isClimbing = false;
        GetComponent<BoxCollider>().isTrigger = false;
        playerClimb.playerState = PlayerState.NormalState;
    }
}