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
    private NetworkManager networkManager;
    private PlayerReady playerReady;
    private CanvasManager canvasManager;
    private Uploader uploader;

    
    
    
    public DebugCanvas debugCanvas;
    public AlwaysOnCanvas alwaysOnCanvas;
    public PreGameCanvas preGameCanvas;
    public QuestionCanvas questionCanvas;
    public SelectPlayerCanvas selectPlayerCanvas;
    public AuxiliaryCanvas auxiliaryCanvas;
    public MiniGame1 miniGame1;
    public MiniGame2 miniGame2;
    public ErrorCanvas errorCanvas;
    public RoomManager roomManager;
    public PlayerProperties playerProperties;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        playerReady = FindObjectOfType<PlayerReady>();
        canvasManager = FindObjectOfType<CanvasManager>();
        uploader = FindObjectOfType<Uploader>();
        errorCanvas = FindObjectOfType<ErrorCanvas>();
        roomManager = FindObjectOfType<RoomManager>();
        playerProperties = FindObjectOfType<PlayerProperties>();


        /*
        debugCanvas = FindObjectOfType<DebugCanvas>();
        alwaysOnCanvas = FindObjectOfType<AlwaysOnCanvas>();
        preGameCanvas = FindObjectOfType<PreGameCanvas>();
        questionCanvas = FindObjectOfType<QuestionCanvas>();
        selectPlayerCanvas = FindObjectOfType<SelectPlayerCanvas>();
        auxiliaryCanvas = FindObjectOfType<AuxiliaryCanvas>();
        miniGame1 = FindObjectOfType<MiniGame1>();
        miniGame2 = FindObjectOfType<MiniGame2>();
        */
    }

    public void OnPreGameCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();
        
        if (targetPanel == preGameCanvas.GetPanel("nameInput"))
        {
            preGameCanvas.SetName();

            preGameCanvas.SetActiveDisplay("nameInput", false);
            preGameCanvas.SetActiveDisplay("imageInput", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("imageInput"))
        {
            preGameCanvas.SetImage();

            preGameCanvas.SetActiveDisplay("imageInput", false);
            preGameCanvas.SetActiveDisplay("genderSelect", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("genderSelect"))
        {
            preGameCanvas.SetGender();
            networkManager.Connect();

            preGameCanvas.SetActiveDisplay("genderSelect", false);
            preGameCanvas.SetActiveDisplay("roomEntry", true);
            preGameCanvas.SetActiveDisplay("loading", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("roomEntry"))
        {
            if (buttonText.text == "�游���") preGameCanvas.SetActiveDisplay("maxSelect", true);
            else if (buttonText.text == "�����ϱ�") preGameCanvas.SetActiveDisplay("roomSelect", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("maxSelect"))
        {
            string txt = clickedButton.GetComponentInChildren<Text>().text;

            if(txt == "2��") GameManager.Instance.SetPlayerMaxNumber(2);
            else if (txt == "4��") GameManager.Instance.SetPlayerMaxNumber(4);
            else if (txt == "6��") GameManager.Instance.SetPlayerMaxNumber(6);

            preGameCanvas.SetActiveDisplay("maxSelect", false);
            roomManager.CreateRoom();
            preGameCanvas.SetActiveDisplay("loading", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("roomSelect"))
        {
            string roomCode = targetPanel.GetComponentInChildren<InputField>().text;
            string gender = playerProperties.GetGender();
            roomManager.TryJoinRoom(roomCode, gender);
            preGameCanvas.SetActiveDisplay("loading", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("loading"))
        {
            
        }
        else if(targetPanel == preGameCanvas.GetPanel("roomDisplay"))
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

    public void OnPreGameCanvasCancel()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");

        if (targetPanel == preGameCanvas.GetPanel("nameInput"))
        {
            //
        }
        else if (targetPanel == preGameCanvas.GetPanel("imageInput"))
        {
            preGameCanvas.SetActiveDisplay("imageInput", false);
            preGameCanvas.SetActiveDisplay("nameInput", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("genderSelect"))
        {
            preGameCanvas.SetActiveDisplay("genderSelect", false);
            preGameCanvas.SetActiveDisplay("imageInput", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("roomEntry"))
        {
            if(PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
            preGameCanvas.SetActiveDisplay("roomEntry", false);
            preGameCanvas.SetActiveDisplay("genderSelect", true);
        }
        else if(targetPanel == preGameCanvas.GetPanel("maxSelect"))
        {
            preGameCanvas.SetActiveDisplay("maxSelect", false);
            preGameCanvas.SetActiveDisplay("roomEntry", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("roomSelect"))
        {
            preGameCanvas.SetActiveDisplay("roomSelect", false);
            preGameCanvas.SetActiveDisplay("roomEntry", true);
        }
        else if (targetPanel == preGameCanvas.GetPanel("loading"))
        {
            ;
        }
        else if (targetPanel == preGameCanvas.GetPanel("roomDisplay"))
        {
            preGameCanvas.SetActiveDisplay("roomDisplay", false);
            preGameCanvas.SetActiveDisplay("roomEntry", true);
            PhotonNetwork.LeaveRoom(); // leaveRoom ���� �ʴ� ������ Room�� ������ Lobby�� ���� �Ǵµ� NetworkManager�� OnJoinedLobby�� ������ ���� ����.
        }
    }

    public void OnQuestionCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == questionCanvas.GetPanel("askQuestion1"))
        {
            questionCanvas.CheckAnswer(targetPanel);
            if (!questionCanvas.GetNextSign()) return;
            questionCanvas.SetActiveDisplay("askQuestion2", true);
        }
        else if(targetPanel == questionCanvas.GetPanel("askQuestion2"))
        {
            questionCanvas.CheckAnswer(targetPanel);
            if (!questionCanvas.GetNextSign()) return;
            questionCanvas.SetActiveDisplay("askQuestion3", true);
        }
        else if(targetPanel == questionCanvas.GetPanel("askQuestion3"))
        {
            questionCanvas.CheckAnswer(targetPanel);
            if (!questionCanvas.GetNextSign()) return;
            questionCanvas.SetActiveDisplay("askQuestion4", true);
        }
        else if(targetPanel == questionCanvas.GetPanel("askQuestion4"))
        {
            questionCanvas.CheckAnswer(targetPanel);
            if (!questionCanvas.GetNextSign()) return;
            questionCanvas.SubmitAnswer();
            
            canvasManager.TurnOffAndOn(canvasManager.QuestionCanvas, canvasManager.AlwaysOnCanvas); // temp
            canvasManager.TurnOffAndOn(null, canvasManager.SelectPlayerCanvas); // temp
        }
    }

    public void OnQuestionCanvasCancel()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == questionCanvas.GetPanel("askQuestion1"))
        {
            //
        }
        else if (targetPanel == questionCanvas.GetPanel("askQuestion2"))
        {
            questionCanvas.SetActiveDisplay("askQuestion2", false);
            questionCanvas.SetActiveDisplay("askQuestion1", true);
        }
        else if (targetPanel == questionCanvas.GetPanel("askQuestion3"))
        {
            questionCanvas.SetActiveDisplay("askQuestion3", false);
            questionCanvas.SetActiveDisplay("askQuestion2", true);
        }
        else if (targetPanel == questionCanvas.GetPanel("askQuestion4"))
        {
            questionCanvas.SetActiveDisplay("askQuestion4", false);
            questionCanvas.SetActiveDisplay("askQuestion3", true);
        }
    }

    public void OnSelectPlayerCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();
        

        if (targetPanel == selectPlayerCanvas.GetPanel("loveCardDisplay"))
        {
        }
        else if (targetPanel == selectPlayerCanvas.GetPanel("secretMessageDisplay"))
        {
            if (buttonText.text == "�ǳʶٱ�")
            {
                selectPlayerCanvas.SetActiveDisplay("secretMessageDisplay", false);
            }
        }
    }

    public void OnSelectPlayerCanvasSelecting()
    {
        GameObject clickedButton    = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel      = FindParentWithTag(clickedButton, "Panel");

        GameObject targetPlayer     = FindParentWithTag(clickedButton, "Player");
        Text actorNumberText        = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>();

        int targetActorNumber = int.Parse(actorNumberText.text);

        if (targetPanel == selectPlayerCanvas.GetPanel("loveCardDisplay"))
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
            selectPlayerCanvas.SetActiveDisplay("secretMessageDisplay", false);

            if (!GameManager.Instance.GetSignMG1())
            {
                canvasManager.TurnOffAndOn(canvasManager.SelectPlayerCanvas, canvasManager.MiniGame1);
                uploader.UploadReadyCount("MiniGame1");
            }                
            else if (!GameManager.Instance.GetSignMG2())
            {
                canvasManager.TurnOffAndOn(canvasManager.SelectPlayerCanvas, canvasManager.MiniGame2);
                uploader.UploadReadyCount("MiniGame2");
            }
            else
            {
                canvasManager.TurnOffAndOn(canvasManager.SelectPlayerCanvas, null);
                uploader.UploadReadyCount("GameEnd");
            }
                
            // else
        }


        else if (targetPanel == selectPlayerCanvas.GetPanel("secretMessageDisplay"))
        {
            GameObject messageScreen = selectPlayerCanvas.GetPanel("messageScreen");
            if (targetActorNumber != -1)
            {
                string targetName = targetPlayer.transform.Find("PlayerName").GetComponent<Text>().text;
                selectPlayerCanvas.SetActiveDisplay("messageScreen", true);
                messageScreen.transform.Find("TargetPlayerName").GetComponent<Text>().text = targetName;
                messageScreen.transform.Find("TargetPlayerActorNumber").GetComponent<Text>().text = targetActorNumber.ToString();
            }
        }
    }

    public void OnMessageScreen() // SelectPlayerCanvas�� SecretMessageDisplay�� ���� ���� �г�.
                                  // AuxiliaryCanvas�� ���ų� SelectPlayerCanvas�� �������� �ٲ�� �ұ�?
    {
        GameObject clickedButton    = EventSystem.current.currentSelectedGameObject;
        GameObject messageScreen    = selectPlayerCanvas.GetPanel("messageScreen");
        Text buttonText             = clickedButton.GetComponentInChildren<Text>();
        int targetActorNumber       = int.Parse(messageScreen.transform.Find("TargetPlayerActorNumber").GetComponent<Text>().text);
        string message              = messageScreen.transform.transform.Find("InputField").GetComponent<InputField>().text;

        if (buttonText.text == "����")
        {
            selectPlayerCanvas.SendSecretMessage(targetActorNumber, message);

            selectPlayerCanvas.SetActiveDisplay("messageScreen", false);
            selectPlayerCanvas.SetActiveDisplay("secretMessageDisplay", false);
        }
        else if (buttonText.text == "���")
        {
            selectPlayerCanvas.SetActiveDisplay("messageScreen", false);
        }
    }

    public void OnAlwaysOnCanvasConfirm()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;

        if (clickedButton == alwaysOnCanvas.GetPanel("messageBtn"))
        {
            alwaysOnCanvas.ToggleDisplay(alwaysOnCanvas.GetPanel("messageDisplay"), alwaysOnCanvas.GetPanel("loveCardDisplay"));
        }

        else if (clickedButton == alwaysOnCanvas.GetPanel("loveCardBtn"))
        {
            alwaysOnCanvas.ToggleDisplay(alwaysOnCanvas.GetPanel("loveCardDisplay"), alwaysOnCanvas.GetPanel("messageDisplay"));
        }
    }

    public void OnAuxiliaryCanvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == null)
        {
            Debug.LogError("Target panel not found!");
            return;
        }

        if (targetPanel == auxiliaryCanvas.GetPanel("miniGameSelectDisplay"))
        {
            GameObject targetPlayer = FindParentWithTag(clickedButton, "Player");
            Text actorNumberText = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>();

            // �׽�Ʈ�� Mock ������ ����
            int targetActorNumber = int.Parse(actorNumberText.text);

            if (targetActorNumber == -1)
            {
                // �÷��̾ ��������, �ش� �÷��̾��� ActorNumber�� Empty�� �����̹Ƿ�, Erroró��.
                PhotonNetwork.Disconnect(); // �̰� ���� ���� �ʿ���.
            }

            string currCanvas = canvasManager.GetCurrCanvas();
            if (currCanvas == "MiniGame1")
            {
                auxiliaryCanvas.SetActiveDisplay("miniGameSelectDisplay", false);

                if (auxiliaryCanvas.GetSelectDisplayConcept() == "���� ���� ���� ����� �����ϱ��?")
                {
                    Debug.Log("auxiliaryCanvas.GetSelectDisplayConcept() == \"���� ���� ���� ����� �����ϱ��?\" check in");
                    uploader.UploadSelectionMG1(targetActorNumber);
                    miniGame1.AfterSelection();
                    miniGame1.SetActiveDisplay("resultDisplay", true);
                }
                else
                {
                    Debug.Log("else check in");
                    uploader.UploadReceivedVotesMG1(targetActorNumber);
                    miniGame1.AfterSelection();
                    auxiliaryCanvas.SetSelectDisplayConcept("���� ���� ���� ����� �����ϱ��?");
                    miniGame1.SetActiveDisplay("baseDisplay", false);
                    miniGame1.SetActiveDisplay("waitDisplay", true);
                }
            }
            // MiniGame2�� �ʿ����.
            Debug.Log("OnAuxiliaryCanvas check out");
        }

        else if (targetPanel == auxiliaryCanvas.GetPanel("timer"))
        {
            auxiliaryCanvas.GetPanel("timer").transform.Find("ButtonSkip").gameObject.SetActive(false);
            string type = canvasManager.GetCurrCanvas() + "Timer";
            uploader.UploadReadyCount(type);
        }

        else if(targetPanel == auxiliaryCanvas.GetPanel("rewardEffect"))
        {
            if (buttonText.text == "Ȯ��")
            {
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", false);
                auxiliaryCanvas.InitRewardDisplay();
                if (GameManager.Instance.GetSignMG2()) // MiniGame1�� �����ٴ� ��ȣ�� true�̸�,
                {
                    canvasManager.TurnOffAndOn(canvasManager.MiniGame2, canvasManager.SelectPlayerCanvas);
                }
                else if (GameManager.Instance.GetSignMG1())
                {
                    canvasManager.TurnOffAndOn(canvasManager.MiniGame1, canvasManager.SelectPlayerCanvas);
                }
            }
        }

        else if(targetPanel == auxiliaryCanvas.GetPanel("winner"))
        {
            if(buttonText.text == "Ȯ��")
            {
                GameManager.Instance.EndGame();
                // ����, �غ�� �� �����ϰ� �ٽ� ���� �� ȭ������ �ʱ�ȭ �ؾ���.
            }
        }
    }

    public void OnMiniGame1Canvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == miniGame1.GetPanel("baseDisplay"))
        {
            if (buttonText.text == "�����ϱ�")
            {
                auxiliaryCanvas.SetActiveDisplay("miniGameSelectDisplay", true);
                miniGame1.SetActiveDisplay("baseDisplay", false);
            }
        }
        else if (targetPanel == miniGame1.GetPanel("resultDisplay"))
        {
            if (buttonText.text == "Ȯ��")
            {
                GameManager.Instance.SetSignMG1(true);
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", true);
                miniGame1.SetActiveDisplay("resultDisplay", false);
            }
        }
    }

    public void OnMiniGame2Canvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = FindParentWithTag(clickedButton, "Panel");
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == miniGame2.GetPanel("baseDisplay"))
        {
            if (clickedButton.name == "ButtonLeft")
            {
                miniGame2.AfterSelection(0);
            }
            else if(clickedButton.name == "ButtonRight")
            {
                miniGame2.AfterSelection(1);
            }
            miniGame2.SetActiveDisplay("baseDisplay", false);
            miniGame2.SetActiveDisplay("waitDisplay", true);
        }
        else if(targetPanel == miniGame2.GetPanel("resultDisplay"))
        {
            if (buttonText.text == "Ȯ��")
            {
                //�� ������ ���� �߰��ϱ�.
                GameManager.Instance.SetSignMG2(true);
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", true);
                miniGame2.SetActiveDisplay("resultDisplay", false);
            }
        }
    }

    private GameObject FindParentWithTag(GameObject child, string tag)
    {
        Transform current = child.transform;

        while (current != null)
        {
            if (current.CompareTag(tag))
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        return null; // ���ǿ� �´� �θ� ���� ���
    }

    public void OnDebugCanvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;

        Text targetText = targetPanel.transform.Find("DebugBtn/Text").gameObject.GetComponent<Text>();
        GameObject debugBox = targetPanel.transform.Find("DebugBox").gameObject;

        if(targetText != null && targetText.text == "D")
        {
            debugCanvas.ToggleDisplay(debugBox, null);
        }
    }

    public void OnErrorCanvas()
    {
        errorCanvas.OnErrorConfirmed();
    }
}
