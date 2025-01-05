using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Data;
using System;

public class PreLifeManager : MonoBehaviourPunCallbacks
{
    private bool preLifesAssigned = false;
    List<string> males = new List<string> { "몽룡", "견우", "온달" };
    List<string> females = new List<string> { "춘향", "직녀", "평강" };

    SceneController sceneController;

    private void Awake()
    {
        sceneController = FindObjectOfType<SceneController>();
    }

    public void OnSetSceneIdentity()
    {
        if (PhotonNetwork.IsMasterClient && !preLifesAssigned)
        {
            AssignPreLifes();
        }
    }

    private void AssignPreLifes()
    {
        Shuffle(males);
        Shuffle(females);

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            List<string> list = player.GetPlayerGender() == "Male" ? males : females;
            string preLifeName = list[0];
            list.RemoveAt(0);
            try
            {
                Debug.Log($"photonView : {photonView}");
                photonView.RPC("ReceivePreLife", player, preLifeName);  // 각 플레이어에게 역할 전달
            }
            catch(Exception e)
            {
                Debug.Log("Error Detect : " + e);
            }
            
        }

        preLifesAssigned = true;  // 역할 배정 완료 플래그 설정
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();  // 난수 생성기
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n+1);  // n은 현재 인덱스 범위 내에서 무작위 인덱스를 선택
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    [PunRPC]
    void ReceivePreLife(string preLifeName)
    {
        Debug.Log("내 전생은: " + preLifeName);

        // 플레이어의 역할 처리 로직
        ExitGames.Client.Photon.Hashtable playerPropertiesHashtable = new ExitGames.Client.Photon.Hashtable
        {
            { $"PreLifeName", preLifeName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerPropertiesHashtable);

        sceneController.TransferDataAndLoadScene(preLifeName);
    }
}
