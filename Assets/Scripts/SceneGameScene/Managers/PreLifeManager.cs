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
    Dictionary<string, string> coupleDic = new Dictionary<string, string> 
    {   
        {"몽룡", "춘향"},
        {"견우", "직녀"},
        {"온달", "평강"},
        {"춘향", "몽룡"},
        {"직녀", "견우"},
        {"평강", "온달"}
    };

    SceneController sceneController;
    Uploader uploader;

    private void Awake()
    {
        sceneController = FindObjectOfType<SceneController>();
        uploader = FindObjectOfType<Uploader>();
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
        Dictionary<string, Player> roleDic = new Dictionary<string, Player>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            List<string> list = player.GetPlayerGender() == "Male" ? males : females;
            string preLifeName = list[0];

            roleDic.Add(preLifeName, player);

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

        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var role in roleDic.Keys)
            {
                Player player = roleDic[role];
                string partnerRole = coupleDic[role];
                Player partner = roleDic[partnerRole];
                uploader.PartnerActorNumber(player.ActorNumber, partner.ActorNumber);
            }
        }        

        preLifesAssigned = true;  // 역할 배정 완료 플래그 설정
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();  // 난수 생성기
        //int n = list.Count;
        int n = GameManager.Instance.GetPlayerMaxNumber()/2;
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
