using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canva : MonoBehaviour
{
     public GameObject panelPerdiste; 

     void Start(){
         panelPerdiste.SetActive(false);
     }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            panelPerdiste.SetActive(true); 
          
        }
    }
}
