using UnityEngine;

public class MiniMapCameraFollow : MonoBehaviour
{
    public GameObject player;

    private void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y+30, player.transform.position.z);
    }
}
