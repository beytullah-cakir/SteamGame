using UnityEngine;
using System.Collections.Generic;

public class StairGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;
    public Transform endPoint;
    public GameObject stepPrefab;

    [Header("Settings")]
    public int stepCount = 10;

    

    void Start()
    {
        GenerateStairs();
    }

    void GenerateStairs()
    {
        if (stepCount < 2 || stepPrefab == null || startPoint == null || endPoint == null)
        {
            Debug.LogWarning("Eksik veya hatali ayar!");
            return;
        }

        

        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        Vector3 stepOffset = (endPos - startPos) / (stepCount - 1);

        for (int i = 0; i < stepCount; i++)
        {
            Vector3 stepPosition = startPos + stepOffset * i;

            GameObject stepGO = Instantiate(
                stepPrefab,
                stepPosition,
                stepPrefab.transform.rotation,
                transform
            );
        }
    }
}
