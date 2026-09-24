using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveParticlesAfterEmission : MonoBehaviour
{
    void Update()
    {
        if(transform.childCount == 0) Destroy(gameObject);
    }
}