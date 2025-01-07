using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPlayerCanvas : MonoBehaviour
{
    Uploader uploader;

    public GameObject roomDisplay;
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

            UpdatePartnerDisplay(loveCardDisplay.transform, playerGender);
            UpdatePartnerDisplay(secretMessageDisplay.transform, playerGender);
        }
    }

    private void OnEnable()
    {
        InitializedCanvas();
    }

    private void InitializedCanvas()
    {
        loveCardDisplay.SetActive(true); 
        secretMessageDisplay.SetActive(true);
        
        InputField inputField = secretMessageDisplay.transform.Find("MessageScreen/InputField").GetComponent<InputField>();
        inputField.text = "";
    }

    private void UpdatePartnerDisplay(Transform display, string gender)
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers / 2;

        for (int i = 0; i < maxPlayers; i++)
        {
            Transform partnerTransform = display.Find($"Partner{i + 1}");
            Transform sourceTransform;  // ������� ������ �ϴ� ������Ʈ

            if (gender == "Male") sourceTransform = roomDisplay.transform.Find($"Female{i + 1}");
            else sourceTransform = roomDisplay.transform.Find($"Male{i + 1}");

            // PlayerName ����
            Text partnerNameText = partnerTransform.Find("PlayerName").GetComponent<Text>();
            Text sourceNameText = sourceTransform.Find("PlayerName").GetComponent<Text>();

            if (sourceNameText.text == "Empty")
            {
                partnerTransform.gameObject.SetActive(false);
                continue;
            }
            partnerNameText.text = sourceNameText.text;

            // PlayerActorNumber ����
            Text partnerActorNumberText = partnerTransform.Find("PlayerActorNumber").GetComponent<Text>();
            Text sourceActorNumberText = sourceTransform.Find("PlayerActorNumber").GetComponent<Text>();
            partnerActorNumberText.text = sourceActorNumberText.text;

            // ImageSource ����
            Image partnerImage = partnerTransform.Find("Mask/ImageSource").GetComponent<Image>();
            Image sourceImage = sourceTransform.Find("Mask/ImageSource").GetComponent<Image>();
            partnerImage.sprite = sourceImage.sprite;
        }
    }

    public void AddLoveCardScore(int targetActorNumber)
    {
        Text concept = loveCardDisplay.transform.Find("Concept").GetComponent<Text>();
        if (concept.text == "ù�λ��� ������ ��� ������� ��ǥ���ּ���.") concept.text = "ȣ��ī�带 �ְ� ���� �÷��̾ �����ϼ���!";
        uploader.UploadScore("LoveCardScore", targetActorNumber);
    }

    public void SendSecretMessage(int targetActorNumber, string message)
    {
        uploader.UploadSecretMessage(targetActorNumber, message);
    }
}
