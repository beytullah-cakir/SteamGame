using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NPCInteract : AInteractable
{
    public List<TextMeshProUGUI> npcChat;
    public List<Transform> walkPoints;
    public float waitTimeAtPoint = 2f;
    public Transform playerTransform;

    private int currentChatIndex = -1;
    private Coroutine showCoroutine;
    private NavMeshAgent agent;
    private bool isInteracting = false;
    private bool isWalking = false;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
    }

    private void Update()
    {
        
        if (!isInteracting && isWalking && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndWalk());
        }

        // NPC, etkileþim sýrasýnda oyuncuya döner
        if (isInteracting && playerTransform != null)
        {
            StopCoroutine(WaitAndWalk());
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        }
    }

    public override void Interact()
    {
        isInteracting = true;
        agent.isStopped = true;
        if (currentChatIndex + 1 >= npcChat.Count)
        {
            EndInteraction();
            return;
        }
       

        currentChatIndex++;

        // Önceki coroutine çalýþýyorsa durdur
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowString());
    }

    IEnumerator ShowString()
    {
        npcChat[currentChatIndex].gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
       
        npcChat[currentChatIndex].gameObject.SetActive(false);
    }

    private void EndInteraction()
    {
        currentChatIndex = -1;
        isInteracting = false;
        agent.isStopped = false;
        GoToNextPoint();
    }

    IEnumerator WaitAndWalk()
    {
        isWalking = false;
        yield return new WaitForSeconds(waitTimeAtPoint);
        GoToNextPoint();
    }

    void GoToNextPoint()
    {
        if (walkPoints.Count == 0) return;

        Transform randomPoint = walkPoints[Random.Range(0, walkPoints.Count)];
        agent.SetDestination(randomPoint.position);
        agent.isStopped = false;
        isWalking = true;
    }
}

