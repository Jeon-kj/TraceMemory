using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        buttonManager = FindObjectOfType<ButtonManager>();
        playerReady = FindObjectOfType<PlayerReady>();
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
        buttonManager.ReadyToStartGame();
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
}
