using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CarManager : AInteractable
{
    public GameObject player;
    public bool isDriving;
    public CinemachineCamera carFollow;
    private PrometeoCarController carController;

    public override void Interact()
    {
        player.SetActive(false);
        player.transform.SetParent(transform);
        promptPrefab.SetActive(false);
        isDriving = true;
        carFollow.Priority = 20;
        carController.enabled = true;
    }
    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
        carController.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isDriving)
        {
            isDriving = false;
            player.SetActive(true);
            player.transform.SetParent(null);
            carFollow.Priority = 0;
            carController.enabled = false;
        }
    }
}
