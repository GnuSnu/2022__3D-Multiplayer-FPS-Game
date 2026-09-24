using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    int SpawnTime;
    // Start is called before the first frame update
    void Start()
    {
        SpawnTime = (int)Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.Find("spine").position.y <= -5 || (int)Time.time - SpawnTime >= 20)
            Destroy(gameObject);
    }
}
