using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ServerSearch : MonoBehaviour
{
    public GameObject ServerManagerWindow;
    public GameObject EditionWindow;
    public Button ConfirmButton;
    public GameObject ServerQuickConnectObj;
    public GameObject Content;
    public GameObject DeleteServerWindow;
    private int DeletionState=0;
    private string savedServers="";
    private int lastPosY = 40;
    private int editIndex;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("Servers"))
            savedServers = PlayerPrefs.GetString("Servers");
        else
            PlayerPrefs.SetString("Servers", "");
        print(savedServers);
        ConfirmButton.onClick.AddListener(AddServerByButton);
        DeleteServerWindow.transform.Find("YES").GetComponent<Button>().onClick.AddListener(DeleteStateYES);
        DeleteServerWindow.transform.Find("NO").GetComponent<Button>().onClick.AddListener(DeleteStateNO);
        RefreshServers();
    }

    public void RefreshServers()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        foreach (var item in serversTab)
        {
            GameObject Serv = Instantiate(ServerQuickConnectObj, Content.transform);
            //Serv.GetComponent<RectTransform>().localPosition = new Vector3(48.5f, lastPosY-50);
            GameObject.Find("MenuUI").GetComponent<MenuManager>().OfflineServers.Add(item.Split(",")[1]);
            Serv.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.Split(",")[0];
            Serv.GetComponent<ServerObjScript>().IP = item.Split(",")[1];
            lastPosY -= 14;
        }
        lastPosY = 40;
    }

    void ClearSavedServers()
    {
        PlayerPrefs.SetString("Servers", "");
        savedServers = "";
    }
    public void AddServerByButton()
    {
        if (ConfirmButton.transform.parent.Find("Name_input").GetComponent<TMP_InputField>().text == "" || ConfirmButton.transform.parent.Find("IP_input").GetComponent<TMP_InputField>().text == "")
            return;
        AddServer(ConfirmButton.transform.parent.Find("Name_input").GetComponent<TMP_InputField>().text, ConfirmButton.transform.parent.Find("IP_input").GetComponent<TMP_InputField>().text);
        CloseServerManagerWindow();
    }

    void DeleteStateYES()
    {
        DeletionState = 1;
    }
    void DeleteStateNO()
    {
        DeletionState = 2;
    }

    public void JoinServerOnIP(int index)
    {
        Debug.Log(NetworkClient.isConnecting);
        if (NetworkClient.isConnecting)
        {
            NetworkManager.singleton.StopClient();
            NetworkClient.StaticMonoB_obj.StopAllCoroutines();
        }
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        if (serversTab.Count < index + 1) return;
        transform.root.GetComponent<MenuManager>().JoinServerWithIP(serversTab[index].Split(",")[1]);
    }

    public void OpenServerManagerWindow()
    {
        ServerManagerWindow.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
    }
    public void CloseServerManagerWindow()
    {
        ServerManagerWindow.transform.Find("IP_input").GetComponent<TMP_InputField>().text = "";
        ServerManagerWindow.transform.Find("Name_input").GetComponent<TMP_InputField>().text = "";
        ServerManagerWindow.transform.parent.GetComponent<RectTransform>().localScale = Vector3.zero;
    }
    public void OpenDeletionWindow()
    {
        DeleteServerWindow.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }
    public void CloseDeletionWindow()
    {
        DeleteServerWindow.transform.parent.GetComponent<RectTransform>().localScale = Vector3.zero;
    }
    public void OpenEditionWindow()
    {
        EditionWindow.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }
    public void CloseEditionWindow()
    {
        EditionWindow.transform.Find("IP_input").GetComponent<TMP_InputField>().text = "";
        EditionWindow.transform.Find("Name_input").GetComponent<TMP_InputField>().text = "";
        EditionWindow.transform.parent.GetComponent<RectTransform>().localScale = Vector3.zero;
    }
    public void EditServer(int index)
    {
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        if (serversTab.Count < index + 1) return;
        EditionWindow.transform.Find("Name_input").GetComponent<TMP_InputField>().text = serversTab[index].Split(",")[0];
        EditionWindow.transform.Find("IP_input").GetComponent<TMP_InputField>().text = serversTab[index].Split(",")[1];
        editIndex = index;
    }
    public void ConfirmEditServer()
    {
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        if (serversTab.Count < editIndex + 1) return;
        serversTab[editIndex] = $"{EditionWindow.transform.Find("Name_input").GetComponent<TMP_InputField>().text},{EditionWindow.transform.Find("IP_input").GetComponent<TMP_InputField>().text}";
        savedServers = string.Join("|", serversTab.ToArray());
        PlayerPrefs.SetString("Servers", savedServers);
        CloseEditionWindow();
        RefreshServers();
    }

    public void AddServer(string name, string IP)
    {
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        serversTab.Add($"{name},{IP}");
        savedServers = string.Join("|", serversTab.ToArray());
        PlayerPrefs.SetString("Servers", savedServers);
        CloseServerManagerWindow();
        RefreshServers();
    }
    public IEnumerator DeleteServerAt(int index)
    {
        for(int i = 0; i < 8000; i++)
        {
            if (DeletionState == 0)
                yield return new WaitForFixedUpdate();
            else
                break;
        }
        CloseDeletionWindow();
        if (DeletionState == 2)      
        {
            DeletionState = 0;
            yield break;
        }
        DeletionState = 0;
        List<string> serversTab = new(savedServers.Split("|"));
        if (serversTab[0] == "")
            serversTab.RemoveAt(0);
        if (serversTab.Count < index + 1) yield break;
        serversTab.RemoveAt(index);
        savedServers = string.Join("|", serversTab.ToArray());
        PlayerPrefs.SetString("Servers", savedServers);
        CloseServerManagerWindow();
        RefreshServers();
    }

}
