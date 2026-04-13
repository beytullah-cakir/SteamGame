using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("Bu plakanın aktif olması için kutunun sahip olması gereken renk")]
    public BoxColor requiredColor;
    
    [Header("State (Read Only)")]
    public bool isActivated;

    [Header("Visual Feedback / Events (Opsiyonel)")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    // Aynı anda birden fazla doğru renkli aynı tipte kutu gelirse diye sayı tutuyoruz
    private int _validObjectsInside = 0;

    private void OnTriggerEnter(Collider other)
    {
        BoxPullable box = other.GetComponent<BoxPullable>();
        
        // Giren obje BoxPullable içeriyorsa ve rengi bizim plakaya uyuyorsa
        if (box != null && box.boxColor == requiredColor)
        {
            _validObjectsInside++;
            
            // Eğer daha önceden içeride başka obje yoksa ve ilk kez geldi ise
            if (_validObjectsInside == 1) 
            {
                isActivated = true;
                OnActivated?.Invoke();
                Debug.Log($"Plaka ({requiredColor}) Aktif!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BoxPullable box = other.GetComponent<BoxPullable>();
        
        // Çıkan obje doğru renkte bir kutuysa
        if (box != null && box.boxColor == requiredColor)
        {
            _validObjectsInside--;
            
            // İçerideki bütün doğru kutular çıktıysa
            if (_validObjectsInside <= 0)
            {
                _validObjectsInside = 0;
                isActivated = false;
                OnDeactivated?.Invoke();
                Debug.Log($"Plaka ({requiredColor}) Deaktif.");
            }
        }
    }
}
