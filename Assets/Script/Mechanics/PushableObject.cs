using System;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public bool isPushing;
    private Rigidbody rb;
    public float playerTargetOffset;
    private PlayerMovement playerMovement;
    private GameObject player;
    public bool isPlayer;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.E))
        {
            PushObejct();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player=other.gameObject;
            isPlayer = true;

        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            
            isPlayer = false;

        }
    }

    private void PushObejct()
    {
        playerMovement = player.transform.GetComponent<PlayerMovement>();

        if (!isPushing) // Push başlat
        {
            isPushing = true;
            playerMovement.isObjectPushing = true;
            player.transform.rotation = transform.rotation;
            player.transform.position=new Vector3(transform.position.x, player.transform.position.y, transform.position.z+playerTargetOffset);
            
            transform.SetParent(player.transform);
        }
        else // Push bırak
        {
            isPushing = false;
            playerMovement.isObjectPushing = false;
            transform.SetParent(null);
        }
    }
}