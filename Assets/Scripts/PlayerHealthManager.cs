using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class PlayerHealthManager : NetworkBehaviour
{
    public const float maxHealth = 100;
    public float health = maxHealth;
    private int damageTaken=0;
    private List<GameObject> AttackersObj = new();
    public GameObject damageTextObj;
    public Vector3 LastPos=Vector3.zero;
    public void ResetHealth()
    {
        health = maxHealth;
        if(isLocalPlayer)
            GameObject.Find("PlayerUI").transform.Find("Healthbar").Find("Health").GetComponent<Image>().fillAmount = health / 100;
    }
    public void TakeDamage(int damage, GameObject Attacker = null)
    {
        
        health -= damage;
        damageTaken += damage;
        if (isLocalPlayer)
            GameObject.Find("PlayerUI").transform.Find("Healthbar").Find("Health").GetComponent<Image>().fillAmount = health / 100;
        if (Attacker != null && !AttackersObj.Contains(Attacker))
            AttackersObj.Add(Attacker);
        

        if (health <= 0)
        {
            IncreaseIntVariable_CMD2(Attacker, "Kills");
            print($"{Attacker.name} killed me");
            StartCoroutine(gameObject.GetComponent<PlayerMovement>().DeathCam(Attacker.transform));
        }
    }

    [Command]
    public void IncreaseIntVariable_CMD2(GameObject attacker, string variable) //=> attacker.GetComponent<PlayerMovement>().IncreaseIntVariable_CMD(variable);
    {
        IncreaseIntVariable(attacker, variable);
    }

    [ClientRpc]
    public void IncreaseIntVariable(GameObject attacker, string variable)
    {
        print($"Changing {variable} variable for player {attacker.name}");
        if (variable == "Deaths")
            attacker.GetComponent<PlayerMovement>().Deaths++;
        else if (variable == "Kills")
            attacker.GetComponent<PlayerMovement>().Kills++;
    }

    void Update()
    {
        if(damageTaken != 0)
        {
            foreach (var item in AttackersObj)
            {
                Cmd_AttackerShowDamageText(item, damageTaken);
            }
            AttackersObj.Clear();
            damageTaken = 0;
        }
        LastPos = transform.position;
    }

    [Command]
    public void Cmd_AttackerShowDamageText(GameObject attacker, int damage) { TRpc_AttackerShowDamageText(attacker.GetComponent<NetworkIdentity>().connectionToClient, attacker, damage); }
    [TargetRpc]
    public void TRpc_AttackerShowDamageText(NetworkConnection conn, GameObject attacker, int damage)
    {
        attacker.GetComponent<PlayerMovement>().SpawnDamageText(damageTextObj, Camera.main.WorldToScreenPoint(transform.GetComponent<PlayerHealthManager>().LastPos + new Vector3(Random.Range(-.3f, .3f), Random.Range(1f, 2f))), Quaternion.identity, GameObject.Find("Canvas").transform, damage);
    }

}
