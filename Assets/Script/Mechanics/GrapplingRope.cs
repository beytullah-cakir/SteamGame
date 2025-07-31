using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    public float springValue;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;
    public Grappling grapplingGun;
    public HookSystem hookSystem;
    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;
    private float ropeAnimationTime;
    public float ropeStraightenSpeed = 5f;
    private Vector3 lastGrapplePoint;


    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    //Called after Update
    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!grapplingGun.IsGrappling() && !hookSystem.IsSwinging())
        {
            currentGrapplePosition = grapplingGun.gunTip.position;
            ropeAnimationTime = 0f; 
            if (lr.positionCount > 0) lr.enabled = false;                
            return;
        }
        else
        {
            lr.enabled = true;
            lr.positionCount = quality + 1;
        }

        var grapplePoint = grapplingGun.IsGrappling()? grapplingGun.GetGrapplePoint() : hookSystem.IsSwinging() ? hookSystem.GetSwingingPoint(): Vector3.zero;
       
        if (grapplePoint != lastGrapplePoint)
        {
            ropeAnimationTime = 0f;
            lastGrapplePoint = grapplePoint;
        }

        ropeAnimationTime += Time.deltaTime * ropeStraightenSpeed;

        
        var gunTipPosition = grapplingGun.gunTip.position;
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
    }

}