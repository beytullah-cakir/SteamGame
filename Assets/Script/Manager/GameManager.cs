using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject inventoryPanel;
    public bool inventoryOpen;

    [Header("Indicator")]
    public GameObject indicator;

    private Camera mainCam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            OpenMenuScene();
    }

    public void OpenMenuScene()
    {
        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);

        Cursor.lockState = inventoryOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = inventoryOpen;

        Time.timeScale = inventoryOpen ? 0f : 1f;
    }
    public void UpdateIndicator(bool show, Transform target)
    {
        if (!show || target == null || inventoryOpen)
        {
            indicator.SetActive(false);
            return;
        }

        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);

        // Kamera arkasýndaysa gizle
        if (screenPos.z < 0)
        {
            indicator.SetActive(false);
            return;
        }

        indicator.SetActive(true);
        indicator.transform.position = screenPos;
    }

}
