using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*
 * NOTE: This codebase was written when I was 15 years old, prior to my involvement
 * in competitive programming and interest in code optimalization. 
 * My coding standards, software architecture, and optimization techniques have evolved since then. 
 */

public class Gun : MonoBehaviour
{
    public GameObject Particles;
    public Sprite RedDot;
    public Sprite Crosshair;
    public int Damage;
    public int HeadshotDamage;
    public int LoadedBullets;
    public int MagazineCapacity;
    public int BulletsInShot=1;
    public float minShootTime;
    public int BulletTravelLifetime = 400;
    public float ReloadAnimationTime=1;
    public bool sniperGun = false;
    public bool canZoom;
    [Header("Stuff not to change")]
    public float TimeSinceLastShot;
    public float ReloadTime;
    public bool isZoomed;
    public float zoomingTime;
    private const float ZOOM_TIME = 0.4f;
    private const float SNIPER_ZOOM_TIME = 0.15f;
    private Vignette vignette;
    public void PlayShootAnimation()
    {
        GetComponent<AudioSource>().Play();
        GetComponent<Animator>().SetTrigger("Shoot");
    }

    void Start()
    {
        if (transform.root.GetComponent<NetworkIdentity>().hasAuthority)
        {
            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().SetFloat("ReloadTime", 1 / ReloadAnimationTime);
            //GameObject.Find("Global Volume").GetComponent<Volume>().profile.TryGet(out vignette);
            VolumeProfile volumeProfile = GameObject.Find("Global Volume").GetComponent<Volume>()?.profile;
            if (!volumeProfile) throw new System.NullReferenceException(nameof(VolumeProfile));

            if (!volumeProfile.TryGet(out vignette)) throw new System.NullReferenceException(nameof(vignette));
        }
    }

    public void OnEnable()
    {
        GetComponent<Animator>().SetFloat("ReloadTime", 1 / ReloadAnimationTime);
        if(GameObject.Find("PlayerUI/RecoilIndicator").GetComponent<Image>().enabled)
            GameObject.Find("PlayerUI/RecoilIndicator").GetComponent<Image>().enabled = false;
        if(!sniperGun)
        {
            if (vignette == null)
            {
                VolumeProfile volumeProfile = GameObject.Find("Global Volume").GetComponent<Volume>()?.profile;
                if (!volumeProfile) throw new System.NullReferenceException(nameof(VolumeProfile));

                if (!volumeProfile.TryGet(out vignette)) throw new System.NullReferenceException(nameof(vignette));
            }
            if (vignette.rounded.value)
            {
                vignette.rounded.Override(false);
                vignette.intensity.Override(0.428f);
                vignette.smoothness.Override(0.546f);
            }

        }
        //if (Vector3.Distance(transform.localPosition, Vector3.zero) > 0.01f)
        //{
        //    transform.localPosition = new Vector3(0, 0, 0);
        //    transform.localRotation = new Quaternion(0, 0, 0,0);
        //}
    }

    public void FireGun()
    {
        LoadedBullets--;
        TimeSinceLastShot = minShootTime;
        GameObject partcl = Instantiate(Particles,transform.Find("main/Tip"));
        partcl.transform.localRotation = transform.Find("main").localRotation;
        partcl.transform.Rotate(new Vector3(9f, -3f, 0));
        UpdateUI();
        PlayShootAnimation();
    }

    public void ChangeCrosshair(int index)
    {
        if(index == 0)
        {
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().color = new Color(0, 0, 0, 1);
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().sprite = Crosshair;
            GameObject.Find("PlayerUI/Crosshair").GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
        else
        {
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().color = new Color(1, 1, 1, .7f);
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().sprite = RedDot;
            GameObject.Find("PlayerUI/Crosshair").GetComponent<RectTransform>().localScale = new Vector3(.15f, .15f, .15f);
        }
    }

    void UpdateUI()
    {
        GameObject.Find("PlayerUI/BulletsAmount").GetComponent<TextMeshProUGUI>().text = LoadedBullets.ToString();
        if(LoadedBullets == 0)
            GameObject.Find("PlayerUI/BulletsAmount").GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
        else
            GameObject.Find("PlayerUI/BulletsAmount").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
    }

    public void Reload()
    {
        TimeSinceLastShot = -1;
        LoadedBullets = MagazineCapacity;
        ReloadTime = ReloadAnimationTime;
        GetComponent<Animator>().SetTrigger("Reload");
    }

    void ManageZooming()
    {
        if (canZoom && isZoomed && sniperGun)
        {
            if (Camera.main.GetComponent<Camera>().fieldOfView == 28)
            {
                zoomingTime = 0;
            }
            else
            {
                if (zoomingTime < SNIPER_ZOOM_TIME)
                    zoomingTime += Time.deltaTime;
                ChangeCrosshair(1);
                vignette.rounded.Override(true);
                vignette.intensity.Override(Mathf.Lerp(0.428f, 1, zoomingTime / SNIPER_ZOOM_TIME));
                vignette.smoothness.Override(Mathf.Lerp(0.546f,0.1f, zoomingTime / SNIPER_ZOOM_TIME));
                Camera.main.GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.GetComponent<Camera>().fieldOfView, 28, zoomingTime / SNIPER_ZOOM_TIME);
                Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView, 8, zoomingTime / SNIPER_ZOOM_TIME);
            }

        }
        else if (canZoom && isZoomed)
        {
            ChangeCrosshair(0);
            if (Camera.main.GetComponent<Camera>().fieldOfView == 48)
            {
                zoomingTime = 0;
            }
            else
            {
                if (zoomingTime < ZOOM_TIME)
                    zoomingTime += Time.deltaTime;
                Camera.main.GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.GetComponent<Camera>().fieldOfView, 48, zoomingTime / ZOOM_TIME);
                Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView, 48, zoomingTime / ZOOM_TIME);
            }

        }
        else
        {
            ChangeCrosshair(0);
            if (!canZoom)
            {
                Camera.main.GetComponent<Camera>().fieldOfView = 88;
                Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = 88;
            }
            else if (!sniperGun)
            {
                if (Camera.main.GetComponent<Camera>().fieldOfView == 88)
                {
                    zoomingTime = 0;
                }
                else
                {
                    if (zoomingTime < ZOOM_TIME)
                        zoomingTime += Time.deltaTime;
                    Camera.main.GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.GetComponent<Camera>().fieldOfView, 88, zoomingTime / ZOOM_TIME);
                    Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView, 88, zoomingTime / ZOOM_TIME);
                }
            }
            else
            {
                if (Camera.main.GetComponent<Camera>().fieldOfView == 88)
                {
                    zoomingTime = 0;
                }
                else
                {
                    if (zoomingTime < SNIPER_ZOOM_TIME)
                        zoomingTime += Time.deltaTime;
                    vignette.rounded.Override(false);
                    vignette.intensity.Override(Mathf.Lerp(vignette.intensity.value, 0.428f, zoomingTime / SNIPER_ZOOM_TIME));
                    vignette.smoothness.Override(Mathf.Lerp(vignette.smoothness.value, 0.546f, zoomingTime / SNIPER_ZOOM_TIME));
                    Camera.main.GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.GetComponent<Camera>().fieldOfView, 88, zoomingTime / SNIPER_ZOOM_TIME);
                    Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView, 88, zoomingTime / SNIPER_ZOOM_TIME);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManageZooming();
        if (ReloadTime > 0)
        {
            GameObject.Find("PlayerUI/ReloadIndicator").GetComponent<Image>().enabled = true;
            GameObject.Find("PlayerUI/ReloadIndicator").GetComponent<Image>().fillAmount = ReloadTime / ReloadAnimationTime;
            ReloadTime -= Time.deltaTime;
        }
        else { 
            GameObject.Find("PlayerUI/ReloadIndicator").GetComponent<Image>().enabled = false;
            if (GameObject.Find("PlayerUI/BulletsAmount").GetComponent<TextMeshProUGUI>().text != LoadedBullets.ToString())
                UpdateUI();
        }

        if (TimeSinceLastShot > 0) TimeSinceLastShot -= Time.deltaTime;

        if (minShootTime >= 0.6f)
        {
            if (TimeSinceLastShot > 0&&LoadedBullets>0)
            {
                GameObject.Find("PlayerUI/RecoilIndicator").GetComponent<Image>().enabled = true;
                GameObject.Find("PlayerUI/RecoilIndicator").GetComponent<Image>().fillAmount = TimeSinceLastShot / minShootTime;
            }
            else GameObject.Find("PlayerUI/RecoilIndicator").GetComponent<Image>().enabled = false;

        }
        //if (Input.GetKeyDown(KeyCode.Mouse0))
           // PlayShootAnimation();
    }
}
