using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class MiniMapPathDrawer : MonoBehaviour
{
    public Transform player;
    public Transform target;
    private LineRenderer lineRenderer;
    private NavMeshPath path;
    private float timer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        path = new NavMeshPath();  
    }

    void Update()
    {
        UpdatePath();
        DrawPath();
        DeletePath();
    }

    void UpdatePath()
    {
        if (player == null || target == null) return;
        NavMesh.CalculatePath(player.position, target.position, NavMesh.AllAreas, path);        
    }

    void DrawPath()
    {
        if (path == null || path.corners.Length < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 pos = path.corners[i];
            pos.y = player.position.y + 0.5f;
            lineRenderer.SetPosition(i, pos);
        }
    }

    void DeletePath()
    {
        if (target == null) return;
        if(Vector3.Distance(player.position, target.position) <5f)
        {
            lineRenderer.positionCount = 0;
            target = null;
            path.ClearCorners();
        }
    }
}
