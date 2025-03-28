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
        if(text == "준비")
        {
            SetPlayerReady(true);
        }
        else if(text == "취소")
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

        // 현재 방에 있는 플레이어 ActorNumber 목록 (string)
        var currentPlayers = new HashSet<string>(PhotonNetwork.PlayerList.Select(p => p.ActorNumber.ToString()));


        // 기존 준비된 플레이어 딕셔너리 가져오기
        var readyDict = GetPlayerReadyDictionary();

        // 남아 있는 플레이어만 걸러냄
        var cleanedDict = readyDict
            .Where(kvp => currentPlayers.Contains(kvp.Key.ToString()))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // 다시 저장
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerReady", cleanedDict } });
    }

    public bool AreAllPlayersReady()
    {
        int cnt = 0;
        var readyDict = GetPlayerReadyDictionary();

        // 현재 방에 있는 모든 플레이어에 대해 준비 여부 확인
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int actorNumber = player.ActorNumber;

            if (readyDict.TryGetValue(actorNumber, out bool isReady) && isReady)
            {
                Debug.Log($"플레이어 {actorNumber} 준비 됨");
                cnt++;
            }
        }

        if (cnt == GameManager.Instance.GetPlayerMaxNumber())
        {
            Debug.Log("모든 플레이어 준비 완료");
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