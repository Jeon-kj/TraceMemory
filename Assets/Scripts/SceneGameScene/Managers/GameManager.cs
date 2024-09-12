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
        // 3�� ���
        yield return new WaitForSeconds(3f);

        preLifeManager.OnSetSceneIdentity();
    }

    public void SetRoomCode(string roomCode) { roomCode = this.roomCode; }
    public string GetRoomCode() { return roomCode; }
}
