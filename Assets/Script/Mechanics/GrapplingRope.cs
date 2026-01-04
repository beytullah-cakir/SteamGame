using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    public float springValue;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;
    private HookManager hookManager;
    public int quality;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;
    private float ropeAnimationTime;
    public float ropeStraightenSpeed = 5f;
    private Vector3 lastGrapplePoint;


    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        hookManager=transform.parent.GetComponent<HookManager>();
        
    }

    //Called after Update
    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!hookManager.canGrapple)
        {
            currentGrapplePosition = hookManager.gunTip.position;
            ropeAnimationTime = 0f; 
            if (lr.positionCount > 0) lr.enabled = false;                
            return;
        }
        else
        {
            lr.enabled = true;
            lr.positionCount = quality + 1;
        }

        var grapplePoint = hookManager.canGrapple ? hookManager.GetGrapplePoint() : Vector3.zero;
       
        if (grapplePoint != lastGrapplePoint)
        {
            ropeAnimationTime = 0f;
            lastGrapplePoint = grapplePoint;
        }

        ropeAnimationTime += Time.deltaTime * ropeStraightenSpeed;

        
        var gunTipPosition = hookManager.gunTip.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;

            // ⬇️ Dalga etkisi zamanla azalır
            float waveFactor = 1 - Mathf.Clamp01(ropeAnimationTime); // 1 → 0
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * springValue *
                         affectCurve.Evaluate(delta) * waveFactor;

            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
        float distanceToTarget = Vector3.Distance(currentGrapplePosition, grapplePoint);

        // 0.2f gibi küçük bir eşik değeri (threshold) kullanıyoruz
        if (distanceToTarget < 0.2f)
        {
            // Eğer karakter zaten hareket etmiyorsa ve bu bir Grapple işlemiyse çalıştır
            if (hookManager.canGrapple && !hookManager.isMovingToGrapplePoint)
            {
                hookManager.isMovingToGrapplePoint = true;
                hookManager.rb.useGravity = false;
            }
        }
    }

}