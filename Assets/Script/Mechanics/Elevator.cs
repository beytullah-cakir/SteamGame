using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public bool canMove;
    public float speed;
    public int startPoint;
    public Transform[] points;
    private int targetIndex;
    private int lastPosIndex;
    private GameObject player;
    private bool isPlayer;

    void Start()
    {
        transform.position = points[startPoint].position;
        lastPosIndex = points.Length - 1;
    }

    void Update()
    {
        // J tuşuna basınca sıradaki hedefe git
        if (Input.GetKeyDown(KeyCode.J) && isPlayer)
        {
            canMove = !canMove;
            if (targetIndex != lastPosIndex)
            {
                targetIndex++;
            }
            else
            {
                targetIndex--;
            }
        }

        if (canMove)
            Move();
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            points[targetIndex].position,
            speed * Time.deltaTime
        );
        player.transform.SetParent(transform);

        // Hedefe ulaştı mı?
        if (Vector3.Distance(transform.position, points[targetIndex].position) < 0.01f)
        {
            canMove = false;
            player.transform.SetParent(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject;
            isPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            player = null;
            isPlayer = false;
        }
    }
}