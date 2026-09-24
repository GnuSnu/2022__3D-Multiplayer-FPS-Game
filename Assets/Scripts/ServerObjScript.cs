using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ServerObjScript : MonoBehaviour
{
    public string IP;
    private GameObject ServerMainObj;
    private TextMeshProUGUI DeletionWindowText;


    public void Join()
    {
        ServerMainObj.GetComponent<ServerSearch>().JoinServerOnIP(transform.GetSiblingIndex());
    }

    public void Edit()
    {
        ServerMainObj.GetComponent<ServerSearch>().OpenEditionWindow();
        ServerMainObj.GetComponent<ServerSearch>().EditServer(transform.GetSiblingIndex());
    }

    public void Delete()
    {
        DeletionWindowText.text = $"Are you sure you want to delete \"{transform.Find("Name").GetComponent<TextMeshProUGUI>().text}\" ?";
        ServerMainObj.GetComponent<ServerSearch>().OpenDeletionWindow();
        StartCoroutine(ServerMainObj.GetComponent<ServerSearch>().DeleteServerAt(transform.GetSiblingIndex()));
    }
    // Start is called before the first frame update
    void Start()
    {
        DeletionWindowText = GameObject.Find("MenuUI/2/DeleteBG/DeletionWindow/text").GetComponent<TextMeshProUGUI>();
        ServerMainObj = GameObject.Find("MenuUI/2/Servers");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
