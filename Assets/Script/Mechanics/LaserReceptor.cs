using UnityEngine;
using UnityEngine.Events;

public class LaserReceptor : MonoBehaviour
{
    public LaserColor requiredColor = LaserColor.Red;

    [Tooltip("Doðru renk geldiðinde tetiklenecek olaylar (Örn: Kapý Açma animasyonu)")]
    public UnityEvent OnActivated;

    [Tooltip("Lazer kesildiðinde tetiklenecek olaylar")]
    public UnityEvent OnDeactivated;

    private bool isHitThisFrame = false;
    private bool isCurrentlyActive = false;

    // Lazer scriptimiz bu fonksiyonu her frame çaðýracak
    public void ProcessLaserHit(LaserColor hitColor)
    {
        if (hitColor == requiredColor)
        {
            isHitThisFrame = true;
        }
    }

    private void LateUpdate()
    {
        // Eðer bu frame doðru renk lazer çarptýysa ve henüz aktif deðilsek aktif et
        if (isHitThisFrame && !isCurrentlyActive)
        {
            isCurrentlyActive = true;
            OnActivated?.Invoke();
        }
        // Eðer bu frame lazer çarpmadýysa ama aktifsek, deaktif et
        else if (!isHitThisFrame && isCurrentlyActive)
        {
            isCurrentlyActive = false;
            OnDeactivated?.Invoke();
        }

        // Sonraki frame için sýfýrla
        isHitThisFrame = false;
    }
}