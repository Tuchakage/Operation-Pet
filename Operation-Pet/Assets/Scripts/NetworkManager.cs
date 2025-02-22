using UnityEngine;
using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEditor;
using TMPro;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    //Variable that is shared between all instances
    public static NetworkManager networkInstance;
    public GameObject player;

    public GameObject ball;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        networkInstance = this;


        //If the 2nd Player
        if (PhotonNetwork.CurrentRoom.Players.Count > 1)
        {
            PhotonNetwork.Instantiate(player.name, new Vector3(-4f, 1.5f, -2f), Quaternion.identity, 0);

            PhotonNetwork.Instantiate(ball.name, new Vector3(0f, 1.5f, -2f), Quaternion.identity, 0);
        }
        else //If the 1st Player
        {
            PhotonNetwork.Instantiate(player.name, new Vector3(4f, 1.5f, -2f), Quaternion.identity, 0);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }




}
