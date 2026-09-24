using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class DamageText : MonoBehaviour
{
    public Vector3 posToTarget;

    private Transform CameraTransform;
    [Range(0f, 1f)]
    public float sensitivity = 0.4f;
    Vector3 forwardVectorTowardsCamera;
    bool cameraLooking;
    float dotProductResult;

    void Start()
    {
        posToTarget = Camera.main.ScreenToWorldPoint(transform.position);
        CameraTransform = Camera.main.transform;
        StartCoroutine(ShadingEffect());
    }

    void Update()
    {
        if (cameraLooking)
        {
            if (Camera.main.ScreenToWorldPoint(transform.position) != posToTarget)
            {
                transform.position = Camera.main.WorldToScreenPoint(posToTarget);
            }
        }

        forwardVectorTowardsCamera = (CameraTransform.position - posToTarget).normalized;
        dotProductResult = Vector3.Dot(CameraTransform.forward, forwardVectorTowardsCamera);
        if (dotProductResult > sensitivity)
            cameraLooking = false;
        else
            cameraLooking = true;
    }
    private IEnumerator ShadingEffect()
    {
        while (transform.GetChild(0).GetComponent<TextMeshProUGUI>().color.a > 0.1f)
        {
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, transform.GetChild(0).GetComponent<TextMeshProUGUI>().color.a - 0.1f);
            transform.GetChild(0).transform.position = new Vector3(transform.GetChild(0).transform.position.x, transform.GetChild(0).transform.position.y + 2f, transform.GetChild(0).transform.position.z);
            yield return new WaitForSecondsRealtime(0.07f);
        }
        Destroy(gameObject);
    }
}
