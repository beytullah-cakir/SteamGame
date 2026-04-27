using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject inventoryPanel;
    public bool inventoryOpen;

    [Header("Esc Menu")]
    [Tooltip("ESC tusuyla acilip kapanacak obje")]
    public GameObject escMenuObject;
    public bool escMenuOpen;

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

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleEscMenu();
    }

    public void ToggleEscMenu()
    {
        escMenuOpen = !escMenuOpen;
        if (escMenuObject != null) escMenuObject.SetActive(escMenuOpen);

        UpdateCursorAndTime();
    }

    public void OpenMenuScene()
    {
        inventoryOpen = !inventoryOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(inventoryOpen);

        UpdateCursorAndTime();
    }

    private void UpdateCursorAndTime()
    {
        bool anyMenuOpen = inventoryOpen || escMenuOpen;
        
        Cursor.lockState = anyMenuOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = anyMenuOpen;
        Time.timeScale = anyMenuOpen ? 0f : 1f;
    }

    public void UpdateIndicator(GameObject ui, bool show, Vector3 worldPos)
    {
        if (ui == null) return;

        if (!show || inventoryOpen)
        {
            ui.SetActive(false);
            return;
        }

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        // Hedef kameranın arkasındaysa gizle
        if (screenPos.z < 0)
        {
            ui.SetActive(false);
            return;
        }

        ui.SetActive(true);
        ui.transform.position = screenPos;
    }

    public void UpdateIndicator(GameObject ui, bool show, Transform target)
    {
        if (target == null)
        {
            UpdateIndicator(ui, false, Vector3.zero);
            return;
        }
        UpdateIndicator(ui, show, target.position);
    }
}
