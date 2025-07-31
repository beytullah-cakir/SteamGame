using UnityEngine;
using UnityEngine.AI;

public class Pet : MonoBehaviour
{
    public Transform player; // Sürükleyip býrakýlacak

    public float followDistance = 2f;

    private NavMeshAgent agent;

    private Animator anm;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anm= GetComponent<Animator>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > followDistance)
        {
            Moving();
        }
        else
        {
             Stoping();
        }        
    }


    void Moving()
    {
        agent.SetDestination(player.position);
        anm.SetFloat("Vert", agent.velocity.magnitude);
    }

    void Stoping()
    {
        agent.ResetPath();
        anm.SetFloat("Vert", agent.velocity.magnitude);
    }
}
