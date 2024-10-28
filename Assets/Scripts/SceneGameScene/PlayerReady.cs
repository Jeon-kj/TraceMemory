using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerReady : MonoBehaviour
{
    public List<int> prePlayerReady;

    public void OnJoinedRoom()
    {
        prePlayerReady = PhotonNetwork.CurrentRoom.GetPlayerReadyList();
    }


    public void OnButtonClick(string text)
    {
        if(text == "준비")
        {
            SetPlayerReady();
        }
        else if(text == "취소")
        {
            CancelPlayerReady(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    void SetPlayerReady()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetwork.CurrentRoom.AddPlayerReady(actorNumber);
    }

    public void CancelPlayerReady(int actorNumber)
    {
        PhotonNetwork.CurrentRoom.DelPlayerReady(actorNumber);
    }

    public void CleanUpPlayerReady()
    {
        ExitGames.Client.Photon.Hashtable customProps = PhotonNetwork.CurrentRoom.CustomProperties;

        // 순서를 저장할 배열이나 리스트를 가져옵니다.
        List<int> playerReady = PhotonNetwork.CurrentRoom.GetPlayerReadyList();


        List<int> playersToRemove = new List<int>();

        foreach (int actorNumber in playerReady)
        {
            Player targetPlayer = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == actorNumber);

            if (targetPlayer == null)
            {
                playersToRemove.Add(actorNumber);
            }
        }

        foreach (int actorNumber in playersToRemove)
        {
            playerReady.Remove(actorNumber);
        }


        // 룸 커스텀 프로퍼티에 다시 저장
        customProps["PlayerReady"] = playerReady.ToArray(); // 배열로 저장
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
    }

    public bool AreAllPlayersReady()
    {
        //return prePlayerReady.Count == PhotonNetwork.CurrentRoom.MaxPlayers;
        return prePlayerReady.Count == 2;
    }
}

// 할 거 :
// 준비 다 되면 시작하기.