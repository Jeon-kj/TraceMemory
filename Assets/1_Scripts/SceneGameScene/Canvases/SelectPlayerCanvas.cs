using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPlayerCanvas : MonoBehaviour
{
    Uploader uploader;

    [Header("Family Section")]
    public GameObject loveCardDisplay;
    public GameObject secretMessageDisplay;
    public GameObject messageScreen;
    [Space(10)]

    [Header("Other Section")]
    public GameObject roomDisplay;

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
        SetActiveDisplay("loveCardDisplay", true);
        SetActiveDisplay("secretMessageDisplay", true);

        InputField inputField = secretMessageDisplay.transform.Find("MessageScreen/InputField").GetComponent<InputField>();
        inputField.text = "";
    }

    private void UpdatePartnerDisplay(Transform display, string gender)
    {
        int maxPlayers = 6 / 2;

        for (int i = 0; i < maxPlayers; i++)
        {
            Transform partnerTransform = display.Find($"Partner{i + 1}");
            Transform sourceTransform;  // 복사원본 역할을 하는 오브젝트

            if (gender == "Male") sourceTransform = roomDisplay.transform.Find($"Female{i + 1}");
            else sourceTransform = roomDisplay.transform.Find($"Male{i + 1}");

            // PlayerName 복사
            Text partnerNameText = partnerTransform.Find("PlayerName").GetComponent<Text>();
            Text sourceNameText = sourceTransform.Find("PlayerName").GetComponent<Text>();

            if (sourceNameText.text == "Empty")
            {
                partnerTransform.gameObject.SetActive(false);
                continue;
            }
            partnerNameText.text = sourceNameText.text;

            // PlayerActorNumber 복사
            Text partnerActorNumberText = partnerTransform.Find("PlayerActorNumber").GetComponent<Text>();
            Text sourceActorNumberText = sourceTransform.Find("PlayerActorNumber").GetComponent<Text>();
            partnerActorNumberText.text = sourceActorNumberText.text;

            // ImageSource 복사
            Image partnerImage = partnerTransform.Find("Mask/ImageSource").GetComponent<Image>();
            Image sourceImage = sourceTransform.Find("Mask/ImageSource").GetComponent<Image>();
            partnerImage.sprite = sourceImage.sprite;
        }
    }

    public void AddLoveCardScore(int targetActorNumber)
    {
        Text concept = loveCardDisplay.transform.Find("Concept").GetComponent<Text>();
        if (concept.text == "첫인상이 마음에 드는 사람에게 투표해주세요.") concept.text = "호감카드를 주고 싶은 플레이어를 선택하세요!";
        uploader.UploadScore("LoveCardScore", targetActorNumber);
    }

    public void SendSecretMessage(int targetActorNumber, string message)
    {
        uploader.UploadSecretMessage(targetActorNumber, message);
    }

    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "loveCardDisplay":
                loveCardDisplay.SetActive(sign);
                break;
            case "secretMessageDisplay":
                secretMessageDisplay.SetActive(sign);
                break;
            case "messageScreen":
                messageScreen.SetActive(sign);
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }
    }

    public GameObject GetPanel(string target)
    {
        GameObject gameObject = null;
        switch (target)
        {
            case "loveCardDisplay":
                gameObject = loveCardDisplay;
                break;
            case "secretMessageDisplay":
                gameObject = secretMessageDisplay;
                break;
            case "messageScreen":
                gameObject = messageScreen;
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }
}
