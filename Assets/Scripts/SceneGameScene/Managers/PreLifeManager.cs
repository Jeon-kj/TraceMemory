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
    List<string> males = new List<string> { "����", "�߿�", "�´�" };
    List<string> females = new List<string> { "����", "����", "��" };

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
                photonView.RPC("ReceivePreLife", player, preLifeName);  // �� �÷��̾�� ���� ����
            }
            catch(Exception e)
            {
                Debug.Log("Error Detect : " + e);
            }
            
        }

        preLifesAssigned = true;  // ���� ���� �Ϸ� �÷��� ����
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();  // ���� ������
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n+1);  // n�� ���� �ε��� ���� ������ ������ �ε����� ����
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    [PunRPC]
    void ReceivePreLife(string preLifeName)
    {
        Debug.Log("�� ������: " + preLifeName);

        // �÷��̾��� ���� ó�� ����
        ExitGames.Client.Photon.Hashtable playerPropertiesHashtable = new ExitGames.Client.Photon.Hashtable
        {
            { $"PreLifeName", preLifeName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerPropertiesHashtable);

        sceneController.TransferDataAndLoadScene(preLifeName);
    }
}
