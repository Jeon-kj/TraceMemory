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

    // ���� ���� ���� sign
    private bool signMG1 = false;
    private bool signMG2 = false;

    PreLifeManager preLifeManager;
    ButtonManager buttonManager;
    PlayerReady playerReady;
    PreGameCanvas preGameCanvas;
    CanvasManager canvasManager;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this; // ���� �ν��Ͻ��� ����
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ��� �ν��Ͻ��� �������� �ʵ��� �ı�
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
        // 3�� ���
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
            PhotonNetwork.LeaveRoom(true);  // �� ���� �� ��� �÷��̾� ���� ����
        }
        else
        {
            PhotonNetwork.LeaveRoom();  // �Ϲ� �÷��̾�� �׳� ���� ����
        }

        StartCoroutine(WaitForLeaveAndLoadScene());

        canvasManager.TurnOffAndOn(canvasManager.AuxiliaryCanvas, canvasManager.PreGameCanvas);
        canvasManager.TurnOffAndOn(canvasManager.AlwaysOnCanvas, null);
        preGameCanvas.SetInit();
        preGameCanvas.SetActiveDisplay("nameInput", true);
    }

    private IEnumerator WaitForLeaveAndLoadScene()
    {
        // ���� ������ ���� ������ ��ٸ�
        while (PhotonNetwork.InRoom || PhotonNetwork.NetworkClientState == ClientState.Leaving)
        {
            yield return null;  // �� ������ ���
        }

        // Photon �������� ������ ���� ����ִ��� Ȯ��
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // ���� �����ϴ��� Ȯ���ϰ� �ε�
            if (SceneManager.GetSceneByName("LobbyScene") != null)
            {
                PhotonNetwork.LoadLevel("LobbyScene"); // �κ� ������ �̵�
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
