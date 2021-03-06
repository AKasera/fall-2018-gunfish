﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

//SERVER ONLY
public class ConnectionManager : NetworkBehaviour {

    public static ConnectionManager instance;

    public Dictionary<Gunfish, bool> readyFish;
    public int readyCount;
    public int notReadyCount;

    // Use this for initialization
    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

        readyCount = 0;
        notReadyCount = 0;
        readyFish = new Dictionary<Gunfish, bool>(0);
    }

    public void AddGunfish (Gunfish fish) {
        readyFish.Add(fish, false);
        if (RaceManager.instance.gameActive) {
            fish.Stun(3f);
        }
        notReadyCount++;
        RaceManager.instance.TrySwapLevel();
    }

    public void RemoveGunfish (NetworkConnection conn) {
        Gunfish fish = null;
        foreach (var thisFish in readyFish.Keys) {
            if (thisFish.connectionToServer == conn) {
                fish = thisFish;
            }
        }

        if (fish == null) return;

        bool ready;
        if (!readyFish.TryGetValue(fish, out ready)) return;

        if (ready) {
            readyCount--;
        } else {
            notReadyCount--;
        }

        readyFish.Remove(fish);
        RaceManager.instance.TrySwapLevel();
    }

    public void SetReady (Gunfish fish, bool ready) {
        if (!readyFish.ContainsKey(fish)) {
            AddGunfish(fish);
        }

        if (readyFish[fish] == ready) return;

        readyFish[fish] = ready;

        if (ready) {
            readyCount++;
            notReadyCount--;
        } else {
            print("Set not ready ++");
            readyCount--;
            notReadyCount++;
        }
        RaceManager.instance.TrySwapLevel();
    }

    public void SetAllFishReady (bool ready) {

        List<Gunfish> keys = new List<Gunfish>(readyFish.Keys);

        foreach (Gunfish fish in keys) {
            readyFish[fish] = ready;
            //print("Setting " + fish.name + " to " + ready);
        }

        if (ready) {
            readyCount = readyFish.Count;
            notReadyCount = 0;
        } else {
            readyCount = 0;
            notReadyCount = readyFish.Count;
        }
    }

    public void Clear () {
        readyFish.Clear();
        readyCount = 0;
        notReadyCount = 0;
    }

    //   void StartGame () {
    //       print("Start Game");
    //       EventManager.TriggerEvent(EventType.NextLevel);
    //   }

    //// Update is called once per frame
    //void Update () {
    //       Gunfish[] fish = FindObjectsOfType<Gunfish>();
    //       gunfish = new List<Gunfish>(fish);
    //}

    //public void SortByScore () {
    //    //List<PlayerConnection> temp = new List<PlayerConnection>();

    //    //for (int i = 0; i < players.Count; i++) {
    //    //    int highest = 0;
    //    //    int highestIndex = 0;
    //    //    for (int j = 0; j < players.Count; j++) {
    //    //        if (players[j].score > highest) {
    //    //            highest = players[j].score;
    //    //            highestIndex = j;
    //    //        }
    //    //    }
    //    //    temp.Add(players[highestIndex]);
    //    //}
    //    //players = temp;
    //}
}
