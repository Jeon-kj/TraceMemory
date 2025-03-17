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
        buttonManager = GameObject.FindWithTag("MainButtonManager").GetComponent<ButtonManager>(); // 모든 오브젝트가 할당되어 있는 ButtonManager를 확정적으로 탐색하기 위해서.
        uploader = FindObjectOfType<Uploader>();
        loader = FindObjectOfType<Loader>();
        roomDisplay = FindObjectOfType<RoomDisplay>();
        playerReady = FindObjectOfType<PlayerReady>();
        networkManager = FindObjectOfType<NetworkManager>();
        preGameCanvas = FindObjectOfType<PreGameCanvas>();
        errorCanvas = FindObjectOfType<ErrorCanvas>();
    }
    
    // 방만들기
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
                char randomChar = (char)UnityEngine.Random.Range(65, 91); // 'A'의 아스키 코드가 65, 'Z'는 90
                roomCode += randomChar;
            }

            // 무작위 방 이름 생성 ("ABC" + 3자리 무작위 숫자)
            roomCode += UnityEngine.Random.Range(000, 1000).ToString();

            // 현재 존재하는 방 목록을 확인
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
                return false; // 동일한 방 이름이 있으면 false 반환
            }
        }
        return true; // 중복이 없으면 true 반환
    }

    // 콜백: 방 목록이 업데이트될 때 호출
    public override void OnRoomListUpdate(List<RoomInfo> updatedRoomList)
    {
        roomList.Clear();
        roomList.AddRange(updatedRoomList);
        Debug.Log("방 목록 업데이트됨. 총 방 수: " + roomList.Count);
    }

    // 방만들기 이후 호출됨.
    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully.");
        // 방이 생성된 후 처리
    }

    // 방 입장하기 실패 시 호출됨.
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

        // 현재 로비에서 방 리스트 가져오기
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
            // UI 예외처리.
            return;
        }

        // 방의 현재 남녀 정원 확인
        int maleCount = targetRoom.CustomProperties.ContainsKey("MaleCount") ? (int)targetRoom.CustomProperties["MaleCount"] : 0;
        int femaleCount = targetRoom.CustomProperties.ContainsKey("FemaleCount") ? (int)targetRoom.CustomProperties["FemaleCount"] : 0;
        int maxPlayer = (int)targetRoom.CustomProperties["MaxPlayer"];

        if ((gender == "Male" && maleCount >= maxPlayer/2) || (gender == "Female" && femaleCount >= maxPlayer/2))
        {
            // UI 예외처리.
            errorCanvas.ShowErrorMessage($"해당 방 {roomCode}에 입장할 수 없습니다. : {gender}칸 정원 초과", () =>
            {
                preGameCanvas.OnRoomFailed();
            });
            return; 
        }

        // 입장이 가능하면 방에 들어가기
        PhotonNetwork.JoinRoom(roomCode);
        this.roomCode = roomCode;
    }

    // 방 입장하기 이후 호출됨.
    public override async void OnJoinedRoom()
    {
        // 방 코드 설정.
        GameManager.Instance.SetRoomCode(roomCode);

        // 방 최대인원 공유.
        //int maxPlayer = await loader.LoadPlayerMaxNumber();
        int maxPlayer = PhotonNetwork.CurrentRoom.GetMaxPlayerNumber();
        GameManager.Instance.SetPlayerMaxNumber(maxPlayer);

        // 플레이어 준비 상태 업데이트
        playerReady.OnJoinedRoom();
        prePlayerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();

        Debug.Log("Joined the room successfully.");

        await SendImageToStorage();

        // 방에 입장한 후 처리
        string playerName = playerProperties.GetName(); ; // 예: 플레이어 이름 가져오기
        string playerGender = playerProperties.GetGender(); // 예: 플레이어 성별 가져오기
        string playerImageFileName = playerProperties.GetImageFileName(); // 예: 플레이어 이미지 URL 가져오기

        // 방 입장 후 성별 인원 정보 업데이트
        UpdateRoomGenderCount(playerGender);

        // 플레이어 정보 저장
        SavePlayerInfo(playerName, playerGender, playerImageFileName);

        // UI 업데이트
        roomDisplay.RoomCodeUpdate(roomCode);
        preGameCanvas.OnRoomJoined();
    }

    public void UpdateRoomGenderCount(string gender)
    {
        if (!PhotonNetwork.InRoom) return;

        // 현재 Room Properties 가져오기
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        int maleCount = properties.ContainsKey("MaleCount") ? (int)properties["MaleCount"] : 0;
        int femaleCount = properties.ContainsKey("FemaleCount") ? (int)properties["FemaleCount"] : 0;

        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
        ExitGames.Client.Photon.Hashtable expectedProperties = new ExitGames.Client.Photon.Hashtable();

        if (gender == "Male")
        {
            newProperties["MaleCount"] = maleCount + 1;
            expectedProperties["MaleCount"] = maleCount; // 기존 값이 예상한 값과 같아야 업데이트 가능
        }
        else
        {
            newProperties["FemaleCount"] = femaleCount + 1;
            expectedProperties["FemaleCount"] = femaleCount; // 기존 값이 예상한 값과 같아야 업데이트 가능
        }

        // expectedProperties와 현재 CustomProperties가 일치해야만 newProperties가 적용됨.
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties, expectedProperties);
    }

    async Task SendImageToStorage()
    {
        // 프로필 이미지로 설정된 이미지를 가져와서 firebase storage에 저장하고 URL을 가져와서 playerProperties에 저장함.
        Texture2D profileImage = playerProperties.GetImage();

        // 방에 입장 한 후로 바꿔야 오류가 발생하지 않음.
        string fileName = $"{PhotonNetwork.CurrentRoom.Name}/{playerProperties.GetName()}_ProfileImage.png";

        bool isSuccessed = await uploader.UploadImage(profileImage.EncodeToPNG(), fileName); // await로 비동기 작업 처리

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

    // 방 입장하기 실패 시 호출됨.
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        preGameCanvas.OnRoomFailed();
        // 방 입장 실패 처리
    }

    // 커스텀 프로퍼티가 업데이트될 때 호출되는 콜백
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Name") && changedProps.ContainsKey("Gender") && changedProps.ContainsKey("ImageFileName"))
        {
            // 현재 접속된 플레이어 목록을 업데이트
            debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : SavePlayerInfo\n";
            OnPlayerEnteredUpdateRoomProperties(targetPlayer.ActorNumber);
        }        
    }

    void OnPlayerEnteredUpdateRoomProperties(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.AddPlayerOrder(actorNumber);
    }

    // 플레이어가 방 퇴장할 때마다 호출됨.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) // 모두가 실행하면 동일한 룸커스텀프로퍼티 업데이트가 여러번 발생해서 비효율적.
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

            // 이전 상태와 비교
            if (!AreListsEqual(prePlayerOrder, currentPlayerOrder))
            {
                debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : UpdatePlayerOrderDisplay\n";
                prePlayerOrder = new List<int>(currentPlayerOrder);  // 최신 상태로 업데이트
                roomDisplay.UpdatePlayerOrderDisplay();
            }
        }

        if (propertiesThatChanged.ContainsKey("PlayerReady"))
        {
            List<int> currentPlayerReady = ((int[])propertiesThatChanged["PlayerReady"]).ToList();

            // 이전 상태와 비교
            if (!AreListsEqual(playerReady.prePlayerReady, currentPlayerReady))
            {
                debugText.text += $"{PhotonNetwork.LocalPlayer.ActorNumber} : UpdatePlayerReadyStatus\n";
                playerReady.prePlayerReady = new List<int>(currentPlayerReady);  // 최신 상태로 업데이트
                roomDisplay.UpdatePlayerReadyStatus();
                GameManager.Instance.CheckIfAllPlayersReady(); // @@플레이어들의 준비상태 확인하고, 게임 시작@@
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
        // 새로운 마스터 클라이언트가 되었을 때 남은 작업을 수행
        if (PhotonNetwork.IsMasterClient)
        {
            CleanUpPlayerOrder();
            playerReady.CleanUpPlayerReady();
        }
    }

    void CleanUpPlayerOrder()
    {
        ExitGames.Client.Photon.Hashtable customProps = PhotonNetwork.CurrentRoom.CustomProperties;

        // 순서를 저장할 배열이나 리스트를 가져옵니다.
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


        // 룸 커스텀 프로퍼티에 다시 저장
        customProps["PlayerOrder"] = playerOrder.ToArray(); // 배열로 저장
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
    }
}

