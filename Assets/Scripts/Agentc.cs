using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agentc : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] NavMeshAgent agent1;
    
    // Start is called before the first frame update
    void Awake()
    {
        agent1 = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent1.SetDestination(target.position);
    }
}
