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
    private PreGameCanvas preGameCanvas;

    private void Awake()
    {
        Screen.SetResolution(1080, 1920, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        preGameCanvas = FindObjectOfType<PreGameCanvas>();
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon ������ ����.
    public void Connect() 
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InLobby)
            {
                Debug.Log("Already in the lobby.");
                return;
            }
            else
            {
                PhotonNetwork.JoinLobby(TypedLobby.Default);
                return;
            }
        }

        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    // Photon ������ ����� �� ȣ���.
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        preGameCanvas.SetActiveDisplay("loading", false);
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
