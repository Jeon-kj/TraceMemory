using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon;
using System;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private RoomManager roomManager;

    private string pendingAction = "";

    private void Awake()
    {
        Screen.SetResolution(1080, 1920, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        
        roomManager = FindObjectOfType<RoomManager>();
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon 서버에 연결.
    public void Connect(string action) 
    {
        pendingAction = action;
        PhotonNetwork.GameVersion = "1.0"; // 동일한 게임 버전을 설정
        PhotonNetwork.ConnectUsingSettings();
    }

    // Photon 서버에 연결된 후 호출됨.
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default); // 기본 로비에 연결
    }

    //로비에 입장한 후 실행.
    public override void OnJoinedLobby()
    {
        if (pendingAction == "방만들기")
        {
            roomManager.CreateRoom();
        }
        else if (pendingAction.Contains("입장하기"))
        {
            string roomCode = pendingAction.Replace("입장하기", "");
            roomManager.JoinRoom(roomCode);
        }
        else
        {
            Debug.LogError("pendingAction Text Error");
        }

        pendingAction = "";
    }

    // 매 프레임마다 호출됨.
    //void Update() { if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) PhotonNetwork.Disconnect(); }

    // Photon 서버와 연결이 끊길 경우 호출됨.
    public override void OnDisconnected(DisconnectCause cause)
    {
        //A.SetActive(false);
        Debug.Log("Disconnected from server.");
    }
}
