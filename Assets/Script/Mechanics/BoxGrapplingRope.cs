using UnityEngine;

/// <summary>
/// Kanca kutuya takıldığı anda doğrudan düz bir ip çizer.
/// Animasyon/dalga yoktur.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BoxGrapplingRope : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Karakterdeki BoxPullController")]
    public BoxPullController boxPullController;

    private LineRenderer _lr;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.enabled = false;
    }

    private void LateUpdate()
    {
        if (boxPullController == null) return;

        if (!boxPullController.isHooked)
        {
            _lr.enabled = false;
            return;
        }

        Transform tip = boxPullController.gunTip;
        Vector3 start = tip != null ? tip.position : boxPullController.transform.position;
        Vector3 end   = boxPullController.GetHookPoint();

        _lr.enabled = true;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
    }
}
