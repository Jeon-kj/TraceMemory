using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private string roomCode;
    private int playerMaxNumber = 0;

    // 게임 진행 관련 sign
    private bool signMG1 = false;
    private bool signMG2 = false;

    PreLifeManager preLifeManager;
    ButtonManager buttonManager;
    PlayerReady playerReady;
    PreGameCanvas preGameCanvas;
    CanvasManager canvasManager;

    private void Awake()
    {
        // 싱글턴 인스턴스 설정
        if (Instance == null)
        {
            Instance = this; // 현재 인스턴스를 설정
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 중복된 인스턴스가 생성되지 않도록 파괴
        }

        preLifeManager = FindObjectOfType<PreLifeManager>();
        buttonManager = GameObject.FindWithTag("MainButtonManager").GetComponent<ButtonManager>();
        playerReady = FindObjectOfType<PlayerReady>();
        preGameCanvas = FindObjectOfType<PreGameCanvas>();
        canvasManager = FindObjectOfType<CanvasManager>();
    }

    public void CheckIfAllPlayersReady()
    {
        if (playerReady.AreAllPlayersReady())
        {
            SetSceneIdentity();
        }
        else
        {
            Debug.Log("Not all players are ready.");
        }
    }

    public void SetSceneIdentity()
    {
        preGameCanvas.ReadyToStartGame();
        StartCoroutine(WaitAndExecute());
    }
    IEnumerator WaitAndExecute()
    {
        // 3초 대기
        yield return new WaitForSeconds(3f);

        preLifeManager.OnSetSceneIdentity();
    }

    public void SetRoomCode(string roomCode) { this.roomCode = roomCode; }
    public string GetRoomCode() { return roomCode; }

    public int GetPlayerMaxNumber() {  return playerMaxNumber; }

    public void SetPlayerMaxNumber(int n) { playerMaxNumber = n; }

    public void SetSignMG1(bool sign) { signMG1 = sign; }

    public bool GetSignMG1() { return signMG1; }

    public void SetSignMG2(bool sign) { signMG2 = sign; }

    public bool GetSignMG2() { return signMG2; }

    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LeaveRoom(true);  // 방 삭제 및 모든 플레이어 강제 퇴장
        }
        else
        {
            PhotonNetwork.LeaveRoom();  // 일반 플레이어는 그냥 방을 나감
        }

        StartCoroutine(WaitForLeaveAndLoadScene());

        canvasManager.TurnOffAndOn(canvasManager.AuxiliaryCanvas, canvasManager.PreGameCanvas);
        canvasManager.TurnOffAndOn(canvasManager.AlwaysOnCanvas, null);
        preGameCanvas.SetInit();
        preGameCanvas.SetActiveDisplay("nameInput", true);
    }

    private IEnumerator WaitForLeaveAndLoadScene()
    {
        // 방을 완전히 떠날 때까지 기다림
        while (PhotonNetwork.InRoom || PhotonNetwork.NetworkClientState == ClientState.Leaving)
        {
            yield return null;  // 한 프레임 대기
        }

        // Photon 서버와의 연결이 아직 살아있는지 확인
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // 씬이 존재하는지 확인하고 로드
            if (SceneManager.GetSceneByName("LobbyScene") != null)
            {
                PhotonNetwork.LoadLevel("LobbyScene"); // 로비 씬으로 이동
            }
            else
            {
                ErrorCanvas.Instance.ShowErrorMessage("LobbyScene is missing from Build Settings!", () =>
                {
                    Debug.LogError("LobbyScene is missing from Build Settings!");
                });                
            }
        }
        else
        {
            Debug.LogWarning("Photon is not connected. Disconnecting...");
            PhotonNetwork.Disconnect();
        }
    }
}
