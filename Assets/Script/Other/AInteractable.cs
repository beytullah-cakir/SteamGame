using UnityEngine;

public abstract class AInteractable: MonoBehaviour
{
    public string objectName = "Etkileþimli Obje";
    public GameObject promptPrefab;

    public Vector3 promptPos;

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
