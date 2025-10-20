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

    
}