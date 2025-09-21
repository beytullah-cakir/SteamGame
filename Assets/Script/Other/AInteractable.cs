using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AInteractable: MonoBehaviour
{
    public GameObject promptPrefab;
    public bool canInteract;
    public float interactArea;
    public LayerMask playerLayer;

    protected virtual void Start()
    {
        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            Interact();
        }
    }

    public void ShowPrompt(bool state)
    {
        if (promptPrefab != null)
            promptPrefab.SetActive(state);
    }
    public abstract void Interact();
}
