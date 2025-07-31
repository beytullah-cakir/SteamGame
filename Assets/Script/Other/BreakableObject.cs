using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakableObject : MonoBehaviour
{
    public float requiredPressTime = 1.5f;     // Kapýyý kýrmak için gerekli toplam süre
    public float pressIncreasePerTap = 0.1f;   // Her space basýþýnda eklenecek süre
    public float decayRate = 1f;               // Saniyede ne kadar süre azalýr (basýlmazsa)
    public float forceMultiplier = 10f;       // Kýrma kuvveti çarpaný
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
        Debug.Log("Kanca kapýya takýldý!");
    }

    void Update()
    {
        if (!isHooked || isBroken)
            return;

        // Her basýþta süre ekle
        if (Input.GetKeyDown(KeyCode.Space))
        {
            accumulatedTime += pressIncreasePerTap;
            Debug.Log("Basýþ! Ýlerleme: " + accumulatedTime.ToString("F2") + " / " + requiredPressTime);
        }

        // Her frame ilerlemeyi azalt (zamanla düþsün)
        if (accumulatedTime > 0f)
        {
            accumulatedTime -= decayRate * Time.deltaTime;
            accumulatedTime = Mathf.Max(0f, accumulatedTime);
        }

        // Yeterince süre birikmiþse kapýyý kýr
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
