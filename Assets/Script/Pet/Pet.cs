using UnityEngine;
using UnityEngine.AI;

public class Pet : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2f;

    [Header("Durumlar (States)")]
    public bool isSitting = false;
    private bool _isActuallySitting = false;

    [Header("Baktığın Yere Gitme (Y Tuşu)")]
    public LayerMask groundLayer;
    public float commandRange = 50f;
    public GameObject targetIndicator;

    private bool _isOrderedToPoint = false;
    private bool _reachedPoint = false; // Hedefe varıp varmadığını tutan yeni kilidimiz

    private NavMeshAgent agent;
    private Animator anm;
    private Camera mainCamera;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anm = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (targetIndicator != null) targetIndicator.SetActive(false);
    }

    void Update()
    {
        // Animasyon kontrolü
        if (agent.isOnNavMesh)
        {
            float currentSpeed = agent.isStopped ? 0f : agent.velocity.magnitude;
            anm.SetFloat("Vert", currentSpeed);
        }

        // T Tuşu: Geri Çağır / Oturt / Kaldır
        if (Input.GetKeyDown(KeyCode.T))
        {
            _isOrderedToPoint = false; // YALNIZCA T'ye basınca özel görev iptal olur
            _reachedPoint = false;
            if (targetIndicator != null) targetIndicator.SetActive(false);
            SetSitting(!isSitting);
        }

        // Y Tuşu: Baktığım yere git
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CommandGoToPoint();
        }

        // --- YAPAY ZEKA KARAR AĞACI ---

        // 1. DURUM: Baktığımız özel yere gitme emri
        if (_isOrderedToPoint)
        {
            // Hedefe henüz varmadıysa kontrol et
            if (!_reachedPoint)
            {
                // NavMesh hesaplaması bitmişse ve hedefe ulaştıysa:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                {
                    _reachedPoint = true; // Vardığını işaretle
                    Stoping(); // El frenini çek
                    if (targetIndicator != null) targetIndicator.SetActive(false);
                }
            }

            // ÇOK ÖNEMLİ: Hedefe varsa da, varmasa da T tuşuna basılana kadar
            // update döngüsü BURADA kesilir. Asla aşağıdaki takip kodlarına inmez.
            return;
        }

        // 2. DURUM: Köpek ZATEN oturmuş ise
        if (_isActuallySitting)
        {
            return;
        }

        // 3. DURUM: Normal Takip
        float distToPlayer = Vector3.Distance(GetFlatPos(transform.position), GetFlatPos(player.position));

        if (distToPlayer > followDistance)
        {
            if (anm != null) anm.SetBool("IsSitting", false);

            if (agent.isOnNavMesh)
            {
                if (agent.isStopped) agent.isStopped = false;

                if (Vector3.Distance(agent.destination, player.position) > 0.5f)
                {
                    agent.SetDestination(player.position);
                }
            }
        }
        else
        {
            Stoping();

            if (isSitting)
            {
                _isActuallySitting = true;
                if (anm != null) anm.SetBool("IsSitting", true);
            }
            else
            {
                if (anm != null) anm.SetBool("IsSitting", false);
            }
        }
    }

    public void SetSitting(bool sitState)
    {
        isSitting = sitState;

        if (!isSitting)
        {
            _isActuallySitting = false;
            if (anm != null) anm.SetBool("IsSitting", false);
            if (agent.isOnNavMesh) agent.isStopped = false;
        }
        else
        {
            Stoping();
        }
    }

    private void CommandGoToPoint()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, commandRange, groundLayer))
        {
            _isOrderedToPoint = true;
            _reachedPoint = false; // Yeni komut, henüz hedefe varmadı

            isSitting = false;
            _isActuallySitting = false;
            if (anm != null) anm.SetBool("IsSitting", false);

            if (targetIndicator != null)
            {
                targetIndicator.transform.position = hit.point;
                targetIndicator.SetActive(true);
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(hit.point);
            }
        }
    }

    private void Stoping()
    {
        if (agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    private Vector3 GetFlatPos(Vector3 pos)
    {
        return new Vector3(pos.x, 0, pos.z);
    }
}