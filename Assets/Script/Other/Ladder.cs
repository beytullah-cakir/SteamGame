using System;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float playerOffset;
    public bool isPlayer;
    public GameObject player;
    
    
    


    private void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.E))
        {
            Climb();
        }
    }

    private void Climb()
    {
        player.transform.rotation = transform.rotation;
        player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z+playerOffset);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.anm.SetTrigger("Climb");
        playerMovement.isLadderClimbing = true;
        
    }


    

    // public override void Interact()
    // {
    //     PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
    //
    //     // Tırmanma animasyonunu başlat
    //     playerMovement.anm.SetTrigger("Climb");
    //     
    //
    //     
    //     if (playerMovement.anm.GetCurrentAnimatorStateInfo(0).IsName("Climb") && !playerMovement.anm.IsInTransition(0))
    //     {
    //         Vector3 targetPosition = transform.position + offset;
    //         Quaternion targetRotation = transform.rotation;
    //         playerMovement.anm.MatchTarget(targetPosition, targetRotation, AvatarTarget.Root,
    //             new MatchTargetWeightMask(Vector3.one,0), .1f, .9f);
    //     }
    //
    //
    //     // Karakterin yönünü merdiven yönüne çevir
    //     player.transform.rotation = transform.rotation;
    //
    //     // Ladder state aktif
    //     playerMovement.isLadderClimbing = true;
    // }
}