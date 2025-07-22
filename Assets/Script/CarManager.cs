using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CarManager : AInteractable
{
    public GameObject player;
    public bool isDriving;
    public CinemachineCamera carFollow;
    private PrometeoCarController carController;
    public GameObject crosshair;

    public override void Interact()
    {
        if (!isDriving)
        {
            player.SetActive(false);
            crosshair.SetActive(true);
            player.transform.SetParent(transform);
            promptPrefab.SetActive(false);
            isDriving = true;
            carFollow.Priority = 20;
            carController.enabled = true;
        }
        else
        {
            isDriving = false;
            player.SetActive(true);
            player.transform.SetParent(null);
            carFollow.Priority = 0;
            carController.enabled = false;
            crosshair.SetActive(false);
        }
        
    }
    protected override void Start()
    {
        base.Start();
        carController = GetComponent<PrometeoCarController>();
        carController.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Interact();
        }
    }
}
