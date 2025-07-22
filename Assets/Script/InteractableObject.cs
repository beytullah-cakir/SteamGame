using Unity.VisualScripting;
using UnityEngine;

public class InteractableObject : AInteractable
{


    public override void Interact()
    {
       Destroy(gameObject);
    }
}
