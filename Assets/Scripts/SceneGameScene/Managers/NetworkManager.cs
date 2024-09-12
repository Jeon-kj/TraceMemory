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

    // Photon ������ ����.
    public void Connect(string action) 
    {
        pendingAction = action;
        PhotonNetwork.GameVersion = "1.0"; // ������ ���� ������ ����
        PhotonNetwork.ConnectUsingSettings();
    }

    // Photon ������ ����� �� ȣ���.
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default); // �⺻ �κ� ����
    }

    //�κ� ������ �� ����.
    public override void OnJoinedLobby()
    {
        if (pendingAction == "�游���")
        {
            roomManager.CreateRoom();
        }
        else if (pendingAction.Contains("�����ϱ�"))
        {
            string roomCode = pendingAction.Replace("�����ϱ�", "");
            roomManager.JoinRoom(roomCode);
        }
        else
        {
            Debug.LogError("pendingAction Text Error");
        }

        pendingAction = "";
    }

    // �� �����Ӹ��� ȣ���.
    //void Update() { if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) PhotonNetwork.Disconnect(); }

    // Photon ������ ������ ���� ��� ȣ���.
    public override void OnDisconnected(DisconnectCause cause)
    {
        //A.SetActive(false);
        Debug.Log("Disconnected from server.");
    }
}
