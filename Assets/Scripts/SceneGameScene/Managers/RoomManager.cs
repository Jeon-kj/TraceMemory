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
    
    // 방만들기
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // 방의 최대 플레이어 수 설정
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
                char randomChar = (char)UnityEngine.Random.Range(65, 91); // 'A'의 아스키 코드가 65, 'Z'는 90
                roomCode += randomChar;
            }

            // 무작위 방 이름 생성 ("ABC" + 3자리 무작위 숫자)
            roomCode += UnityEngine.Random.Range(000, 1000).ToString();

            // 현재 존재하는 방 목록을 확인
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
        // 문제 가능성 1. 로비에 들어올 때 방 목록이 자동으로 갱신되긴 하지만 그게 "로비 들어온 직후"라고 하기는 어렵다.
        //              그렇기 때문에 새로운 플레이어가 방 목록을 불러오는 데까지 얼마나 걸리는 지가 중요함.
        //              너무 오래 걸린다면 새로운 방식의 설계가 필요할 것.
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
        Debug.LogError("Failed to create the room: " + message);
    }

    // 방 입장하기
    public void JoinRoom(string roomCode)
    {
        PhotonNetwork.JoinRoom(roomCode);
        this.roomCode = roomCode;
    }

    // 방 입장하기 이후 호출됨.
    public override async void OnJoinedRoom()
    {
        playerReady.OnJoinedRoom();
        prePlayerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();

        Debug.Log("Joined the room successfully.");

        await SendImageToStorage();

        // 방에 입장한 후 처리
        string playerName = playerProperties.GetName(); ; // 예: 플레이어 이름 가져오기
        string playerGender = playerProperties.GetGender(); // 예: 플레이어 성별 가져오기
        string playerImageFileName = playerProperties.GetImageFileName(); // 예: 플레이어 이미지 URL 가져오기

        SavePlayerInfo(playerName, playerGender, playerImageFileName);

        roomDisplay.RoomCodeUpdate(roomCode);
        buttonManager.OnRoomJoined();
    }

    async Task SendImageToStorage()
    {
        // 프로필 이미지로 설정된 이미지를 가져와서 firebase storage에 저장하고 URL을 가져와서 playerProperties에 저장함.
        Texture2D profileImage = playerProperties.GetImage();

        // 방에 입장 한 후로 바꿔야 오류가 발생하지 않음.
        string fileName = $"{PhotonNetwork.CurrentRoom.Name}/{playerProperties.GetName()}_ProfileImage.png";

        bool isSuccessed = await Uploader.UploadImage(profileImage.EncodeToPNG(), fileName); // await로 비동기 작업 처리

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
        Debug.LogError($"Failed to join the room: {message} (Return Code: {returnCode})");
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
        buttonManager.SetInit();        
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

