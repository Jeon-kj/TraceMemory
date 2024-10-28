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
        if(text == "�غ�")
        {
            SetPlayerReady();
        }
        else if(text == "���")
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

        // ������ ������ �迭�̳� ����Ʈ�� �����ɴϴ�.
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


        // �� Ŀ���� ������Ƽ�� �ٽ� ����
        customProps["PlayerReady"] = playerReady.ToArray(); // �迭�� ����
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
    }

    public bool AreAllPlayersReady()
    {
        //return prePlayerReady.Count == PhotonNetwork.CurrentRoom.MaxPlayers;
        return prePlayerReady.Count == 2;
    }
}

// �� �� :
// �غ� �� �Ǹ� �����ϱ�.