using UnityEngine;
using System.Collections.Generic;
using System.IO.Pipes;

public class PlayerInteraction : MonoBehaviour
{
   

    [Header("Zipline")]
    [SerializeField] private float checkOffset = 1f;
    [SerializeField] private float chechkRadius = 2f;
    [SerializeField] private string ziplineTag;


   



    void Update()
    {
        
        //InteractWithZipline();
        
    }


    // public void InteractWithZipline()
    // {
    //     if (Input.GetKeyDown(Input.GetKeyDown(KeyCode.E)))
    //     {
    //         RaycastHit[] hits = Physics.SphereCastAll(objectCheck.position + Vector3.up * checkOffset, chechkRadius, Vector3.up);
    //         foreach (RaycastHit hit in hits)
    //         {
    //             if (hit.collider.tag == ziplineTag)
    //             {
    //                 hit.collider.GetComponent<Zipline>().StartZipline(gameObject);
    //
    //             }
    //         }
    //     }
    // }
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(objectCheck.position, interactionDistance);
    // }
}
