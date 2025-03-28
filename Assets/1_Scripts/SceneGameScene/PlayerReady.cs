using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerReady : MonoBehaviour
{
    Dictionary<int, bool> prePlayerReady;

    public void OnJoinedRoom()
    {
        prePlayerReady = GetPlayerReadyDictionary();
    }


    public void OnButtonClick(string text)
    {
        if(text == "�غ�")
        {
            SetPlayerReady(true);
        }
        else if(text == "���")
        {
            SetPlayerReady(false);
        }
    }

    public void RegisterPlayerReadyStatus(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.RegisterPlayerReadyStatus(actorNumber);
    }

    public void DelPlayerReady(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.DelPlayerReady(actorNumber);
    }

    void SetPlayerReady(bool sign)
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetwork.CurrentRoom.SetPlayerReadyStatus(actorNumber, sign);
    }

    public void CleanUpPlayerReady()
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
            return;

        // ���� �濡 �ִ� �÷��̾� ActorNumber ��� (string)
        var currentPlayers = new HashSet<string>(PhotonNetwork.PlayerList.Select(p => p.ActorNumber.ToString()));


        // ���� �غ�� �÷��̾� ��ųʸ� ��������
        var readyDict = GetPlayerReadyDictionary();

        // ���� �ִ� �÷��̾ �ɷ���
        var cleanedDict = readyDict
            .Where(kvp => currentPlayers.Contains(kvp.Key.ToString()))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // �ٽ� ����
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerReady", cleanedDict } });
    }

    public bool AreAllPlayersReady()
    {
        int cnt = 0;
        var readyDict = GetPlayerReadyDictionary();

        // ���� �濡 �ִ� ��� �÷��̾ ���� �غ� ���� Ȯ��
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int actorNumber = player.ActorNumber;

            if (readyDict.TryGetValue(actorNumber, out bool isReady) && isReady)
            {
                Debug.Log($"�÷��̾� {actorNumber} �غ� ��");
                cnt++;
            }
        }

        if (cnt == GameManager.Instance.GetPlayerMaxNumber())
        {
            Debug.Log("��� �÷��̾� �غ� �Ϸ�");
            return true;
        }
        else
        {
            return false;
        }
    }

    public Dictionary<int, bool> GetPrePlayerReady { get { return prePlayerReady; } }

    public void SetPrePlayerReady(Dictionary<int, bool> dict)
    {
        prePlayerReady = dict;
    }

    public Dictionary<int, bool> GetPlayerReadyDictionary()
    {
        return PhotonNetwork.CurrentRoom.GetPlayerReadyDictionary();
    }
}