using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Objects")]
    public float interactionDistance = 2f;
    public KeyCode interactKey = KeyCode.E;
    public Transform objectCheck;
    private List<AInteractable> nearbyInteractables = new();



    [Header("Zipline")]
    [SerializeField] private float checkOffset = 1f;
    [SerializeField] private float chechkRadius = 2f;

    void Update()
    {
        InteractWithObject();
        InteractWithZipline();
    }

    public void InteractWithZipline()
    {
        if (Input.GetKeyDown(interactKey))
        {
            RaycastHit[] hits=Physics.SphereCastAll(objectCheck.position + Vector3.up * checkOffset, chechkRadius, Vector3.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Zipline")
                {
                    hit.collider.GetComponent<Zipline>().StartZipline(gameObject);

                }
            }
        }
    }


    public void InteractWithObject()
    {
        Collider[] colliders = Physics.OverlapSphere(objectCheck.position, interactionDistance);
        List<AInteractable> currentInteractables = new List<AInteractable>();

        foreach (Collider col in colliders)
        {
            AInteractable interactable = col.GetComponent<AInteractable>();
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
            AInteractable closest = GetClosestInteractable();
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

    private AInteractable GetClosestInteractable()
    {
        AInteractable closest = null;
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
