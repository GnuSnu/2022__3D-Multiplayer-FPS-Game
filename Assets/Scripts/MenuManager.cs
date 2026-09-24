using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using System.Net;

/*
 * NOTE: This codebase was written when I was 15 years old, prior to my involvement
 * in competitive programming and interest in code optimalization. 
 * My coding standards, software architecture, and optimization techniques have evolved since then. 
 */


public class MenuManager : MonoBehaviour
{

    public GameObject Panel1;
    public GameObject Panel2;
    public GameObject Panel3;
    public GameObject PlayerUI;
    public GameObject QuitGameBG;
    public string CurrentConnectionCheckIP="";
    public List<string> ActiveServers;
    public List<string> OfflineServers;
    public bool connectionCheck = false;

    public Button buttonHost, buttonClient, buttonLeave, buttonGoToJoinMenu, buttonBack;
    public TextMeshProUGUI ConnectingTo;
    public GameObject inputIP;

    [Obsolete]
    private void Start()
    {
        //Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
       // if (NetworkManager.singleton.networkAddress != "localhost") { inputFieldAddress.text = NetworkManager.singleton.networkAddress; }

        //Adds a listener to the main input field and invokes a method when the value changes.
        Panel1.transform.Find("JOIN").GetComponent<Button>().onClick.AddListener(GoToPanel2);
        Panel2.transform.Find("BackPage").GetComponent<Button>().onClick.AddListener(GoToPanel1);
        //Make sure to attach these Buttons in the Inspector
        buttonHost.onClick.AddListener(ButtonHost);
        buttonClient.onClick.AddListener(ButtonClient);
        buttonLeave.onClick.AddListener(ButtonLeave);

        //This updates the Unity canvas, we have to manually call it every change, unlike legacy OnGUI.
        SetupCanvas();
        StartCoroutine(GetIPAddress());
    }

    [Obsolete]
    IEnumerator GetIPAddress()
    {
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get("83.24.131.29:7777");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            print("ERROR");
        }
        else
        {
            print("server online!!");
        }
    }


    public void Refresh()
    {
        if(CurrentConnectionCheckIP=="")
            StartCoroutine(CheckConnections());
    }

    IEnumerator CheckConnections()
    {
        int timeConnected=0;
        ActiveServers.Clear();
        transform.Find("2/Servers").GetComponent<ServerSearch>().RefreshServers();
        for (int i = 0; i < transform.Find("2/Servers/Scroll View/Viewport/Content").childCount; i++)
        {
            int timer = 0;
            NetworkManager.singleton.networkAddress = transform.Find("2/Servers/Scroll View/Viewport/Content").GetChild(i).GetComponent<ServerObjScript>().IP;
            CurrentConnectionCheckIP = NetworkManager.singleton.networkAddress;
            NetworkManager.singleton.StartClient(true);
            while (timer <= 125)
            {
                if (NetworkClient.isConnected)
                {
                    if (timeConnected > 50)
                        yield break;
                    timeConnected++;
                    yield return new WaitForFixedUpdate();
                    continue;
                }
                else
                {
                    timer++;
                }
                if (timer > 125 || CurrentConnectionCheckIP == "")
                {
                    if (!ActiveServers.Contains(transform.Find("2/Servers/Scroll View/Viewport/Content").GetChild(i).GetComponent<ServerObjScript>().IP))
                        transform.Find("2/Servers/Scroll View/Viewport/Content").GetChild(i).Find("ConnectionIndicator").GetComponent<Image>().color = Color.red;
                    CurrentConnectionCheckIP = "";
                    break;
                }
                transform.Find("2/Servers/Scroll View/Viewport/Content").GetChild(i).Find("ConnectionIndicator").GetComponent<Image>().color = Color.blue;
                yield return new WaitForFixedUpdate();
            }
        }
    }

    public void ResetPanels()
    {
        Panel1.SetActive(false);
        Panel2.SetActive(false);
        Panel3.SetActive(false);
    }

    public void GoToPanel1()
    {
        ResetPanels();
        Panel1.SetActive(true);
    }
    public void GoToPanel2()
    {
        ResetPanels();
        Panel2.SetActive(true);
        ConnectingTo.text = "";
        if (PlayerMovement.ServersContent == null)
            PlayerMovement.ServersContent = GameObject.Find("MenuUI/2/Servers/Scroll View/Viewport/Content").transform;
    }
    public void GoToPanel3()
    {
        ResetPanels();
        Panel3.SetActive(true);
    }

    // Invoked when the value of the text field changes.
    public void setServerIP()
    {
        NetworkManager.singleton.networkAddress = inputIP.GetComponent<TMP_InputField>().text;
    }

    public void ButtonHost()
    {
        if (NetworkClient.isConnecting)
        {
            NetworkManager.singleton.StopClient();
            NetworkClient.StaticMonoB_obj.StopAllCoroutines();
        }
        NetworkManager.singleton.StartHost();
        SetupCanvas();
    }


    public void ButtonClient()
    {
        setServerIP();
        NetworkManager.singleton.StartClient();
        SetupCanvas();
    }

    public void JoinServerWithIP(string IP)
    {
        NetworkManager.singleton.networkAddress = IP;
        NetworkManager.singleton.StartClient();
        SetupCanvas();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowQuitGameBG()
    {
        QuitGameBG.GetComponent<RectTransform>().localScale = Vector3.one;
    }
    public void CloseQuitGameBG()
    {
        QuitGameBG.GetComponent<RectTransform>().localScale = Vector3.zero;
    }


    public void ButtonLeave()
    {
        PlayerUI.GetComponent<Canvas>().enabled = false;
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkClient.localPlayer.transform.GetComponent<PlayerMovement>().DisconnectAllPlayers();
            NetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        if (connectionCheck)
        {
            GoToPanel2();
            connectionCheck = false;
        }
        else
        {
            GoToPanel1();
        }
    }

    public void SetupCanvas()
    {
        // Here we will dump majority of the canvas UI that may be changed.

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (NetworkClient.active)
            {
                GoToPanel2();
                ConnectingTo.text = "Connecting to " + NetworkManager.singleton.networkAddress + "..";
            }
            else
            {
                GoToPanel1();
            }
        }
        else
        {
            GoToPanel3();
            PlayerUI.GetComponent<Canvas>().enabled = true;
        }
    }
}
