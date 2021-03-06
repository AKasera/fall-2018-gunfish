﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
public class CustomNetworkManager : NetworkManager  
{
    public List<GameObject> fishList;
    
    private NetworkStartPosition[] spawnPoints;
    public int spawnNum;

    public override void OnStartServer() {
        fishList = new List<GameObject>(GunfishList.Get());
        spawnNum = 0;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        spawnPoints = FindObjectsOfType<NetworkStartPosition>(); //Get list of all spawn points in the scene

        //If there aren't any spawn points in the scene, spawn players at the origin
        Vector3 targetPosition = (spawnPoints.Length > 0 ? spawnPoints[(spawnNum) % spawnPoints.Length].transform.position : Vector3.zero);

        //Assign the players a random fish when they join
        int index = Random.Range(0, fishList.Count);
        if (RaceManager.instance.fishTable.ContainsKey(conn)) {
            index = RaceManager.instance.fishTable[conn];
        } else {
            RaceManager.instance.fishTable.Add(conn, index);
        }

        GameObject player = (GameObject)Instantiate(fishList[index], targetPosition, Quaternion.identity);
        //print("PLAYER: " + player);
        string playerName = "Player " + (conn.connectionId + 1);
        if (RaceManager.instance && RaceManager.instance.pointTable.ContainsKey(conn)) {
            if (RaceManager.instance.pointTable[conn] > 0) {
                playerName += "\nPoints: " + RaceManager.instance.pointTable[conn];
                if(RaceManager.instance.pointTable[conn] >= RaceManager.instance.MaxPointsEarned) {
                    Crowner.SpawnCrown(player);
                    
                }
            } else {
                Crowner.SpawnCrown(player);
                RaceManager.instance.pointTable[conn] = 0;
            }
        }

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        bool crowned = false;
        if (RaceManager.instance.pointTable.ContainsKey(conn) && RaceManager.instance.pointTable[conn] >= RaceManager.instance.MaxPointsEarned && RaceManager.instance.pointTable[conn] > 0) {
            crowned = true;
        }
        StartCoroutine(SetRpc(player, playerName, crowned));
        ConnectionManager.instance.AddGunfish(player.GetComponent<Gunfish>());
        spawnNum++;
    }

    /// <summary>
    /// Spawn a crayon on a winning fish
    /// </summary>
    /// <param name="fish"></param>
    /// 
    /*
    void SpawnCrown(GameObject fish) {
        if (RaceManager.instance.CrownPrefab != null) {
            GameObject crown = Instantiate(RaceManager.instance.CrownPrefab);
            Transform crownLoc = fish.transform.FindDeepChild("CrownLocation");

            crown.transform.SetParent(crownLoc);
            crown.transform.localPosition = Vector3.zero;
            crown.transform.localRotation = Quaternion.Euler(0, 0, -180);
        }
    }
    */

    //NOTE: This is simply a race condition. Replace this with something not stupid
    public IEnumerator SetRpc (GameObject player, string playerName, bool crowned) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (player.GetComponent<Gunfish>()) {
            player.GetComponent<Gunfish>().gameName = playerName;
            player.GetComponent<Gunfish>().crowned = crowned;
            player.GetComponent<Gunfish>().RpcSetName(playerName);
            if (crowned)
                player.GetComponent<Gunfish>().RpcCrown();
        }
    }

    public override void OnServerRemovePlayer (NetworkConnection conn, UnityEngine.Networking.PlayerController player) {
        //base.OnServerRemovePlayer(conn, player);
        ConnectionManager.instance.RemoveGunfish(conn);
        //print("Removing player");
        RaceManager.instance.TrySwapLevel();
    }

    public override void OnServerSceneChanged (string sceneName) {
        base.OnServerSceneChanged (sceneName);
    }





    //UI
    /***************************************************/

    /// <summary>
    /// A wrapper for NetworkManager's StartHost method. 
    /// </summary>
    public void StartHost_Button()
    {
        base.StartHost();
    }

    public void StartClient_Button()
    {
        base.StartClient();
    }

    public void UpdateAddress()
    {
        networkAddress = FindMainMenu().GetComponent<MainMenuManager>().Addr;
    }

    public void UpdatePort()
    {
        string port = FindMainMenu().GetComponent<MainMenuManager>().Port;
        int res;
        bool success = int.TryParse(port, out res);
        if(success)
            networkPort = res;
        else
        {
            Debug.Log("Failed to set port");
        }
    }

    public GameObject FindMainMenu()
    {
        return GameObject.Find("mainmenu").transform.parent.gameObject;
    }
}