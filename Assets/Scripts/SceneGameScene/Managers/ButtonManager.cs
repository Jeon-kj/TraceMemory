using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Photon.Pun;

public class ButtonManager : MonoBehaviour
{
    //In PreGameCanvas
    [Header("PreGameCanvas Section")]
    public GameObject PanelNameInput;   // NameInput
    public GameObject PanelImageInput;  // ImageInput
    public GameObject PanelGenderSelect;   // GenderSelect
    public GameObject PanelRoomEntry;  // RoomEntry
    public GameObject PanelRoomSelect;   // RoomSelect
    public GameObject PanelLoading;   // Loading
    public GameObject PanelRoomDisplay; // RoomDisplay
    public GameObject PanelReadyToStart; // ReadyToStart
    [Space(10)]

    //In QuestionCanvas
    [Header("QuestionCanvas Section")]
    public GameObject AskQuestion1;
    public GameObject AskQuestion2;
    public GameObject AskQuestion3;
    public GameObject AskQuestion4;
    [Space(10)]

    //In SelectPlayerCanvas
    [Header("SelectPlayerCanvas Section")]
    public GameObject FirstImpressionDisplay;
    public GameObject LoveCardDisplay;
    public GameObject SecretMessageDisplay;
    [Space(10)]

    //In AuxiliaryCanvas
    [Header("AuxiliaryCanvas Section")]
    public GameObject MiniGameSelectDisplay;
    public GameObject IntroduceMyself;
    public GameObject Timer;
    public GameObject RewardEffect;
    [Space(10)]

    //In MiniGame1
    [Header("MiniGame1 Section")]
    public GameObject BaseDisplay1;
    public GameObject ResultDisplay1;
    [Space(10)]

    //In MiniGame2
    [Header("MiniGame2 Section")]
    public GameObject BaseDisplay2;
    public GameObject ResultDisplay2;
    [Space(10)]

    //In MiniGame3
    [Header("MiniGame3 Section")]
    public GameObject BaseDisplay3;
    public GameObject ResultDisplay3;

    //In AlwaysOnCanvas
    [Header("AlwaysOnCanvas Section")]
    public GameObject AboutSecretMessage;
    public GameObject AboutLoveCard;


    private PlayerProperties playerProperties;
    private LoadGallery loadGallery;
    private NetworkManager networkManager;
    private PlayerReady playerReady;
    private QuestionCanvas questionCanvas;
    private SelectPlayerCanvas selectPlayerCanvas;
    private CanvasManager canvasManager;
    private AuxiliaryCanvas auxiliaryCanvas;

    private void Awake()
    {
        loadGallery = GetComponent<LoadGallery>();
        playerProperties = FindObjectOfType<PlayerProperties>();
        networkManager = FindObjectOfType<NetworkManager>();
        playerReady = FindObjectOfType<PlayerReady>();
        questionCanvas = FindObjectOfType<QuestionCanvas>();
        selectPlayerCanvas = FindObjectOfType<SelectPlayerCanvas>();
        canvasManager = FindObjectOfType<CanvasManager>();
        auxiliaryCanvas = FindObjectOfType<AuxiliaryCanvas>();
    }

    public void OnPreGameCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();
        
        if (targetPanel == PanelNameInput)
        {
            SetName();

            PanelNameInput.SetActive(false);
            PanelImageInput.SetActive(true);
        }
        else if (targetPanel == PanelImageInput)
        {
            SetImage();

            PanelImageInput.SetActive(false);
            PanelGenderSelect.SetActive(true);
        }
        else if (targetPanel == PanelGenderSelect)
        {
            SetGender();

            PanelGenderSelect.SetActive(false);
            PanelRoomEntry.SetActive(true);
        }
        else if (targetPanel == PanelRoomEntry)
        {
            if (buttonText.text == "�游���")
            {
                networkManager.Connect(buttonText.text);
                PanelLoading.SetActive(true);
            }
            else if (buttonText.text == "�����ϱ�") PanelRoomSelect.SetActive(true);
        }
        else if (targetPanel == PanelRoomSelect)
        {
            string roomCode = targetPanel.GetComponentInChildren<InputField>().text;
            networkManager.Connect("�����ϱ�"+roomCode);

            PanelLoading.SetActive(true);
        }
        else if (targetPanel == PanelLoading)
        {
            
        }
        else if(targetPanel == PanelRoomDisplay)
        {
            playerReady.OnButtonClick(buttonText.text);
            if (buttonText.text == "�غ�")
            {
                buttonText.text = "���";
            }
            else if (buttonText.text == "���")
            {
                buttonText.text = "�غ�";
            }
        }
    }

    public void SetInit()
    {
        PanelRoomDisplay.transform.Find("ReadyButton").GetComponentInChildren<Text>().text = "�غ�";
    }

    private void SetName()
    {
        String name = PanelNameInput.GetComponentInChildren<InputField>().text;
        playerProperties.SetName(name);
    }

    private void SetImage()
    {
        // ������ �̹����� ������ �̹����� �����ͼ� firebase storage�� �����ϰ� URL�� �����ͼ� playerProperties�� ������.
        Texture2D profileImage = PanelImageInput.transform.Find("ProfileImage").transform.Find("ImageSource").GetComponent<Image>().sprite.texture;

        profileImage = loadGallery.ConvertToReadableTexture(profileImage); // LoadGallery���� ������ �⺻ �̹����� ���� ��������.

        playerProperties.SetImage(profileImage);
    }

    private void SetGender()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (buttonText.text == "����") playerProperties.GenderIsMale();
        else if (buttonText.text == "����") playerProperties.GenderIsFemale();
    }

    public void OnPreGameCanvasCancel()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;

        if (targetPanel == PanelNameInput)
        {
            //
        }
        else if (targetPanel == PanelImageInput)
        {
            PanelImageInput.SetActive(false);
            PanelNameInput.SetActive(true);
        }
        else if (targetPanel == PanelGenderSelect)
        {
            PanelGenderSelect.SetActive(false);
            PanelImageInput.SetActive(true);
        }
        else if (targetPanel == PanelRoomEntry)
        {
            PanelRoomEntry.SetActive(false);
            PanelGenderSelect.SetActive(true);
        }
        else if (targetPanel == PanelRoomSelect)
        {
            PanelRoomSelect.SetActive(false);
            PanelRoomEntry.SetActive(true);
        }
        else if (targetPanel == PanelLoading)
        {
            PanelLoading.SetActive(false);
            PanelRoomEntry.SetActive(true);
            PhotonNetwork.Disconnect();
        }
        else if (targetPanel == PanelRoomDisplay)
        {
            PanelRoomDisplay.SetActive(false);
            PanelRoomEntry.SetActive(true);
            PhotonNetwork.Disconnect(); // leaveRoom ���� �ʴ� ������ Room�� ������ Lobby�� ���� �Ǵµ� NetworkManager�� OnJoinedLobby�� ������ ���� ����.
        }
    }

    public void OnRoomJoined()
    {
        PanelRoomEntry.SetActive(false);
        PanelRoomSelect.SetActive(false);
        PanelLoading.SetActive(false);
        PanelRoomDisplay.SetActive(true);
    }

    public void ReadyToStartGame()
    {
        PanelRoomDisplay.SetActive(false);
        PanelReadyToStart.SetActive(true);
    }

    public void OnQuestionCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == AskQuestion1)
        {
            AskQuestion1.SetActive(false);
            AskQuestion2.SetActive(true);
        }
        else if(targetPanel == AskQuestion2)
        {
            AskQuestion2.SetActive(false);
            AskQuestion3.SetActive(true);
        }
        else if(targetPanel == AskQuestion3)
        {
            AskQuestion3.SetActive(false);
            AskQuestion4.SetActive(true);
        }
        else if(targetPanel == AskQuestion4)
        {
            AskQuestion4.SetActive(false);
            questionCanvas.SubmitAnwer();
            
            canvasManager.TurnOffAndOn(canvasManager.QuestionCanvas, canvasManager.AlwaysOnCanvas); // temp
            canvasManager.TurnOffAndOn(null, canvasManager.SelectPlayerCanvas); // temp
        }
    }

    public void OnQuestionCanvasCancel()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == AskQuestion1)
        {
            //
        }
        else if (targetPanel == AskQuestion2)
        {
            AskQuestion2.SetActive(false);
            AskQuestion1.SetActive(true);
        }
        else if (targetPanel == AskQuestion3)
        {
            AskQuestion3.SetActive(false);
            AskQuestion2.SetActive(true);
        }
        else if (targetPanel == AskQuestion4)
        {
            AskQuestion4.SetActive(false);
            AskQuestion3.SetActive(true);
        }
    }

    public void OnSelectPlayerCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();
        

        if (targetPanel == FirstImpressionDisplay)
        {
        }
        else if (targetPanel == LoveCardDisplay)
        {
        }
        else if (targetPanel == SecretMessageDisplay)
        {
            if (buttonText.text == "�ǳʶٱ�")
            {
                SecretMessageDisplay.SetActive(false);
            }
        }
    }

    public void OnSelectPlayerCanvasSelecting()
    {
        GameObject clickedButton    = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel      = clickedButton.transform.parent.parent.parent.gameObject;

        GameObject targetPlayer     = clickedButton.transform.parent.parent.gameObject;
        Text actorNumberText        = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>();

        // �׽�Ʈ�� Mock ������ ����
        int targetActorNumber = actorNumberText.text == "Empty" ? GetMockActorNumber() : int.Parse(actorNumberText.text);

        /*string targetString         = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>().text;
        int targetActorNumber       = targetString != "Empty" ? int.Parse(targetString) : -1;*/


        /*if (targetPanel == FirstImpressionDisplay)
        {
            if (targetActorNumber != -1)
            {
                selectPlayerCanvas.AddFirstImpressionScore(targetActorNumber);
            }
            else
            {
                // �÷��̾ ��������, �ش� �÷��̾��� ActorNumber�� Empty�� �����̹Ƿ�, Erroró��.
                PhotonNetwork.Disconnect(); // �̰� ���� ���� �ʿ���.
            }
            FirstImpressionDisplay.SetActive(false);
        }*/


        if (targetPanel == LoveCardDisplay)
        {
            if (targetActorNumber != -1)
            {
                selectPlayerCanvas.AddLoveCardScore(targetActorNumber);
            }
            else
            {
                // �÷��̾ ��������, �ش� �÷��̾��� ActorNumber�� Empty�� �����̹Ƿ�, Erroró��.
                PhotonNetwork.Disconnect(); // �̰� ���� ���� �ʿ���.
            }
            LoveCardDisplay.SetActive(false);
            canvasManager.TurnOffAndOn(canvasManager.SelectPlayerCanvas, canvasManager.MiniGame1);
        }


        else if (targetPanel == SecretMessageDisplay)
        {
            GameObject messageScreen = SecretMessageDisplay.transform.Find("MessageScreen").gameObject;
            if (targetActorNumber != -1)
            {
                string targetName = targetPlayer.transform.Find("PlayerName").GetComponent<Text>().text;
                messageScreen.SetActive(true);
                messageScreen.transform.Find("TargetPlayerName").GetComponent<Text>().text = targetName;
                messageScreen.transform.Find("TargetPlayerActorNumber").GetComponent<Text>().text = targetActorNumber.ToString();
            }
        }
    }

    // Mock ActorNumber ���� �Լ�
    private int GetMockActorNumber()
    {
        // �׽�Ʈ �� ����� ������ ActorNumber ��ȯ (��: 9999)
        return 9999;
    }

    public void OnMessageScreen() // SelectPlayerCanvas�� SecretMessageDisplay�� ���� ���� �г�.
                                  // AuxiliaryCanvas�� ���ų� SelectPlayerCanvas�� �������� �ٲ�� �ұ�?
    {
        
        GameObject clickedButton    = EventSystem.current.currentSelectedGameObject;
        GameObject messageScreen    = clickedButton.transform.parent.gameObject;
        Text buttonText             = clickedButton.GetComponentInChildren<Text>();
        int targetActorNumber       = int.Parse(messageScreen.transform.Find("TargetPlayerActorNumber").GetComponent<Text>().text);
        string message              = messageScreen.transform.Find("InputField/Text").GetComponent<Text>().text;

        if (buttonText.text == "����")
        {
            selectPlayerCanvas.SendSecretMessage(targetActorNumber, message);
            messageScreen.SetActive(false);
        }
        else if (buttonText.text == "���")
        {
            messageScreen.SetActive(false);
        }
    }

    public void OnAlwaysOnCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        GameObject messageBox = targetPanel.transform.parent.Find("AboutSecretMessage/MessageBox").gameObject;
        GameObject loveCardDisplay = targetPanel.transform.parent.Find("AboutLoveCard/LoveCardDisplay").gameObject;

        if (targetPanel == AboutSecretMessage && buttonText.text == "M")
        {
            ToggleDisplay(messageBox, loveCardDisplay);
        }

        else if (targetPanel == AboutLoveCard && buttonText.text == "L")
        {
            ToggleDisplay(loveCardDisplay, messageBox);
        }
    }

    private void ToggleDisplay(GameObject displayToToggle, GameObject otherDisplay)
    {
        // ���� �г� Ȱ��ȭ/��Ȱ��ȭ
        if (displayToToggle != null)
        {
            displayToToggle.SetActive(!displayToToggle.activeSelf);
        }

        // �ٸ� �г��� Ȱ��ȭ�Ǿ� ������ ��Ȱ��ȭ
        if (otherDisplay != null && otherDisplay.activeSelf)
        {
            otherDisplay.SetActive(false);
        }
    }

    public void OnAuxiliaryCanvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.parent.parent.gameObject;

        GameObject targetPlayer = clickedButton.transform.parent.parent.gameObject;
        Text actorNumberText = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>();

        // �׽�Ʈ�� Mock ������ ����
        int targetActorNumber = actorNumberText.text == "Empty" ? GetMockActorNumber() : int.Parse(actorNumberText.text);

        Debug.Log("OnAuxiliaryCanvas check in1" + " : " + targetPanel);

        if (targetPanel == MiniGameSelectDisplay)
        {
            Debug.Log("OnAuxiliaryCanvas check in2");
            if (targetActorNumber != -1)
            {
                Debug.Log("OnAuxiliaryCanvas check in3");
                auxiliaryCanvas.AddScore(targetActorNumber);
                canvasManager.MiniGame1.GetComponent<MiniGame1>().CompleteSelecting();
            }
            else
            {
                // �÷��̾ ��������, �ش� �÷��̾��� ActorNumber�� Empty�� �����̹Ƿ�, Erroró��.
                PhotonNetwork.Disconnect(); // �̰� ���� ���� �ʿ���.
            }
            MiniGameSelectDisplay.SetActive(false);
            BaseDisplay1.SetActive(true);
            Debug.Log("OnAuxiliaryCanvas check out");
        }
    }

    public void OnMiniGame1Canvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == BaseDisplay1)
        {
            if(buttonText.text == "�����ϱ�")
            {
                MiniGameSelectDisplay.SetActive(true);
                BaseDisplay1.SetActive(false);
            }
        }
    }
}
