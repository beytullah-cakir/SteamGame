using UnityEngine;

public abstract class AInteractable: MonoBehaviour
{
    public GameObject promptPrefab;    

    protected virtual void Start()
    {
        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }
    }

    public void ShowPrompt(bool state)
    {
        if (promptPrefab != null)
            promptPrefab.SetActive(state);
    }
    public abstract void Interact();
}
