using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Agentc : MonoBehaviour
{
    [SerializeField] Transform[] targets;
    [SerializeField] NavMeshAgent agent1;
    private int index = 0;
    public Transform player;
    public LayerMask visionMask;
    private int chase = 0;
    private bool sees;

    private float lostTimer = 0f;
    public float lostTime = 2f; 
    
    
    // Start is called before the first frame update
    void Awake()
    {
        agent1 = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (targets.Length > 0)
            agent1.SetDestination(targets[index].position);
    }

    // Update is called once per frame
    void Update()
    {

        sees  = false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.8f, transform.forward, out RaycastHit hit, 5, visionMask))
        {
            if (hit.transform == player)
            {
                chase = 1;  
                sees = true;
              
            }
        }

        if (sees)
        {
            agent1.destination = player.position;
        }
        else if (chase==1)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= lostTime)
            {
                chase = 0;
                agent1.SetDestination(targets[index].position);
                lostTimer = 0f;
            }
            else
            {
                // sigue hacia última posición conocida
                agent1.destination = player.position;
            }
        }

        else if(!agent1.pathPending && agent1.remainingDistance <= 0.3)
            {
            
                index = (index + 1) % targets.Length;
                agent1.SetDestination(targets[index].position);
            }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.8f, transform.forward * 5);
    }
}
