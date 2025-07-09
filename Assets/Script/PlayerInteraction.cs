using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 2f;
    public KeyCode interactKey = KeyCode.E;
    public Transform objectCheck;

    private List<InteractableObject> nearbyInteractables = new List<InteractableObject>();

    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(objectCheck.position, interactionDistance);
        List<InteractableObject> currentInteractables = new List<InteractableObject>();

        foreach (Collider col in colliders)
        {
            InteractableObject interactable = col.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                currentInteractables.Add(interactable);

                if (!nearbyInteractables.Contains(interactable))
                {
                    nearbyInteractables.Add(interactable);
                    interactable.ShowPrompt(true);
                }
            }
        }

        // Eski (uzaklaşılmış) etkileşimleri temizle
        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            if (!currentInteractables.Contains(nearbyInteractables[i]))
            {
                nearbyInteractables[i].ShowPrompt(false);
                nearbyInteractables.RemoveAt(i);
            }
        }

        if (nearbyInteractables.Count > 0)
        {
            InteractableObject closest = GetClosestInteractable();
            if (closest != null)
            {
                // ✅ Prompt kameraya baksın
                if (closest.promptPrefab != null)
                {
                    Vector3 direction = (Camera.main.transform.position - closest.promptPrefab.transform.position).normalized;
                    closest.promptPrefab.transform.rotation = Quaternion.LookRotation(-direction); // önemli: ters bakış
                }

                if (Input.GetKeyDown(interactKey))
                {
                    closest.Interact();
                }
            }
        }
    }

    private InteractableObject GetClosestInteractable()
    {
        InteractableObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var interactable in nearbyInteractables)
        {
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, interactable.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = interactable;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(objectCheck.position, interactionDistance);
    }
}
