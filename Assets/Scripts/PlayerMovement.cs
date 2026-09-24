using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Mirror;
using System.Net;

/*
 * NOTE: This codebase was written when I was 15 years old, prior to my involvement
 * in competitive programming and interest in code optimalization. 
 * My coding standards, software architecture, and optimization techniques have evolved since then. 
 */



public class PlayerMovement : NetworkBehaviour 
{
    private GameObject PauseUI;
    private GameObject PauseIP;
    private GameObject PauseShowIP;
    public static Transform ServersContent;
    private bool finishedStartPhase;
    private bool paused = false;
    private string externalIP="";
    public int itemSlot = 1;
    private float speed = 10;
    private const float crouchSpeed = 4;
    private const float normalSpeed = 10;
    //private const float sprintSpeed = 12; -clipping walls
    private bool LockedCursor = true;
    public int FPS = 60;
    public List<Vector3[]> MovementBuffer = new List<Vector3[]>();
    public GameObject CameraMountPoint1stPerson;
    public GameObject CameraMountPoint3rdPerson;
    public GameObject ragdoll;
    private Vector3 VectorToMove=Vector3.zero;
    private Vector3 VectorToRotate = Vector3.zero;
    public Vector3 MoveVector;
    public float MouseFrequency=1.75f;
    private float MouseX = 0;
    private float MouseY = 0;
    private GameObject Coll_check;
    public GameObject HitParticle;
    public GameObject BulletTrail;
    public GameObject SniperBulletTrail;
    public int Kills = 0;
    public int Deaths = 0;
    public bool isAlive = true;
    private Transform Killer;
    public bool UseClientPhysics=false;
    private float timer;
    public int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 60f;
    private const int BUFFER_SIZE = 1024; 
    public int PlayersCount=0;
    public readonly static List<PlayerMovement> PlayerConnections = new();
    public override void OnStopClient()
    {
        if(isLocalPlayer)
            GameObject.Find("MenuUI").GetComponent<MenuManager>().GoToPanel1();
        print(PlayerConnections.Contains(this));
        try
        {
            PlayerConnections.Remove(this);
        }
        catch { }
        print("LEFT");
        if (isLocalPlayer)
        {
            Camera.main.transform.parent = null;
            try
            {
                Coll_check.transform.position = GameObject.Find("SPAWN_POINT").transform.position;
            }
            catch { }
        }
        Cursor.lockState = CursorLockMode.None;
        Camera.main.transform.SetPositionAndRotation(new Vector3(0, 10, 0), Quaternion.Euler(31.8f, 0, 0));
        DontDestroyOnLoad(Camera.main.transform.gameObject);
    }

    public override void OnStartClient(bool connectionCheck)
    {
        print(connectionCheck);
        List<PlayerMovement> GhostPlayersList = new();
        foreach (var item in PlayerConnections)
        {
            try
            {
                print(item.GetComponent<NetworkIdentity>().netId);
            }
            catch
            {
                GhostPlayersList.Add(item);
            }
        }
        foreach (var item in GhostPlayersList)
        {
            PlayerConnections.Remove(item);
        }
        if (connectionCheck && isLocalPlayer && !isServer)
        {
            try
            {
                print("WE CHECKED CONNECTION OGMOGMGMOGOMOGMODFGMOEFGOPMEDFGIONUIOEDRBFGHUIERFGBH");
                GameObject.Find("MenuUI").GetComponent<MenuManager>().ActiveServers.Add(NetworkManager.singleton.networkAddress);
                GameObject.Find("MenuUI").GetComponent<MenuManager>().CurrentConnectionCheckIP = "";
                for (int i = 0; i < ServersContent.childCount; i++)
                {
                    if(ServersContent.GetChild(i).GetComponent<ServerObjScript>().IP == NetworkManager.singleton.networkAddress)
                    {
                        ServersContent.GetChild(i).Find("ConnectionIndicator").GetComponent<Image>().color = Color.green;
                        //ServersContent.GetChild(i).Find("Name").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Underline;
                    }
                }
                GameObject.Find("MenuUI").GetComponent<MenuManager>().OfflineServers.Remove(NetworkManager.singleton.networkAddress);
                NetworkManager.singleton.StopClient();
                GameObject.Find("MenuUI").GetComponent<MenuManager>().connectionCheck = true;
                GameObject.Find("MenuUI").GetComponent<MenuManager>().ButtonLeave();
            }
            catch(Exception e)
            {
                print("bruh, theres an error when checking connection: " + e);
            }

        }
        else
        {
            print("lul");
            if (isLocalPlayer)
                transform.position = GameObject.FindGameObjectsWithTag("SPAWN_POINT")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("SPAWN_POINT").Length)].transform.position;
            PlayerConnections.Add(this);
        }
    }
    [ClientCallback]
    [Obsolete]
    IEnumerator StartPhase()
    {
        if (isLocalPlayer)
        {
            if(!isServer)
                yield return new WaitForSecondsRealtime(0.2f);
            if (!PlayerConnections.Contains(this))
            {
                transform.position = new Vector3(UnityEngine.Random.Range(1000, 3000), UnityEngine.Random.Range(1000, 3000), UnityEngine.Random.Range(1000, 3000));
                yield break;
            }
            print("not checking connection");
            gameObject.tag = "LocalPlayer";
            transform.Find("metarig/spine/spine.001/spine.002/spine.003/shoulder.L/upper_arm.L").gameObject.tag = "LocalPlayer";
            transform.Find("metarig/spine/spine.001/spine.002/spine.003/shoulder.R/upper_arm.R").gameObject.tag = "LocalPlayer";
            foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (item.transform.root != transform)
                    item.transform.root.GetComponent<PlayerMovement>().enabled = false;
            }

            Coll_check =  GameObject.Find("COLLISION_CHECK");
            Application.targetFrameRate = FPS;
            minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
            Cursor.lockState = CursorLockMode.Locked;
            SetCamera(CameraMountPoint1stPerson.transform);
            TurnOffRigidbodyOnServer();
            SendKillsDeath_CMD();
            foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (item.GetComponent<PlayerMovement>() != null && item != gameObject)
                {
                    item.GetComponent<Rigidbody>().isKinematic = true; //Clients will update theirs physics
                }
            }
            transform.Find("BodyMesh").GetComponent<SkinnedMeshRenderer>().enabled = false;
            transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().enabled = true;
            //transform.Find("metarig/Pistol").localPosition = new Vector3(0.6406f, 1.1675f, 0.1999f);
            transform.Find("metarig/Pistol").localPosition = new Vector3(-0.45f, 0.35f, 1.49f);
            //transform.Find("metarig/Pistol").localRotation = Quaternion.Euler(285.8382f, 90, 90);
            for (int j = 1; j <= transform.Find($"metarig/Pistol").childCount; j++)
            {
                transform.Find($"metarig/Pistol/AnimationHolder{j}/main").gameObject.layer = 7;
                for (int i =0; i < transform.Find($"metarig/Pistol/AnimationHolder{j}/main").childCount; i++)
                {
                    transform.Find($"metarig/Pistol/AnimationHolder{j}/main").GetChild(i).gameObject.layer = 7;
                }
            }
            GetComponent<PlayerHealthManager>().ResetHealth();
            if(GameObject.Find("PlayerUI/Crosshair").GetComponent<RectTransform>().localPosition.x != 0)
            {
                if(GameObject.Find("PlayerUI/Crosshair").GetComponent<RectTransform>().localPosition.x > 0)
                    for (int i = 0; i < GameObject.Find("PlayerUI").transform.childCount; i++)
                    {
                        GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.x - 4000, GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.y);
                    }
                else
                    for (int i = 0; i < GameObject.Find("PlayerUI").transform.childCount; i++)
                    {
                        GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.x + 4000, GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.y);
                    }

            }

            GameObject.Find("MenuUI").GetComponent<MenuManager>().SetupCanvas();
            PauseUI = GameObject.Find("MenuUI/3/PauseBG");
            PauseIP = PauseUI.transform.Find("SmallBG/IP").gameObject;
            PauseShowIP = PauseUI.transform.Find("SmallBG/ShowIP").gameObject;
            PauseShowIP.GetComponent<Button>().onClick.AddListener(PauseShowHideIP);
            StartCoroutine(GetIPAddress());

            finishedStartPhase = true;
        }

    }

    [ClientCallback]
    [Obsolete]
    public void Start()
    {
        StartCoroutine(StartPhase());
    }

    [Command]
    public void DisconnectAllPlayers()
    {
        LeaveServer();
    }
    [ClientRpc(includeOwner = false)]
    public void LeaveServer()
    {
        NetworkManager.singleton.StopClient();
        GameObject.Find("MenuUI").GetComponent<MenuManager>().ButtonLeave();
    }


    [Obsolete]
    IEnumerator GetIPAddress()
    {
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get("http://checkip.dyndns.org");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            externalIP = "ERROR";
        }
        else
        {
            string result = www.downloadHandler.text;

            // This results in a string similar to this: <html><head><title>Current IP Check</title></head><body>Current IP Address: 123.123.123.123</body></html>
            // where 123.123.123.123 is your external IP Address.
            //  Debug.Log("" + result);

            string[] a = result.Split(':'); // Split into two substrings -> one before : and one after. 
            string a2 = a[1].Substring(1);  // Get the substring after the :
            string[] a3 = a2.Split('<');    // Now split to the first HTML tag after the IP address.
            string a4 = a3[0];              // Get the substring before the tag.

            externalIP = a4;
        }
    }
    public void PauseShowHideIP()
    {
        if(PauseIP.GetComponent<TextMeshProUGUI>().text == "Server IP: #######")
        {
            if(isLocalPlayer && isServer)
            {
                PauseIP.GetComponent<TextMeshProUGUI>().text = $"Server IP: {externalIP}";
            }
            else
            {
                PauseIP.GetComponent<TextMeshProUGUI>().text = $"Server IP: {NetworkManager.singleton.networkAddress}";
            }
            PauseShowIP.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hide";
        }
        else
        {
            PauseHideIP();
        }
    }

    void PauseHideIP()
    {
        PauseIP.GetComponent<TextMeshProUGUI>().text = "Server IP: #######";
        PauseShowIP.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Show";
    }

    void ChangeitemSlot(int newSlot)
    {
        if (newSlot == itemSlot) return;
        for (int i = 0; i < transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}/main/Tip").childCount; i++)//Destroy FireSmokeParticles
        {
            Destroy(transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}/main/Tip").GetChild(i).gameObject);
        }
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().enabled = false;
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").gameObject.SetActive(false);
        itemSlot = newSlot;
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").gameObject.SetActive(true);
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().enabled = true;
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().zoomingTime = 0;
        speed = normalSpeed;

    }


    [Command]
    void ChangeWeaponOnServer(int slot) => RPC_ChangeWeaponOnServer(slot);

    [ClientRpc(includeOwner =false)]
    void RPC_ChangeWeaponOnServer(int slot)
    {
        for(int i= 1 ;i<= transform.Find($"metarig/Pistol").childCount;i++)
        {
            transform.Find($"metarig/Pistol/AnimationHolder{i}").gameObject.SetActive(false);
        }
        transform.Find($"metarig/Pistol/AnimationHolder{slot}").gameObject.SetActive(true);
    }


    [ClientRpc]
    public void OnlyForHost()
    {
        if (NetworkClient.localPlayer.isServer)
        {
            //print($"<host>: new player! Count: {NetworkServer.connections.Count} PlayersCount: {NetworkClient.localPlayer.gameObject.GetComponent<PlayerMovement>().PlayersCount}");
            PlayersCount = NetworkServer.connections.Count;
            //print($"<host>: PlayersCount: {NetworkClient.localPlayer.gameObject.GetComponent<PlayerMovement>().PlayersCount}");
        }
    }

    [Command]
    public void SendKillsDeath_CMD()
    {
        OnlyForHost();

        List<GameObject> items = new(GameObject.FindGameObjectsWithTag("Player"));
        items.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("LocalPlayer")));

        foreach (var item in items)
        {
            if (item.GetComponent<PlayerMovement>() != null && item != gameObject)
            {
                SetKillsDeathForPlayer(transform.GetComponent<NetworkIdentity>().connectionToClient, item, item.GetComponent<PlayerMovement>().Kills, item.GetComponent<PlayerMovement>().Deaths);
            }
        }
    }


    [TargetRpc]
    public void SetKillsDeathForPlayer(NetworkConnection NewPlayer, GameObject Player, int Kills, int Deaths)
    {
        //print($"Player{Player.GetComponent<NetworkIdentity>().netId}: K: {Player.GetComponent<PlayerMovement>().Kills}/{Kills} D:{ Player.GetComponent<PlayerMovement>().Deaths}/{Deaths} ");        
        Player.GetComponent<PlayerMovement>().Kills = Kills;
        Player.GetComponent<PlayerMovement>().Deaths = Deaths;
    }

    [Command]
    public void TurnOffRigidbodyOnServer() => TurnOffRigidbodyOnAllClients();

    [ClientRpc(includeOwner = false)]
    public void TurnOffRigidbodyOnAllClients()
    {
        transform.GetComponent<Rigidbody>().isKinematic = true;
    }


    public void SpawnDamageText(GameObject original, Vector3 position, Quaternion rotation, Transform parent, int damage)
    {
        if (!hasAuthority) { return; }
        Instantiate(original, position, rotation, parent).transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"-{damage}";

    }

    [Command]
    public void HitPlayer(GameObject target, int damage)
    {
        RPC_HitPlayer(target.GetComponent<NetworkIdentity>().connectionToClient, target, damage);
    }

    [TargetRpc]
    public void RPC_HitPlayer(NetworkConnection targetConnection, GameObject target, int damage)
    {
        target.transform.GetComponent<PlayerHealthManager>().TakeDamage(damage, gameObject);
    }

    [Command]
    public void CMD_SpawnBulletTrail(Quaternion Rotation, Vector3 end, bool sniper, int ItemSlot)
    {
        RPC_SpawnBullet(Rotation, end, sniper, ItemSlot);
    }

    [ClientRpc]
    void RPC_SpawnBullet(Quaternion Rotation, Vector3 end, bool sniper, int ItemSlot)
    {
        if (sniper)
        {
            GameObject BulletObj = Instantiate(SniperBulletTrail, transform.Find($"metarig/Pistol/AnimationHolder{ItemSlot}/main/Tip").position, Rotation);
            BulletObj.GetComponent<Bullet>().Move(BulletObj.transform.position, end, sniper);
        }
        else
        {
            GameObject BulletObj = Instantiate(BulletTrail, transform.Find($"metarig/Pistol/AnimationHolder{ItemSlot}/main/Tip").position, Rotation);
            BulletObj.GetComponent<Bullet>().Move(BulletObj.transform.position, end, sniper) ;
        }
    }

    private void SetCamera(Transform pos)
    {
        Transform cameraTransform = Camera.main.gameObject.transform;
        cameraTransform.parent = pos;
        cameraTransform.SetPositionAndRotation(pos.position, pos.rotation);
    }

    [ClientRpc]
    public void IncreaseIntVariable(string variable)
    {
        print($"Changing {variable} variable for player {gameObject.name}");
        if(variable == "Deaths")
            transform.GetComponent<PlayerMovement>().Deaths++;
        else if(variable == "Kills")
            transform.GetComponent<PlayerMovement>().Kills++;
    }
    [Command]
    public void IncreaseIntVariable_CMD(string variable) {
        //print("changing " + variable + " variable! for player " + gameObject.name);
        IncreaseIntVariable(variable); 
    }

    [Command]
    void HidePlayer() => RPC_hidePlayer();

    [ClientRpc]
    void RPC_hidePlayer()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        transform.position = new Vector3(UnityEngine.Random.Range(0, 1000), UnityEngine.Random.Range(1000, 2000), UnityEngine.Random.Range(0, 1000));
        if(NetworkClient.localPlayer.transform != transform)
            transform.Find("BodyMesh").GetComponent<SkinnedMeshRenderer>().enabled = false;
    }


    [Command]
    void CMD_IncreaseIntVariable(string a) => IncreaseIntVariable(a); 

    public IEnumerator DeathCam(Transform target)
    {
        isAlive = false;
        Killer = target;
        CMD_IncreaseIntVariable("Deaths");
        HidePlayer();
        SpawnRagdoll();
        //Camera.main.transform.parent = target.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006/CameraAttachPoint3rd");
        //Camera.main.transform.localPosition = Vector3.zero;
        //Camera.main.transform.localRotation = Quaternion.identity;
        GameObject.Find("PlayerUI/DeathMessage").GetComponent<TextMeshProUGUI>().text = $"Player {target.GetComponent<NetworkIdentity>().netId} killed you!";
        for (int i = 0; i < GameObject.Find("PlayerUI").transform.childCount; i++)
        {
            GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.x + 4000, GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.y);
        }
        GameObject MotionBlur = GameObject.Find("Global Volume MotionBlur");
        MotionBlur.SetActive(false);

        yield return new WaitForSecondsRealtime(5);

        for (int i = 0; i < GameObject.Find("PlayerUI").transform.childCount; i++)
        {
            GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.x - 4000, GameObject.Find("PlayerUI").transform.GetChild(i).GetComponent<RectTransform>().localPosition.y);
        }
        MotionBlur.SetActive(true);
        RespawnPlayer();
    }

    [Command]
    public void SpawnRagdoll()
    {
        var Ragdoll = Instantiate(ragdoll, transform.position, transform.rotation);
        NetworkServer.Spawn(Ragdoll);
    }

    [TargetRpc]
    void CameraReturn(NetworkConnection target)
    {
        Camera.main.transform.parent = CameraMountPoint1stPerson.transform;
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.identity;
    }


    [TargetRpc]
    void SetAlive(NetworkConnection target) => isAlive = true;


    [Command]
    public void RespawnPlayer()
    {
        ResetSelfMomentum(GetComponent<NetworkIdentity>().connectionToClient);
        ShowPlayer();
        RespawnPlayerRPC();
        //CameraReturn(GetComponent<NetworkIdentity>().connectionToClient);
        SetAlive(GetComponent<NetworkIdentity>().connectionToClient);
    }

    [TargetRpc]
    private void ResetSelfMomentum(NetworkConnection target)
    {
        isAlive = true;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    [ClientRpc(includeOwner =false)]
    void ShowPlayer()
    {
        transform.Find("BodyMesh").GetComponent<SkinnedMeshRenderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    [ClientRpc]
    private void RespawnPlayerRPC()
    {
        print("respawning player(spawinng and reset health)");
        transform.GetComponent<PlayerHealthManager>().ResetHealth();
        transform.position = GameObject.FindGameObjectsWithTag("SPAWN_POINT")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("SPAWN_POINT").Length)].transform.position;
    }

    [Command]
    void RemoveGhostPlayer(PlayerMovement item)
    {
        RemoveGhostPlayer_RPC(item);
    }

    [ClientRpc]
    void RemoveGhostPlayer_RPC(PlayerMovement item)
    {
        PlayerConnections.Remove(item);
    }

    [ClientCallback]
    [Obsolete]
    void Update()
    {
        if (!isLocalPlayer) return;
        if (!finishedStartPhase) return;
        if (!isAlive)
        {
            VectorToRotate = Vector3.zero;
            if (Killer != null)
            {

                Camera.main.transform.position = Killer.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006/CameraAttachPoint3rd").position;
                Camera.main.transform.rotation = Killer.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006/CameraAttachPoint3rd").rotation;
            }
        }
        else
        {
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
        }
        PlayersCount = PlayerConnections.Count();
        if (paused)
        {
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().enabled = false;
        }
        else
        {
            GameObject.Find("PlayerUI/Crosshair").GetComponent<Image>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            PauseHideIP();
        }
        PauseUI.SetActive(paused);

        if (transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().ReloadTime <= 0 && isAlive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeitemSlot(1);
                ChangeWeaponOnServer(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeitemSlot(2);
                ChangeWeaponOnServer(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeitemSlot(3);
                ChangeWeaponOnServer(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ChangeitemSlot(4);
                ChangeWeaponOnServer(4);
            }
        }


        if (transform.position.y <= -1.79f) RespawnPlayer();
        if (Physics.Raycast(transform.position + new Vector3(0, 1.8f, 0), transform.TransformDirection(Vector3.down), 2 * transform.localScale.y + 0.2f) && Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 450, 0));
        }

        if (Input.GetKeyDown(KeyCode.F5) && isAlive)
        {
            if (Camera.main.gameObject.transform.parent == CameraMountPoint1stPerson.transform)
                SetCamera(CameraMountPoint3rdPerson.transform);
            else
                SetCamera(CameraMountPoint1stPerson.transform);
        }


        if (Input.GetKey(KeyCode.Tab))
        {
            GameObject.FindGameObjectsWithTag("Canvas")[0].transform.Find($"LeaderboardBG").gameObject.SetActive(true);

            int index = 0;
            int c = 0;
            foreach (var item in PlayerConnections)
            {
                try
                {
                    GameObject PlayerStats = GameObject.FindGameObjectsWithTag("PlayerStats")[index];
                    if (item == this)
                    {
                        PlayerStats.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                        PlayerStats.transform.Find("PlayerKills").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                        PlayerStats.transform.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        PlayerStats.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
                        PlayerStats.transform.Find("PlayerKills").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
                        PlayerStats.transform.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
                    }
                    PlayerStats.GetComponent<RectTransform>().localPosition = new Vector3(0, PlayerStats.GetComponent<RectTransform>().localPosition.y);
                    PlayerStats.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = $"Player {item.GetComponent<NetworkIdentity>().netId}";
                    PlayerStats.transform.Find("PlayerKills").GetComponent<TextMeshProUGUI>().text = item.Kills.ToString();
                    PlayerStats.transform.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().text = item.Deaths.ToString();
                    index++;
                }
                catch
                {
                    print($"Ghost player {c}");
                    RemoveGhostPlayer(item);
                    c++;
                }
            }
            for (int i = index; i < 10; i++)
            {
                GameObject.FindGameObjectsWithTag("PlayerStats")[i].GetComponent<RectTransform>().localPosition = new Vector3(4000, GameObject.FindGameObjectsWithTag("PlayerStats")[i].GetComponent<RectTransform>().localPosition.y);
            }

        } else if (GameObject.FindGameObjectsWithTag("Canvas")[0].activeInHierarchy)
            GameObject.FindGameObjectsWithTag("Canvas")[0].transform.Find($"LeaderboardBG").gameObject.SetActive(false);


        if (Input.GetKeyDown(KeyCode.R) && transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().ReloadTime <= 0 && isAlive)
        {
            transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().Reload();
        }


        if (Input.GetKey(KeyCode.Mouse1) && isAlive)
        {
            transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().isZoomed = true;
            if (transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().canZoom)
            {
                speed = crouchSpeed;
                if (Input.GetKeyDown(KeyCode.Mouse1))
                    transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().zoomingTime = 0;
            }
        }
        else
        {
            transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().isZoomed = false;
            if (transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().canZoom && Input.GetKeyUp(KeyCode.Mouse1))
                transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().zoomingTime = 0;
            speed = normalSpeed;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.Mouse0) && transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().LoadedBullets > 0 && transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().TimeSinceLastShot <= 0 && transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().ReloadTime <= 0 && isAlive && !paused)
        {
            Dictionary<GameObject, int> TargetsObj = new();
            transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().FireGun();
            for (int i = 0; i < Mathf.Sqrt(transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().BulletsInShot); i++)
            {
                for (int j = 0; j < Mathf.Sqrt(transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().BulletsInShot); j++)
                {
                    float offsetX = 0;
                    float offsetY = 0;
                    if (transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().BulletsInShot > 2)
                    {
                        offsetX = UnityEngine.Random.Range(-6f, 6f);
                        offsetY = UnityEngine.Random.Range(-6f, 6f);
                    }
                    Vector3 direction = Mathf.Abs(Camera.main.transform.forward.x) < .45f ? Quaternion.AngleAxis(offsetX, Vector3.left) * Camera.main.transform.forward : Quaternion.AngleAxis(offsetX, Vector3.forward) * Camera.main.transform.forward;
                    direction = Quaternion.AngleAxis(offsetY, Vector3.up) * direction;
                    if (i == 0 && j == 0)
                        direction = Camera.main.transform.forward;
                    RaycastHit[] hitInfo;
                    hitInfo = Physics.RaycastAll(Camera.main.transform.position, direction, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().BulletTravelLifetime);
                    Array.Sort(hitInfo, (x, y) => x.distance.CompareTo(y.distance));
                    if (hitInfo.Length == 0 || (hitInfo.Length == 1 && hitInfo[0].collider.CompareTag("ClippingBlocker")))
                    {
                        CMD_SpawnBulletTrail(Quaternion.identity, Camera.main.transform.position + direction * transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().BulletTravelLifetime, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().sniperGun, itemSlot);
                    }
                    foreach (var item in hitInfo)
                    {
                        //print($"hit: {item.collider.gameObject.name} with tag {item.collider.tag}");
                        if (item.collider.CompareTag("ClippingBlocker") || item.collider.transform.root.CompareTag("LocalPlayer"))
                            continue;
                        if (!item.collider.transform.root.CompareTag("Player"))
                        {
                            //print("hit not a player! cuz its " + item.collider.transform.root.gameObject.name);
                            Quaternion HitRot = Quaternion.LookRotation(item.normal);
                            Instantiate(HitParticle, item.point, HitRot, null);
                            CMD_SpawnBulletTrail(Quaternion.identity, item.point, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().sniperGun, itemSlot);
                            break;
                        }

                        if (item.collider.transform.root.gameObject != gameObject)
                        {
                            Quaternion HitRot = Quaternion.LookRotation(item.normal);
                            GameObject Sparkles = Instantiate(HitParticle, item.point, HitRot, null);
                            Sparkles.GetComponent<ParticleSystem>().startColor = new Color(1, 0, 0);
                            CMD_SpawnBulletTrail(Quaternion.identity, item.point, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().sniperGun, itemSlot);
                            if (item.collider.transform.name == "spine.006")
                            {
                                if (TargetsObj.ContainsKey(item.collider.transform.root.gameObject))
                                {
                                    TargetsObj[item.collider.transform.root.gameObject] += transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().HeadshotDamage;
                                }
                                else
                                {
                                    TargetsObj.Add(item.collider.transform.root.gameObject, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().HeadshotDamage);
                                }

                                break;
                            }
                            if (TargetsObj.ContainsKey(item.collider.transform.root.gameObject))
                            {
                                TargetsObj[item.collider.transform.root.gameObject] += transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().Damage;
                            }
                            else
                            {
                                TargetsObj.Add(item.collider.transform.root.gameObject, transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().Damage);
                            }
                            break;//prevent sparkles from showing behind the player
                        }
                    }
                }
            }
            foreach (var item in TargetsObj)
            {
                print(item.Value);
                HitPlayer(item.Key, item.Value);
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        LockedCursor = Input.GetKeyDown(KeyCode.Escape) ? !LockedCursor : LockedCursor;
        Cursor.lockState = LockedCursor ? CursorLockMode.Locked : CursorLockMode.None;

        MouseX = Input.GetAxis("Mouse X") * MouseFrequency;
        MouseY = Input.GetAxis("Mouse Y") * MouseFrequency;
        if(transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().isZoomed == true)
        {
            if (transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}").GetComponent<Gun>().sniperGun)
            {
                MouseX *= 0.2f;
                MouseY *= 0.2f;
            }
            else
            {
                MouseX *= 0.5f;
                MouseY *= 0.5f;
            }
        }
        float h = Input.GetAxisRaw("Horizontal") > 0 ? 1 : (Input.GetAxisRaw("Horizontal") < 0 ? -1 : 0);
        float v = Input.GetAxisRaw("Vertical") > 0 ? 1 : (Input.GetAxisRaw("Vertical") < 0 ? -1 : 0);
        // v = up/down , h = right/left
        MoveVector = Vector3.zero;
        if (isAlive && !paused)
        {
            MoveVector = new Vector3(h * Time.deltaTime * speed, 0, v * Time.deltaTime * speed);
            VectorToRotate += new Vector3(MouseY, MouseX, 0);
        }

        int MoveableLayer = ~(1 << 3) | ~(1<<8);
      
        if (Physics.Raycast(transform.position + new Vector3(0, 2.5f, 0), transform.TransformDirection(Vector3.right * h), -Vector3.Distance(transform.position, transform.position + MoveVector) + 0.8f, MoveableLayer) ||
            Physics.Raycast(transform.position + new Vector3(0, 1.8f, 0), transform.TransformDirection(Vector3.right * h), -Vector3.Distance(transform.position, transform.position + MoveVector) + 0.8f, MoveableLayer)) 
        {
            MoveVector.x = Vector3.zero.x;
        } 
        if(Physics.Raycast(transform.position + new Vector3(0, 2.5f, 0), transform.TransformDirection(Vector3.forward * v), -Vector3.Distance(transform.position, transform.position + MoveVector) + 0.8f, MoveableLayer) ||
           Physics.Raycast(transform.position + new Vector3(0, 1.8f, 0), transform.TransformDirection(Vector3.forward * v), -Vector3.Distance(transform.position, transform.position + MoveVector) + 0.8f, MoveableLayer))
        {
            MoveVector.z = Vector3.zero.z;
        }
        VectorToMove += MoveVector;
        MovementBuffer.Add(new Vector3[2]);
        MovementBuffer[^1][0] = MoveVector; 
        MovementBuffer[^1][1] = VectorToRotate;

        transform.rotation = Quaternion.Euler(new Vector3(0, VectorToRotate.y, 0));
        transform.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006").localRotation = Quaternion.Euler(new Vector3(VectorToRotate.x, 0, 0));
        transform.Find($"metarig/Pistol/AnimationHolder{itemSlot}/main").localRotation = Quaternion.Euler(new Vector3(-VectorToRotate.x, 0, 0));//.Rotate(new Vector3(-MouseY, 0, 0));
        //transform.Find("metarig/Pistol").rotation = Quaternion.Euler(new Vector3(VectorToRotate.x, 0, 0));
        transform.Translate(MoveVector);
        Coll_check.transform.rotation = Quaternion.Euler(new Vector3(0, VectorToRotate.y, 0));
        Coll_check.transform.Translate(MoveVector);
        if (Vector3.Distance(Coll_check.transform.position, transform.position) >= 0.01f)
        {
            UseClientPhysics = true;
            Coll_check.transform.position = transform.position;
            //print("DYSYNC! - used physics!");
        }
        //else
            //print("SYNNC");

        ///////////////////////////////////////////.Translate(MoveVector);///////////////////////////////////////////
        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            MovePlayer(VectorToRotate, transform.position, MovementBuffer,UseClientPhysics);
            UseClientPhysics = false;
            VectorToMove = Vector3.zero;
            MovementBuffer.Clear();
            currentTick++;
        }
//////////////////////////////////////////////////////////////////////////////////////
    }

    [Command]
    private void MovePlayer(Vector3 RotateVector,  Vector3 pos, List<Vector3[]> MovementBuffer, bool ClientPhysics) => MovePlayerRPC(RotateVector, pos, MovementBuffer, ClientPhysics);

    [ClientRpc(includeOwner =false)]
    private void MovePlayerRPC(Vector3 RotateVector, Vector3 pos, List<Vector3[]> MovementBuffer, bool ClientPhysics)
    {

        if (ClientPhysics)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, RotateVector.y, 0));
            transform.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006").localRotation = Quaternion.Euler(new Vector3(RotateVector.x, 0, 0));
            transform.position = pos;
        }
        else {
            foreach (var item in MovementBuffer)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, item[1].y, 0));
                transform.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004/spine.005/spine.006").localRotation = Quaternion.Euler(new Vector3(item[1].x, 0, 0));
                transform.Translate(item[0]);
            }
        }
    }

}

