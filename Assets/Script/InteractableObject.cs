using Unity.VisualScripting;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName = "Etkileþimli Obje";
    public GameObject promptPrefab;
    
    public Vector3 promptPos;

    void Start()
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

    public void Interact()
    {
        Destroy(gameObject);
    }
}
