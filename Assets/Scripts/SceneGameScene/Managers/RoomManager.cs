using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System.Linq;
using System;


public class RoomManager : MonoBehaviourPunCallbacks
{
    public Text debugText;
    public Text playerListText;

    private PlayerProperties playerProperties;
    private ButtonManager buttonManager;
    private Uploader Uploader;
    private RoomDisplay roomDisplay;
    private PlayerReady playerReady;

    public List<int> prePlayerOrder;

    private List<RoomInfo> roomList = new List<RoomInfo>();

    string roomCode = "";

    private void Awake()
    {
        playerProperties = FindObjectOfType<PlayerProperties>();
        buttonManager = FindObjectOfType<ButtonManager>();
        Uploader = FindObjectOfType<Uploader>();
        roomDisplay = FindObjectOfType<RoomDisplay>();
        playerReady = FindObjectOfType<PlayerReady>();
    }
    
    // �游���
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // ���� �ִ� �÷��̾� �� ����
        roomCode = GenerateRoomCode();
        PhotonNetwork.CreateRoom(roomCode, roomOptions, TypedLobby.Default);
    }

    private string GenerateRoomCode()
    {
        string roomCode = "";

        bool isUnique = false;

        while (!isUnique)
        {
            for (int i = 0; i < 3; i++)
            {
                char randomChar = (char)UnityEngine.Random.Range(65, 91); // 'A'�� �ƽ�Ű �ڵ尡 65, 'Z'�� 90
                roomCode += randomChar;
            }

            // ������ �� �̸� ���� ("ABC" + 3�ڸ� ������ ����)
            roomCode += UnityEngine.Random.Range(000, 1000).ToString();

            // ���� �����ϴ� �� ����� Ȯ��
            isUnique = IsRoomNameUnique(roomCode);
        }
        GameManager.Instance.SetRoomCode(roomCode);
        return roomCode;
    }

    private bool IsRoomNameUnique(string roomCode)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.Name == roomCode)
            {
                return false; // ������ �� �̸��� ������ false ��ȯ
            }
        }
        return true; // �ߺ��� ������ true ��ȯ
    }

    // �ݹ�: �� ����� ������Ʈ�� �� ȣ��
    public override void OnRoomListUpdate(List<RoomInfo> updatedRoomList)
    {
        roomList.Clear();
        roomList.AddRange(updatedRoomList);
        Debug.Log("�� ��� ������Ʈ��. �� �� ��: " + roomList.Count);
        // ���� ���ɼ� 1. �κ� ���� �� �� ����� �ڵ����� ���ŵǱ� ������ �װ� "�κ� ���� ����"��� �ϱ�� ��ƴ�.
        //              �׷��� ������ ���ο� �÷��̾ �� ����� �ҷ����� ������ �󸶳� �ɸ��� ���� �߿���.
        //              �ʹ� ���� �ɸ��ٸ� ���ο� ����� ���谡 �ʿ��� ��.
    }

    // �游��� ���� ȣ���.
    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully.");
        // ���� ������ �� ó��
    }

    // �� �����ϱ� ���� �� ȣ���.
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create the room: " + message);
    }

    // �� �����ϱ�
    public void JoinRoom(string roomCode)
    {
        PhotonNetwork.JoinRoom(roomCode);
        this.roomCode = roomCode;
    }

    // �� �����ϱ� ���� ȣ���.
    public override async void OnJoinedRoom()
    {
        playerReady.OnJoinedRoom();
        prePlayerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();

        Debug.Log("Joined the room successfully.");

        await SendImageToStorage();

        // �濡 ������ �� ó��
        string playerName = playerProperties.GetName(); ; // ��: �÷��̾� �̸� ��������
        string playerGender = playerProperties.GetGender(); // ��: �÷��̾� ���� ��������
        string playerImageFileName = playerProperties.GetImageFileName(); // ��: �÷��̾� �̹��� URL ��������

        SavePlayerInfo(playerName, playerGender, playerImageFileName);

        roomDisplay.RoomCodeUpdate(roomCode);
        buttonManager.OnRoomJoined();
    }

    async Task SendImageToStorage()
    {
        // ������ �̹����� ������ �̹����� �����ͼ� firebase storage�� �����ϰ� URL�� �����ͼ� playerProperties�� ������.
        Texture2D profileImage = playerProperties.GetImage();

        // �濡 ���� �� �ķ� �ٲ�� ������ �߻����� ����.
        string fileName = $"{PhotonNetwork.CurrentRoom.Name}/{playerProperties.GetName()}_ProfileImage.png";

        bool isSuccessed = await Uploader.UploadImage(profileImage.EncodeToPNG(), fileName); // await�� �񵿱� �۾� ó��

        if (isSuccessed)
        {
            Debug.Log("File Name : " + fileName);
            playerProperties.SetImageFileName(fileName);
        }
        else
        {
            Debug.LogError("Failed to upload image");
        }
    }

    void SavePlayerInfo(string playerName, string playerGender, string playerImageFileName)
    {
        ExitGames.Client.Photon.Hashtable playerPropertiesHashtable = new ExitGames.Client.Photon.Hashtable
        {
            { $"Name", playerName },
            { $"Gender", playerGender },
            { $"ImageFileName", playerImageFileName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerPropertiesHashtable);
    }

    // �� �����ϱ� ���� �� ȣ���.
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join the room: {message} (Return Code: {returnCode})");
        // �� ���� ���� ó��
    }

    // Ŀ���� ������Ƽ�� ������Ʈ�� �� ȣ��Ǵ� �ݹ�
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Name") && changedProps.ContainsKey("Gender") && changedProps.ContainsKey("ImageFileName"))
        {
            // ���� ���ӵ� �÷��̾� ����� ������Ʈ
            debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : SavePlayerInfo\n";
            OnPlayerEnteredUpdateRoomProperties(targetPlayer.ActorNumber);
        }        
    }

    void OnPlayerEnteredUpdateRoomProperties(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.AddPlayerOrder(actorNumber);
    }

    // �÷��̾ �� ������ ������ ȣ���.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) // ��ΰ� �����ϸ� ������ ��Ŀ����������Ƽ ������Ʈ�� ������ �߻��ؼ� ��ȿ����.
        {
            OnPlayerLeftUpdateRoomProperties(otherPlayer.ActorNumber);
            playerReady.CancelPlayerReady(otherPlayer.ActorNumber); // 
        }
        roomDisplay.OnLeftRoom(otherPlayer);
    }

    void OnPlayerLeftUpdateRoomProperties(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.DelPlayerOrder(actorNumber);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("PlayerOrder"))
        {         
            List<int> currentPlayerOrder = ((int[])propertiesThatChanged["PlayerOrder"]).ToList();

            // ���� ���¿� ��
            if (!AreListsEqual(prePlayerOrder, currentPlayerOrder))
            {
                debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : UpdatePlayerOrderDisplay\n";
                prePlayerOrder = new List<int>(currentPlayerOrder);  // �ֽ� ���·� ������Ʈ
                roomDisplay.UpdatePlayerOrderDisplay();
            }
        }

        if (propertiesThatChanged.ContainsKey("PlayerReady"))
        {
            List<int> currentPlayerReady = ((int[])propertiesThatChanged["PlayerReady"]).ToList();

            // ���� ���¿� ��
            if (!AreListsEqual(playerReady.prePlayerReady, currentPlayerReady))
            {
                debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : UpdatePlayerReadyStatus\n";
                playerReady.prePlayerReady = new List<int>(currentPlayerReady);  // �ֽ� ���·� ������Ʈ
                roomDisplay.UpdatePlayerReadyStatus();
                GameManager.Instance.CheckIfAllPlayersReady(); // @@�÷��̾���� �غ���� Ȯ���ϰ�, ���� ����@@
            }
        }
    }

    bool AreListsEqual(List<int> list1, List<int> list2)
    {
        if (list1 == null || list2 == null) return false;
        if (list1.Count != list2.Count) return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i]) return false;
        }
        return true;
    }

    public override void OnLeftRoom()
    {
        buttonManager.SetInit();        
        roomDisplay.OnLeftRoom();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // ���ο� ������ Ŭ���̾�Ʈ�� �Ǿ��� �� ���� �۾��� ����
        if (PhotonNetwork.IsMasterClient)
        {
            CleanUpPlayerOrder();
            playerReady.CleanUpPlayerReady();
        }
    }

    void CleanUpPlayerOrder()
    {
        ExitGames.Client.Photon.Hashtable customProps = PhotonNetwork.CurrentRoom.CustomProperties;

        // ������ ������ �迭�̳� ����Ʈ�� �����ɴϴ�.
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();


        List<int> playersToRemove = new List<int>();

        foreach (int actorNumber in playerOrder)
        {
            Player targetPlayer = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == actorNumber);

            if (targetPlayer == null)
            {
                playersToRemove.Add(actorNumber);
            }
        }

        foreach (int actorNumber in playersToRemove)
        {
            playerOrder.Remove(actorNumber);
        }


        // �� Ŀ���� ������Ƽ�� �ٽ� ����
        customProps["PlayerOrder"] = playerOrder.ToArray(); // �迭�� ����
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
    }
}

