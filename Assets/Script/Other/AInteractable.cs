using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AInteractable: MonoBehaviour
{
    public GameObject promptPrefab;
    public bool canInteract;
    public float interactArea;
    public LayerMask playerLayer;
    protected Collider[] hitColliders;
    protected GameObject player;

    protected virtual void Start()
    {
        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        CheckPlayer();
        ShowPrompt(canInteract);
        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            Interact();
        }
    }
    protected virtual void CheckPlayer()
    {
        hitColliders = Physics.OverlapSphere(transform.position, interactArea, playerLayer);
        canInteract = hitColliders.Length > 0;
        if (canInteract) player = hitColliders[0].gameObject;
    }

    public void ShowPrompt(bool state)
    {
        if (promptPrefab != null)
            promptPrefab.SetActive(state);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactArea);
    }
    public abstract void Interact();
}
