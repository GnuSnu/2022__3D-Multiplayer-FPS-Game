using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float startTime;
    public float journeyLength;
    public float speed = 100F;
    public float trailLength;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public bool isMoving = false;
    private float timeStartDissapearing=0;
    private bool sniperGun;
    public float dissapearTransitionTime=1;
    public void Move(Vector3 start, Vector3 end, bool sniper)
    {
        GetComponent<TrailRenderer>().time = trailLength;
        sniperGun = sniper;
        isMoving = true;
        StartPos = start;
        EndPos = end;
        startTime = Time.time;
        journeyLength = Vector3.Distance(start, end);
    }

    void Start(){}

    void Update()
    {
        float distCovered = (Time.time - startTime) * speed;

        float fractionOfJourney = distCovered / journeyLength;

        transform.position = Vector3.Lerp(StartPos, EndPos, fractionOfJourney);
        if (fractionOfJourney >= 1 && sniperGun)
        {
            if (timeStartDissapearing == 0) timeStartDissapearing = Time.time;
            float dissTransition = Mathf.Lerp(1,0,(Time.time -timeStartDissapearing)/dissapearTransitionTime);
            Gradient gradient = new();
            if(dissTransition < 1)
            {
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.632f, 0.632f, 0.632f), 0.0f), new GradientColorKey(new Color(0.632f, 0.632f, 0.632f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(dissTransition, 0.0f), new GradientAlphaKey(dissTransition, 1.0f) }
                );
            }
            GetComponent<TrailRenderer>().colorGradient = gradient;
        }
    }
}