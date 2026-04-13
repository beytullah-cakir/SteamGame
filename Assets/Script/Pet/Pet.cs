using UnityEngine;
using UnityEngine.AI;

public class Pet : MonoBehaviour
{
    public Transform player;

    public float followDistance = 2f;

    [Header("State")]
    public bool isSitting = false;
    private bool _isActuallySitting = false; // Köpeğin tam olarak oturup oturmadığının kilidi

    private NavMeshAgent agent;

    private Animator anm;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anm= GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetSitting(!isSitting);
        }

        // 1. DURUM: Köpek ZATEN oturmuş ve kilitlenmiş ise, biz ne kadar uzaklaşırsak uzaklaşalım artık takip etmesin, yerinden kıpırdamasın!
        if (_isActuallySitting)
        {
            Stoping();
            return;
        }

        // Y eksenindeki (Boy) farklılıkların mesafeyi bozmaması için Y koordinatları eşitmiş gibi hesapla
        Vector3 petPos = transform.position;
        Vector3 targetPos = player.position;
        petPos.y = 0; 
        targetPos.y = 0;
        
        // Köpek hedefe "followDistance" mesafesinden uzaksa...
        bool isFar = Vector3.Distance(petPos, targetPos) > followDistance;

        // 2. DURUM: "Otur" emri verilse bile henüz yanımıza ulaşmadıysa önce yanımıza koşsun
        if (isFar)
        {
            if (anm != null) anm.SetBool("IsSitting", false); 
            Moving();
        }
        else
        {
            Stoping();

            // 3. DURUM: Oyuncunun yanına geldiğinde, emir var ise artık oturabilir ve KENDİNİ KİLİTLEYEBİLİR.
            if (isSitting)
            {
                _isActuallySitting = true; // Ben oturdum, artık sahibim yürüse de asla yerimden kalkmayacağım!
                if (anm != null) anm.SetBool("IsSitting", true);
            }
            else
            {
                if (anm != null) anm.SetBool("IsSitting", false);
            }
        } 
    }

    // Bu komutu istediğin herhangi bir UI Button veya başka Script içerisinden çağırabilirsin
    public void SetSitting(bool sitState)
    {
        isSitting = sitState;
        
        // Kalk emri verildiğinde "Oturma Kilidini" sıfırla ki peşimizden koşabilsin
        if (!isSitting)
        {
            _isActuallySitting = false;
            if (anm != null) anm.SetBool("IsSitting", false);
        }
    }

    void Moving()
    {
        agent.SetDestination(player.position);
        anm.SetFloat("Vert", agent.velocity.magnitude);
    }

    void Stoping()
    {
        // Hedefi sıfırla ki durabilsin (Hata vermemesi için NavMesh üzerinde mi diye kontrol edilir)
        if (agent.isOnNavMesh) agent.ResetPath();
        anm.SetFloat("Vert", agent.velocity.magnitude);
    }
}
