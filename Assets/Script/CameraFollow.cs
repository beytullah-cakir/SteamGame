using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform target; // Araba
    public Vector3 offset = new Vector3(0, 4, -8);
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.2f;

    private Vector3 currentVelocity;

    void Update()
    {
        if (!target) return;

        // 1. Hedef pozisyonu hesapla (arabanın arkasında)
        Vector3 desiredPosition = target.TransformPoint(offset);

        // 2. Kamerayı pozisyon olarak yumuşak şekilde takip ettir
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

        // 3. Kamera rotasyonunu arabanın yönüne yumuşak şekilde uydur
        Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up * 1.5f - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
    }

}
