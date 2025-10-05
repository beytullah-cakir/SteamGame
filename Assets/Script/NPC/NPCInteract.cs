using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCInteract : AInteractable
{
    public TextMeshProUGUI textMesh;
    public List<Transform> walkPoints; // NPC'nin yürüyüş yolları
    public float waitTimeAtPoint = 2f; // Her noktada bekleme süresi
    
    private int currentChatIndex = 0; // Şu anki metin index'i
    private Coroutine showCoroutine;
    private NavMeshAgent agent;
    public bool isInteracting = false; // NPC'nin etkileşimde olup olmadığı
    private bool isWalking = false; // NPC'nin yürüyüp yürümeyeceği
    public bool isTalking = false;
    private List<string> npcChats;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
        npcChats = new List<string>();
        npcChats.Add("aaaaa");
        npcChats.Add("bbbbb");
        npcChats.Add("cccccc");
    }

    protected override void Update()
    {
        base.Update();
        
        ShowPrompt(canInteract && !isInteracting && !isTalking);

        // Oyuncu etkileşimde değilse ve NPC bir noktada bekliyorsa, yeni noktaya git
        if (!isInteracting && isWalking && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndWalk());
        }

        // NPC, etkileşim sırasında oyuncuya döner
        if (isInteracting && !isTalking)
        {
            StopCoroutine(WaitAndWalk());
        }

        // Enter tuşuna basıldığında yeni metin göster
        if (isInteracting && Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextText();
        }
    }

    protected override void CheckPlayer()
    {
        base.CheckPlayer();
        PlayerMovement.Instance.isInteractingWithNPC = isInteracting;
        canInteract = canInteract && !isInteracting && !isTalking;
    }


    public override void Interact()
    {
        isInteracting = true;
        agent.speed = 0;
        textMesh.text = npcChats[0]; // İlk cümleyi direkt göster
        currentChatIndex = 1; 
    }

    private void ShowNextText()
    {
        if (currentChatIndex > npcChats.Count - 1)
        {
            EndInteraction(); 
            return;
        }
        print(currentChatIndex);
        textMesh.text = npcChats[currentChatIndex];
        currentChatIndex++;

    }


    private void EndInteraction()
    {
        isInteracting = false;
        textMesh.text = "";
        agent.speed = 3.5f; // NPC'yi tekrar yürütmeye başla
        isTalking = true; // Konuşma bitince NPC'yi hareket ettir
        GoToNextPoint(); // NPC'nin yolculuğuna devam et
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
        agent.isStopped = false; // Yürümeye devam et
        isWalking = true;
    }

    
}