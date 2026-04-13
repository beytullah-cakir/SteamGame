using UnityEngine;

/// <summary>
/// Kendisine atanan tüm Pressure Plate'ler aktif olduğunda kapıyı yavaşça kaydırarak açar.
/// </summary>
public class PuzzleDoor : MonoBehaviour
{
    [Header("Puzzle Requirements")]
    [Tooltip("Açılması için hepsinin aktif olması gereken basınç plakaları")]
    public PressurePlate[] requiredPlates;

    [Header("Door Movement Settings")]
    [Tooltip("Kapı ne kadar yukarı/aşağı ya da sağ/sola açılsın?")]
    public Vector3 openOffset = new Vector3(0, 4f, 0); // Y ekseninde 4 birim yukarı
    public float moveSpeed = 3f;

    [Header("Door Animator (Opsiyonel)")]
    [Tooltip("Kapı için kayarak hareket yerine animasyon kullanıyorsan animatorü buraya verebilirsin")]
    public Animator doorAnimator;

    private Vector3 _closedPosition;
    private Vector3 _targetPosition;
    private bool _isDoorOpen;

    private void Start()
    {
        _closedPosition = transform.position;
        _targetPosition = _closedPosition;
    }

    private void Update()
    {
        CheckPlates();

        // Eğer Animator atanmadıysa, Vector3.Lerp ile yumuşak hareket sağla
        if (doorAnimator == null)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    private void CheckPlates()
    {
        // Hiç plaka atanmadıysa bir şey yapma
        if (requiredPlates == null || requiredPlates.Length == 0) return;

        bool allActive = true;
        foreach (var plate in requiredPlates)
        {
            if (!plate.isActivated)
            {
                allActive = false;
                break;
            }
        }

        if (allActive && !_isDoorOpen)     OpenDoor();
        else if (!allActive && _isDoorOpen) CloseDoor();
    }

    private void OpenDoor()
    {
        _isDoorOpen = true;
        _targetPosition = _closedPosition + openOffset;
        
        if (doorAnimator != null)
            doorAnimator.SetBool("IsOpen", true);
    }

    private void CloseDoor()
    {
        _isDoorOpen = false;
        _targetPosition = _closedPosition;

        if (doorAnimator != null)
            doorAnimator.SetBool("IsOpen", false);
    }
}
