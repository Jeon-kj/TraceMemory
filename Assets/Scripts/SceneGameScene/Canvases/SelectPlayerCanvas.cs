using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPlayerCanvas : MonoBehaviour
{
    Uploader uploader;

    public GameObject roomDisplay;
    public GameObject firstImpressionDisplay;
    public GameObject loveCardDisplay;
    public GameObject secretMessageDisplay;

    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            string playerGender = PhotonNetwork.LocalPlayer.GetPlayerGender();

            UpdatePartnerDisplay(firstImpressionDisplay.transform, playerGender);
            UpdatePartnerDisplay(loveCardDisplay.transform, playerGender);
            UpdatePartnerDisplay(secretMessageDisplay.transform, playerGender);
        }
    }

    private void UpdatePartnerDisplay(Transform display, string gender)
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers / 2;

        for (int i = 0; i < maxPlayers; i++)
        {
            Transform partnerTransform = display.Find($"Partner{i + 1}");
            Transform sourceTransform;  // 복사원본 역할을 하는 오브젝트

            if (gender == "Male") sourceTransform = roomDisplay.transform.Find($"Female{i + 1}");
            else sourceTransform = roomDisplay.transform.Find($"Male{i + 1}");

            // PlayerName 복사
            Text partnerNameText = partnerTransform.Find("PlayerName").GetComponent<Text>();
            Text sourceNameText = sourceTransform.Find("PlayerName").GetComponent<Text>();
            partnerNameText.text = sourceNameText.text;

            // PlayerActorNumber 복사
            Text partnerActorNumberText = partnerTransform.Find("PlayerActorNumber").GetComponent<Text>();
            Text sourceActorNumberText = sourceTransform.Find("PlayerActorNumber").GetComponent<Text>();
            partnerActorNumberText.text = sourceActorNumberText.text;

            // ImageSource 복사
            Image partnerImage = partnerTransform.Find("Mask/ImageSource").GetComponent<Image>();
            Image sourceImage = sourceTransform.Find("Mask/ImageSource").GetComponent<Image>();
            partnerImage.sprite = sourceImage.sprite;

            if (partnerNameText.text == "Empty") partnerNameText.gameObject.SetActive(false);
        }
    }



    public void AddFirstImpressionScore(int targetActorNumber)
    {
        uploader.UploadScore("FirstImpressionScore", targetActorNumber);
    }

    public void AddLoveCardScore(int targetActorNumber)
    {
        uploader.UploadScore("LoveCardScore", targetActorNumber);
    }

    public void SendSecretMessage(int targetActorNumber, string message)
    {
        uploader.UploadSecretMessage(targetActorNumber, message);
    }
}
