using System.Runtime.CompilerServices;
using UnityEngine;

public class CarHookSystem : MonoBehaviour
{
    public float hookRange = 5f;
    public LayerMask doorLayer;
    private LineRenderer lineRenderer;
    public GameObject hookGameobject;
    private Transform hookedDoor;


    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        lineRenderer = hookGameobject.GetComponent<LineRenderer>();
        lineRenderer.enabled = false; 
        lineRenderer.positionCount = 2;
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.position, cam.forward, out hit, hookRange, doorLayer))
            {
                BreakableObject door = hit.collider.GetComponent<BreakableObject>();
                if (door != null)
                {
                    door.AttachHook();
                    hookedDoor = door.transform;                   
                    lineRenderer.enabled = true;
                    lineRenderer.positionCount = 2;
                }
            }
        }

        CheckLineRenderer();
    }

    private void CheckLineRenderer()
    {
        if (hookedDoor != null)
        {
            lineRenderer.SetPosition(0, hookGameobject.transform.position);
            lineRenderer.SetPosition(1, hookedDoor.position);
        }
        else
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }
    }
}
