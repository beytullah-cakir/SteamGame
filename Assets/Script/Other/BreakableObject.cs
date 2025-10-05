using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakableObject : MonoBehaviour
{
    public float requiredPressTime = 1.5f;     // Kap�y� k�rmak i�in gerekli toplam s�re
    public float pressIncreasePerTap = 0.1f;   // Her space bas���nda eklenecek s�re
    public float decayRate = 1f;               // Saniyede ne kadar s�re azal�r (bas�lmazsa)
    public float forceMultiplier = 10f;       // K�rma kuvveti �arpan�
    private float accumulatedTime = 0f;
    private bool isHooked = false;
    private bool isBroken = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void AttachHook()
    {
        isHooked = true;
        Debug.Log("Kanca kap�ya tak�ld�!");
    }

    void Update()
    {
        if (!isHooked || isBroken)
            return;

        // Her bas��ta s�re ekle
        if (Input.GetMouseButtonDown(1))
        {
            accumulatedTime += pressIncreasePerTap;
            Debug.Log("Bas��! �lerleme: " + accumulatedTime.ToString("F2") + " / " + requiredPressTime);
        }

        // Her frame ilerlemeyi azalt (zamanla d��s�n)
        if (accumulatedTime > 0f)
        {
            accumulatedTime -= decayRate * Time.deltaTime;
            accumulatedTime = Mathf.Max(0f, accumulatedTime);
        }

        // Yeterince s�re birikmi�se kap�y� k�r
        if (accumulatedTime >= requiredPressTime)
        {
            StartCoroutine(BreakObject());
        }
    }

    IEnumerator BreakObject()
    {
        isBroken = true;
        Vector3 direction= ( Camera.main.transform.position- transform.position).normalized;
        rb.AddForce(direction*forceMultiplier,ForceMode.Impulse);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // veya animasyon oynat
    }
}
