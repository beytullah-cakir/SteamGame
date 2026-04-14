using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Requirements (Kimler Basabilir?)")]
    public bool acceptPlayer = true;
    public bool acceptPet = true;
    
    [Tooltip("Kutulara da duyarlı olsun istiyorsan aç")]
    public bool acceptBox = false;
    [Tooltip("Sadece acceptBox aktifse o renkteki kutuları kabul eder")]
    public BoxColor requiredColor;
    
    [Header("Action (Plakaya Basılınca Ne Olacak?)")]
    [Tooltip("İçine sürüklediğin objeyi plakanın üstündeyken çalıştırır, inince kapar. Boş bırakırsan sadece Unity Event olarak çalışır.")]
    public GameObject targetObjectToActivate;

    [Header("State (Read Only)")]
    public bool isActivated;

    [Header("Visual Feedback / Events (Opsiyonel)")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    // Aynı anda birden fazla doğru obje (örneğin hem pet hem player) gelirse diye sayı tutuyoruz
    private int _validObjectsInside = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTrigger(other))
        {
            _validObjectsInside++;
            
            // Plaka boşken ilk defa uygun biri bastı ise
            if (_validObjectsInside == 1) 
            {
                isActivated = true;
                
                // Hedef obşe varsa direk aç
                if (targetObjectToActivate != null) targetObjectToActivate.SetActive(true);
                
                OnActivated?.Invoke();
                Debug.Log("Plaka Aktif! Üstüne biri bastı.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTrigger(other))
        {
            _validObjectsInside--;
            
            // İçerideki tüm geçerli objeler çıktıysa
            if (_validObjectsInside <= 0)
            {
                _validObjectsInside = 0;
                isActivated = false;
                
                // Hedef obje varsa direkt kapat
                if (targetObjectToActivate != null) targetObjectToActivate.SetActive(false);
                
                OnDeactivated?.Invoke();
                Debug.Log("Plaka Deaktif. Üstü boşaldı.");
            }
        }
    }

    // Gelen objenin kabul edilebilir bir canlı/eşya olup olmadığını denetler
    private bool IsValidTrigger(Collider other)
    {
        // Tag'i Player olanlar (Karakter)
        if (acceptPlayer && other.CompareTag("Player")) return true;
        
        // Tag'i "Dog" olan VEYA üstünde/çocuklarında Pet scripti olan objeler (Köpek)
        if (acceptPet && other.GetComponentInParent<Pet>() != null) return true;

        // Üzerinde BoxPullable scripti olup da istenen renkte olan nesneler (Kutular)
        if (acceptBox)
        {
            BoxPullable box = other.GetComponent<BoxPullable>();
            if (box != null && box.boxColor == requiredColor) return true;
        }

        return false;
    }
}
