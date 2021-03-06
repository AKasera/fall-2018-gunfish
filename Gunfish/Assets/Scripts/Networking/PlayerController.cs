﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public static PlayerController instance;

    public static Gunfish ownedGunfish;
    public static GameObject namePlate;

    private void Start () {
        if (!instance) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        //Debug.Log("ConnId: " + connectionToServer.connectionId);

        NetworkManager.singleton.client.RegisterHandler(MessageTypes.DEBUGLOGMSG, OnDebugLog);

        NetworkManager.singleton.client.RegisterHandler(MessageTypes.RAYHIT, OnRayHit);
        NetworkManager.singleton.client.RegisterHandler(MessageTypes.MULTIRAYHIT, OnMultiRayHit);
        NetworkManager.singleton.client.RegisterHandler(MessageTypes.GUNSHOT, OnGunshot);

        NetworkManager.singleton.client.RegisterHandler(MessageTypes.SPAWNCROWN, SpawnCrown);

        foreach (Gunfish fish in FindObjectsOfType<Gunfish>()) {
            if (fish.hasAuthority) {
                ownedGunfish = fish;
            }
        }

        if (isClient) {
            MusicManager.instance.PlayMusic();
        }
    }

    //private void Update () {
    //    if (!isLocalPlayer) return;

    //    if (Input.GetKeyDown(KeyCode.N)) {
    //        print("Doing the thing");
    //        NetworkManager.singleton.client.Send(MessageTypes.CHANGEFISHMSG, new ChangeFishMsg(ownedGunfish.connectionToClient, 0));
    //    }
    //}

    #region MESSAGE HANDLERS

    private void OnRayHit(NetworkMessage netMsg) {
        RayHitMsg msg = netMsg.ReadMessage<RayHitMsg>();

        HandleHitInfo(msg.rayHitInfo);
    }

    private void OnMultiRayHit (NetworkMessage netMsg) {
        MultiRayHitMsg msg = netMsg.ReadMessage<MultiRayHitMsg>();

        foreach (RayHitInfo rayHitInfo in msg.rayHitInfos) {
            HandleHitInfo(rayHitInfo);
        }
    }

    private void HandleHitInfo(RayHitInfo rayHitInfo) {
        if (rayHitInfo.netId != NetworkInstanceId.Invalid) {
            EffectsManager.instance.DisplayBulletHit(rayHitInfo.end, rayHitInfo.normal, rayHitInfo.color, rayHitInfo.hitType);

            if (ownedGunfish && rayHitInfo.netId == ownedGunfish.netId) {
                ownedGunfish.Hit((rayHitInfo.end - rayHitInfo.origin).normalized, rayHitInfo.shotType);
            }
        }

        EffectsManager.instance.DisplayBulletLine(rayHitInfo.origin, rayHitInfo.end, ownedGunfish.gun.shotTrailColor);
    }

    private void OnGunshot(NetworkMessage netMsg) {
        GunfishMsg msg = netMsg.ReadMessage<GunfishMsg>();
        Gunfish gunfish = ClientScene.FindLocalObject(msg.netId).GetComponent<Gunfish>();
        gunfish.DisplayShoot();
    }

    private void OnDebugLog(NetworkMessage netMsg) {
        DebugLogMsg msg = netMsg.ReadMessage<DebugLogMsg>();

        Debug.Log(msg.log);
    }

    private void SpawnCrown(NetworkMessage netMsg)
    {
        GunfishMsg msg = netMsg.ReadMessage<GunfishMsg>();

        Crowner.SpawnCrown(ClientScene.FindLocalObject(msg.netId));
        print("Find Local: "+ClientScene.FindLocalObject(msg.netId));
    }
    #endregion
}
