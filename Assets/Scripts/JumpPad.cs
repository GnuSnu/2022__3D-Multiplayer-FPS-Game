using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.CompareTag("Ragdoll"))
            other.transform.root.Find("spine").GetComponent<Rigidbody>()?.AddForce(new Vector3(0, 20000, 0));
        else
            other.transform.root.GetComponent<Rigidbody>()?.AddForce(new Vector3(0,1500,0));
    }
}
