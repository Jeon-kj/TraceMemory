using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreGameCanvas : MonoBehaviour
{
    [Header("Family Section")]
    public GameObject nameInput;   // NameInput
    public GameObject imageInput;  // ImageInput
    public GameObject genderSelect;   // GenderSelect
    public GameObject roomEntry;  // RoomEntry
    public GameObject maxSelect;   // MaxSelect
    public GameObject roomSelect;   // RoomSelect
    public GameObject loading;   // Loading
    public GameObject roomDisplay; // RoomDisplay
    public GameObject readyToStart; // ReadyToStart
    [Space(10)]

    private PlayerProperties playerProperties;
    private LoadGallery loadGallery;

    private void Awake()
    {
        playerProperties = FindObjectOfType<PlayerProperties>();
        loadGallery = GetComponent<LoadGallery>();
    }

    public void SetInit()
    {
        nameInput.transform.Find("InputField").GetComponent<InputField>().text = "";
        roomSelect.transform.Find("InputField").GetComponent<InputField>().text = "";
    }

    public void ReadyButtonInit()
    {
        roomDisplay.transform.Find("ReadyButton").GetComponentInChildren<Text>().text = "�غ�";
    }

    public void SetName()
    {
        String name = nameInput.GetComponentInChildren<InputField>().text;
        playerProperties.SetName(name);
    }

    public void SetImage()
    {
        // ������ �̹����� ������ �̹����� �����ͼ� firebase storage�� �����ϰ� URL�� �����ͼ� playerProperties�� ������.
        Texture2D profileImage = imageInput.transform.Find("ProfileImage").transform.Find("ImageSource").GetComponent<Image>().sprite.texture;

        profileImage = loadGallery.ConvertToReadableTexture(profileImage); // LoadGallery���� ������ �⺻ �̹����� ���� ��������.

        playerProperties.SetImage(profileImage);
    }

    public void SetGender()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (buttonText.text == "����") playerProperties.GenderIsMale();
        else if (buttonText.text == "����") playerProperties.GenderIsFemale();        
    }

    public void OnRoomJoined()
    {
        SetActiveDisplay("roomEntry", false);
        SetActiveDisplay("roomSelect", false);
        SetActiveDisplay("loading", false);
        SetActiveDisplay("roomDisplay", true);
    }

    public void OnRoomFailed()
    {
        SetActiveDisplay("loading", false);
        PhotonNetwork.Disconnect();
    }

    public void ReadyToStartGame()
    {
        SetActiveDisplay("roomDisplay", false);
        SetActiveDisplay("readyToStart", true);
    }

    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "nameInput":
                nameInput.SetActive(sign);
                break;
            case "imageInput":
                imageInput.SetActive(sign);
                break;
            case "genderSelect":
                genderSelect.SetActive(sign);
                break;
            case "roomEntry":
                roomEntry.SetActive(sign);
                break;
            case "maxSelect":
                maxSelect.SetActive(sign);
                break;
            case "roomSelect":
                roomSelect.SetActive(sign);
                break;
            case "loading":
                loading.SetActive(sign);
                break;
            case "roomDisplay":
                roomDisplay.SetActive(sign);
                break;
            case "readyToStart":
                readyToStart.SetActive(sign);
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
            case "nameInput":
                gameObject = nameInput;
                break;
            case "imageInput":
                gameObject = imageInput;
                break;
            case "genderSelect":
                gameObject = genderSelect;
                break;
            case "roomEntry":
                gameObject = roomEntry;
                break;
            case "maxSelect":
                gameObject = maxSelect;
                break;
            case "roomSelect":
                gameObject = roomSelect;
                break;
            case "loading":
                gameObject = loading;
                break;
            case "roomDisplay":
                gameObject = roomDisplay;
                break;
            case "readyToStart":
                gameObject = readyToStart;
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }
}
