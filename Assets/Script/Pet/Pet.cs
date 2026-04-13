using UnityEngine;
using UnityEngine.AI;

public class Pet : MonoBehaviour
{
    public Transform player;

    public float followDistance = 2f;

    [Header("State")]
    public bool isSitting = false;

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

        // Eğer köpek oturuyorsa hareket etmesi ve algoritmanın çalışması engellenir
        if (isSitting)
        {
            Stoping();
            return;
        }

        // Normal Takip Mantığı
        if (Vector3.Distance(transform.position, player.position) > followDistance)
        {
            Moving();
        }
        else
        {
             Stoping();
        }        
    }

    // Bu komutu istediğin herhangi bir UI Button veya başka Script içerisinden çağırabilirsin
    public void SetSitting(bool sitState)
    {
        isSitting = sitState;
        
        if (isSitting)
        {
            Stoping(); // Anında olduğu yerde durdurur
            if (anm != null) anm.SetBool("IsSitting", true); // Köpeğin Animator'ünde IsSitting (bool) değeri varsa tetikler
        }
        else
        {
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
