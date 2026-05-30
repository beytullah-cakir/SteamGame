using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    public LaserColor startingColor = LaserColor.Red;
    public int maxBounces = 10;
    public float maxDistance = 100f;
    public Material laserMaterial;
    public float laserWidth = 0.1f;

    private List<LineRenderer> laserSegments = new List<LineRenderer>();
    private GameObject segmentContainer;

    void Start()
    {
        // LineRenderer'larý temiz tutmak için bir alt obje oluþturuyoruz
        segmentContainer = new GameObject("LaserSegments");
        segmentContainer.transform.SetParent(transform);
    }

    void Update()
    {
        DrawLaser();
    }

    void DrawLaser()
    {
        int currentSegmentIndex = 0;
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward;
        LaserColor currentColor = startingColor;

        // Iþýn döngüsü
        for (int i = 0; i < maxBounces; i++)
        {
            LineRenderer currentLine = GetOrCreateLineRenderer(currentSegmentIndex);
            SetLineColor(currentLine, currentColor);
            currentLine.SetPosition(0, currentPosition);

            Ray ray = new Ray(currentPosition, currentDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                currentLine.SetPosition(1, hit.point);
                currentSegmentIndex++;

                if (hit.collider.CompareTag("Mirror"))
                {
                    // Aynadan sekme hesabý
                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point + currentDirection * 0.01f; // Ýç içe geçmeyi önlemek için minik bir offset
                }
                else if (hit.collider.CompareTag("Prism"))
                {
                    // Prizmadan geçiþ ve renk deðiþimi
                    LaserPrism prism = hit.collider.GetComponent<LaserPrism>();
                    if (prism != null) currentColor = prism.outputColor;

                    // Yön deðiþmiyor, doðrudan içinden geçiyor
                    currentPosition = hit.point + currentDirection * 0.01f;
                }
                else if (hit.collider.CompareTag("Receptor"))
                {
                    // Reseptöre çarptý, sinyal gönder ve ýþýný bitir
                    LaserReceptor receptor = hit.collider.GetComponent<LaserReceptor>();
                    if (receptor != null) receptor.ProcessLaserHit(currentColor);
                    break;
                }
                else
                {
                    // Normal bir duvara çarptý, ýþýn biter
                    break;
                }
            }
            else
            {
                // Hiçbir þeye çarpmadýysa sonsuza (maxDistance) uzat ve bitir
                currentLine.SetPosition(1, currentPosition + currentDirection * maxDistance);
                currentSegmentIndex++;
                break;
            }
        }

        // Kullanýlmayan fazladan LineRenderer segmentlerini gizle
        for (int i = currentSegmentIndex; i < laserSegments.Count; i++)
        {
            laserSegments[i].gameObject.SetActive(false);
        }
    }

    // Ýhtiyaç oldukça LineRenderer oluþturur (Object Pooling mantýðý)
    LineRenderer GetOrCreateLineRenderer(int index)
    {
        if (index < laserSegments.Count)
        {
            laserSegments[index].gameObject.SetActive(true);
            return laserSegments[index];
        }

        GameObject lrObj = new GameObject("Segment_" + index);
        lrObj.transform.SetParent(segmentContainer.transform);
        LineRenderer lr = lrObj.AddComponent<LineRenderer>();
        lr.material = laserMaterial;
        lr.startWidth = laserWidth;
        lr.endWidth = laserWidth;
        lr.positionCount = 2;
        laserSegments.Add(lr);
        return lr;
    }

    // Renge göre LineRenderer'ý boyar
    void SetLineColor(LineRenderer lr, LaserColor color)
    {
        Color unityColor = Color.white;
        switch (color)
        {
            case LaserColor.Red: unityColor = Color.red; break;
            case LaserColor.Green: unityColor = Color.green; break;
            case LaserColor.Blue: unityColor = Color.blue; break;
        }
        lr.startColor = unityColor;
        lr.endColor = unityColor;
        // Eðer material HDR destekliyorsa (Bloom efekti için) color deðerini þiddetlendirebilirsin.
        lr.material.color = unityColor;
    }
}