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
    }
    
    // �游���
    public void CreateRoom()
    {
        int maxPlayer = GameManager.Instance.GetPlayerMaxNumber();
        roomCode = GenerateRoomCode();
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayer,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "RoomCode", roomCode },
                { "MaleCount", 0 },
                { "FemaleCount", 0 },
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
            ErrorCanvas.Instance.ShowErrorMessage("���� ã�� �� �����ϴ�.", () =>
            {
                Debug.LogError("Can't Find Room");
                preGameCanvas.SetActiveDisplay("loading", false);
            });
            return;
        }

        // ���� ���� ���� ���� Ȯ��
        int maleCount = targetRoom.CustomProperties.ContainsKey("MaleCount") ? (int)targetRoom.CustomProperties["MaleCount"] : 0;
        int femaleCount = targetRoom.CustomProperties.ContainsKey("FemaleCount") ? (int)targetRoom.CustomProperties["FemaleCount"] : 0;
        int maxPlayer = (int)targetRoom.CustomProperties["MaxPlayer"];

        DebugCanvas.Instance.DebugLog($"maleCount :: {maleCount}");
        DebugCanvas.Instance.DebugLog($"femaleCount :: {femaleCount}");
        DebugCanvas.Instance.DebugLog($"maxPlayer :: {maxPlayer}");

        if ((gender == "Male" && maleCount >= maxPlayer/2) || (gender == "Female" && femaleCount >= maxPlayer/2))
        {
            // UI ����ó��.
            ErrorCanvas.Instance.ShowErrorMessage($"�ش� �� {roomCode}�� ������ �� �����ϴ�. : {gender}ĭ ���� �ʰ�", () =>
            {
                Debug.LogError("Can't Join Room");
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
        int playerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // �� ���� �� ���� �ο� ���� ������Ʈ
        UpdateRoomGenderCount(playerGender);

        // �÷��̾� ���� ����
        SavePlayerInfo(playerName, playerGender, playerImageFileName);

        // UI ������Ʈ
        roomDisplay.UpdatePlayerOrderDisplay();
        //roomDisplay.UpdatePlayerReadyStatus();
        playerReady.RegisterPlayerReadyStatus(playerActorNumber);
        roomDisplay.RoomCodeUpdate(roomCode);
        preGameCanvas.OnRoomJoined();
    }

    public void UpdateRoomGenderCount(string gender, bool decrease = false)
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
            newProperties["MaleCount"] = decrease ? Math.Max(0, maleCount - 1) : maleCount + 1;
            expectedProperties["MaleCount"] = maleCount; // ���� ���� ������ ���� ���ƾ� ������Ʈ ����
        }
        else
        {
            newProperties["FemaleCount"] = decrease ? Math.Max(0, femaleCount - 1) : femaleCount + 1;
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
            ErrorCanvas.Instance.ShowErrorMessage("�̹��� ���ε� ����.", () => {
                Debug.LogError("Image File Upload Failed");
            });
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        DebugCanvas.Instance.DebugLog($"OnPlayerEnteredRoom come in!");
        roomDisplay.UpdatePlayerOrderDisplay();
        //roomDisplay.UpdatePlayerReadyStatus();
    }

    // �÷��̾ �� ������ ������ ȣ���.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string leftPlayerGender = otherPlayer.GetPlayerGender();
        DebugCanvas.Instance.DebugLog($"leftPlayerGender :: {leftPlayerGender}");

        if (PhotonNetwork.IsMasterClient) // ��ΰ� �����ϸ� ������ ��Ŀ����������Ƽ ������Ʈ�� ������ �߻��ؼ� ��ȿ����.
        {
            UpdateRoomGenderCount(leftPlayerGender, decrease: true);    // �ۿ��� ���� �� �ο� �� ���ҽ�Ű��
            OnPlayerLeftUpdateRoomProperties(otherPlayer.ActorNumber);
            playerReady.DelPlayerReady(otherPlayer.ActorNumber); // 
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
                roomDisplay.UpdatePlayerReadyStatus();
            }
        }

        if (propertiesThatChanged.ContainsKey("PlayerReady"))
        {
            DebugCanvas.Instance.DebugLog("OnRoomPropertiesUpdate :: PlayerReady");
            ExitGames.Client.Photon.Hashtable updated = (ExitGames.Client.Photon.Hashtable)propertiesThatChanged["PlayerReady"];
            Dictionary<int, bool> updatedDict = new();

            foreach (DictionaryEntry entry in updated)
            {
                if (int.TryParse(entry.Key.ToString(), out int actorNum))
                {
                    updatedDict[actorNum] = (bool)entry.Value;
                }
            }

            if (!AreDictionariesEqual(playerReady.GetPrePlayerReady, updatedDict))
            {
                playerReady.SetPrePlayerReady(new Dictionary<int, bool>(updatedDict));
                roomDisplay.UpdatePlayerReadyStatus();
                GameManager.Instance.CheckIfAllPlayersReady();
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

    bool AreDictionariesEqual(Dictionary<int, bool> a, Dictionary<int, bool> b)
    {
        if (a.Count != b.Count) return false;

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out bool val) || val != kvp.Value)
                return false;
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

