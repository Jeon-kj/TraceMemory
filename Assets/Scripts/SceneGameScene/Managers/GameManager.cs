using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private string roomCode;

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

    public void SetRoomCode(string roomCode) { roomCode = this.roomCode; }
    public string GetRoomCode() { return roomCode; }
}
