using Unity.VisualScripting;
using UnityEngine;

public class CollectableObject : AInteractable
{
    public Item item;

    public override void Interact()
    {
        InventoryManager.Instance.AddItem(item);
        Destroy(gameObject);
    }
}
