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
using UnityEngine.Analytics;


public class RoomManager : MonoBehaviourPunCallbacks
{
    public Text debugText;
    public Text playerListText;

    private PlayerProperties playerProperties;
    private ButtonManager buttonManager;
    private Uploader uploader;
    private Loader loader;
    private RoomDisplay roomDisplay;
    private PlayerReady playerReady;
    private NetworkManager networkManager;
    private PreGameCanvas preGameCanvas;
    private ErrorCanvas errorCanvas;

    public List<int> prePlayerOrder;

    private List<RoomInfo> roomList = new List<RoomInfo>();

    string roomCode = "";

    private void Awake()
    {
        playerProperties = FindObjectOfType<PlayerProperties>();
        buttonManager = GameObject.FindWithTag("MainButtonManager").GetComponent<ButtonManager>(); // ��� ������Ʈ�� �Ҵ�Ǿ� �ִ� ButtonManager�� Ȯ�������� Ž���ϱ� ���ؼ�.
        uploader = FindObjectOfType<Uploader>();
        loader = FindObjectOfType<Loader>();
        roomDisplay = FindObjectOfType<RoomDisplay>();
        playerReady = FindObjectOfType<PlayerReady>();
        networkManager = FindObjectOfType<NetworkManager>();
        preGameCanvas = FindObjectOfType<PreGameCanvas>();
        errorCanvas = FindObjectOfType<ErrorCanvas>();
    }
    
    // �游���
    public void CreateRoom()
    {
        string gender = playerProperties.GetGender();
        int maxPlayer = GameManager.Instance.GetPlayerMaxNumber();
        roomCode = GenerateRoomCode();
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayer,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "RoomCode", roomCode },
                { "MaleCount", gender == "Male" ? 1 : 0 },
                { "FemaleCount", gender == "Female" ? 1 : 0 },
                { "MaxPlayer", maxPlayer }
            },
            CustomRoomPropertiesForLobby = new string[] { "RoomCode", "MaleCount", "FemaleCount", "MaxPlayer" }
        };
        //uploader.UploadPlayerMaxNumber(roomCode, maxPlayer);
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
        preGameCanvas.OnRoomFailed();
    }

    public void TryJoinRoom(string roomCode, string gender)
    {
        if (!PhotonNetwork.InLobby)
        {
            Debug.LogWarning("Not in lobby, cannot check room list.");
            return;
        }

        RoomInfo targetRoom = null;

        // ���� �κ񿡼� �� ����Ʈ ��������
        foreach (RoomInfo room in roomList)
        {
            DebugCanvas.Instance.DebugLog($"room Code: {room.CustomProperties["RoomCode"]}");
            if (room.CustomProperties.ContainsKey("RoomCode") && (string)room.CustomProperties["RoomCode"] == roomCode)
            {
                targetRoom = room;
                break;
            }
        }

        if (targetRoom == null)
        {
            Debug.LogError("Room not found.");
            // UI ����ó��.
            return;
        }

        // ���� ���� ���� ���� Ȯ��
        int maleCount = targetRoom.CustomProperties.ContainsKey("MaleCount") ? (int)targetRoom.CustomProperties["MaleCount"] : 0;
        int femaleCount = targetRoom.CustomProperties.ContainsKey("FemaleCount") ? (int)targetRoom.CustomProperties["FemaleCount"] : 0;
        int maxPlayer = (int)targetRoom.CustomProperties["MaxPlayer"];

        if ((gender == "Male" && maleCount >= maxPlayer/2) || (gender == "Female" && femaleCount >= maxPlayer/2))
        {
            // UI ����ó��.
            errorCanvas.ShowErrorMessage($"�ش� �� {roomCode}�� ������ �� �����ϴ�. : {gender}ĭ ���� �ʰ�", () =>
            {
                preGameCanvas.OnRoomFailed();
            });
            return; 
        }

        // ������ �����ϸ� �濡 ����
        PhotonNetwork.JoinRoom(roomCode);
        this.roomCode = roomCode;
    }

    // �� �����ϱ� ���� ȣ���.
    public override async void OnJoinedRoom()
    {
        // �� �ڵ� ����.
        GameManager.Instance.SetRoomCode(roomCode);

        // �� �ִ��ο� ����.
        //int maxPlayer = await loader.LoadPlayerMaxNumber();
        int maxPlayer = PhotonNetwork.CurrentRoom.GetMaxPlayerNumber();
        GameManager.Instance.SetPlayerMaxNumber(maxPlayer);

        // �÷��̾� �غ� ���� ������Ʈ
        playerReady.OnJoinedRoom();
        prePlayerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();

        Debug.Log("Joined the room successfully.");

        await SendImageToStorage();

        // �濡 ������ �� ó��
        string playerName = playerProperties.GetName(); ; // ��: �÷��̾� �̸� ��������
        string playerGender = playerProperties.GetGender(); // ��: �÷��̾� ���� ��������
        string playerImageFileName = playerProperties.GetImageFileName(); // ��: �÷��̾� �̹��� URL ��������

        // �� ���� �� ���� �ο� ���� ������Ʈ
        UpdateRoomGenderCount(playerGender);

        // �÷��̾� ���� ����
        SavePlayerInfo(playerName, playerGender, playerImageFileName);

        // UI ������Ʈ
        roomDisplay.RoomCodeUpdate(roomCode);
        preGameCanvas.OnRoomJoined();
    }

    public void UpdateRoomGenderCount(string gender)
    {
        if (!PhotonNetwork.InRoom) return;

        // ���� Room Properties ��������
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        int maleCount = properties.ContainsKey("MaleCount") ? (int)properties["MaleCount"] : 0;
        int femaleCount = properties.ContainsKey("FemaleCount") ? (int)properties["FemaleCount"] : 0;

        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
        ExitGames.Client.Photon.Hashtable expectedProperties = new ExitGames.Client.Photon.Hashtable();

        if (gender == "Male")
        {
            newProperties["MaleCount"] = maleCount + 1;
            expectedProperties["MaleCount"] = maleCount; // ���� ���� ������ ���� ���ƾ� ������Ʈ ����
        }
        else
        {
            newProperties["FemaleCount"] = femaleCount + 1;
            expectedProperties["FemaleCount"] = femaleCount; // ���� ���� ������ ���� ���ƾ� ������Ʈ ����
        }

        // expectedProperties�� ���� CustomProperties�� ��ġ�ؾ߸� newProperties�� �����.
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties, expectedProperties);
    }

    async Task SendImageToStorage()
    {
        // ������ �̹����� ������ �̹����� �����ͼ� firebase storage�� �����ϰ� URL�� �����ͼ� playerProperties�� ������.
        Texture2D profileImage = playerProperties.GetImage();

        // �濡 ���� �� �ķ� �ٲ�� ������ �߻����� ����.
        string fileName = $"{PhotonNetwork.CurrentRoom.Name}/{playerProperties.GetName()}_ProfileImage.png";

        bool isSuccessed = await uploader.UploadImage(profileImage.EncodeToPNG(), fileName); // await�� �񵿱� �۾� ó��

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
        preGameCanvas.OnRoomFailed();
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
        preGameCanvas.ReadyButtonInit();        
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

