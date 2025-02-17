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
    public GameObject PanelMaxSelect;   // MaxSelect
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
    public GameObject WaitDisplay1;
    public GameObject ResultDisplay1;
    [Space(10)]

    //In MiniGame2
    [Header("MiniGame2 Section")]
    public GameObject BaseDisplay2;
    public GameObject WaitDisplay2;
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

    //In DebugText
    [Header("DebugText Section")]
    public GameObject AboutDebugText;


    private PlayerProperties playerProperties;
    private LoadGallery loadGallery;
    private NetworkManager networkManager;
    private PlayerReady playerReady;
    private QuestionCanvas questionCanvas;
    private SelectPlayerCanvas selectPlayerCanvas;
    private CanvasManager canvasManager;
    private AuxiliaryCanvas auxiliaryCanvas;
    private Uploader uploader;

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
        uploader = FindObjectOfType<Uploader>();
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
            if (buttonText.text == "방만들기") PanelMaxSelect.SetActive(true); 
            else if (buttonText.text == "입장하기") PanelRoomSelect.SetActive(true);
        }
        else if (targetPanel == PanelMaxSelect)
        {
            string txt = clickedButton.GetComponentInChildren<Text>().text;

            if(txt == "2인") GameManager.Instance.SetPlayerMaxNumber(2);
            else if (txt == "4인") GameManager.Instance.SetPlayerMaxNumber(4);
            else if (txt == "6인") GameManager.Instance.SetPlayerMaxNumber(6);

            // GameManager.Instance.SetPlayerMaxNumber(1); test용

            PanelMaxSelect.SetActive(false);
            networkManager.Connect("방만들기");
            PanelLoading.SetActive(true);
        }
        else if (targetPanel == PanelRoomSelect)
        {
            string roomCode = targetPanel.GetComponentInChildren<InputField>().text;
            networkManager.Connect("입장하기"+roomCode);

            PanelLoading.SetActive(true);
        }
        else if (targetPanel == PanelLoading)
        {
            
        }
        else if(targetPanel == PanelRoomDisplay)
        {
            playerReady.OnButtonClick(buttonText.text);
            if (buttonText.text == "준비")
            {
                buttonText.text = "취소";
            }
            else if (buttonText.text == "취소")
            {
                buttonText.text = "준비";
            }
        }
    }

    public void SetInit()
    {
        PanelRoomDisplay.transform.Find("ReadyButton").GetComponentInChildren<Text>().text = "준비";
    }

    private void SetName()
    {
        String name = PanelNameInput.GetComponentInChildren<InputField>().text;
        playerProperties.SetName(name);
    }

    private void SetImage()
    {
        // 프로필 이미지로 설정된 이미지를 가져와서 firebase storage에 저장하고 URL을 가져와서 playerProperties에 저장함.
        Texture2D profileImage = PanelImageInput.transform.Find("ProfileImage").transform.Find("ImageSource").GetComponent<Image>().sprite.texture;

        profileImage = loadGallery.ConvertToReadableTexture(profileImage); // LoadGallery에서 가져와 기본 이미지도 설정 변경해줌.

        playerProperties.SetImage(profileImage);
    }

    private void SetGender()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (buttonText.text == "남성") playerProperties.GenderIsMale();
        else if (buttonText.text == "여성") playerProperties.GenderIsFemale();
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
        else if(targetPanel == PanelMaxSelect)
        {
            PanelMaxSelect.SetActive(false);
            PanelRoomEntry.SetActive(true);
        }
        else if (targetPanel == PanelRoomSelect)
        {
            PanelRoomSelect.SetActive(false);
            PanelRoomEntry.SetActive(true);
        }
        else if (targetPanel == PanelLoading)
        {
            ;
        }
        else if (targetPanel == PanelRoomDisplay)
        {
            PanelRoomDisplay.SetActive(false);
            PanelRoomEntry.SetActive(true);
            PhotonNetwork.Disconnect(); // leaveRoom 하지 않는 이유는 Room을 나가면 Lobby로 가게 되는데 NetworkManager의 OnJoinedLobby의 로직과 맞지 않음.
        }
    }

    public void OnRoomJoined()
    {
        PanelRoomEntry.SetActive(false);
        PanelRoomSelect.SetActive(false);
        PanelLoading.SetActive(false);
        PanelRoomDisplay.SetActive(true);
    }

    public void OnRoomFailed()
    {
        PanelLoading.SetActive(false);
        PhotonNetwork.Disconnect();
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
            questionCanvas.CheckAnswer(AskQuestion1);
            if (!questionCanvas.GetNextSign()) return;
            AskQuestion2.SetActive(true);
        }
        else if(targetPanel == AskQuestion2)
        {
            questionCanvas.CheckAnswer(AskQuestion2);
            if (!questionCanvas.GetNextSign()) return;
            AskQuestion3.SetActive(true);
        }
        else if(targetPanel == AskQuestion3)
        {
            questionCanvas.CheckAnswer(AskQuestion3);
            if (!questionCanvas.GetNextSign()) return;
            AskQuestion4.SetActive(true);
        }
        else if(targetPanel == AskQuestion4)
        {
            questionCanvas.CheckAnswer(AskQuestion4);
            if (!questionCanvas.GetNextSign()) return;
            questionCanvas.SubmitAnswer();
            
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
            if (buttonText.text == "건너뛰기")
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

        // 테스트용 Mock 데이터 설정
        int targetActorNumber = actorNumberText.text == "Empty" ? GetMockActorNumber() : int.Parse(actorNumberText.text);

        /*string targetString         = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>().text;
        int targetActorNumber       = targetString != "Empty" ? int.Parse(targetString) : -1;*/

        if (targetPanel == LoveCardDisplay)
        {
            if (targetActorNumber != -1)
            {
                selectPlayerCanvas.AddLoveCardScore(targetActorNumber);
            }
            else
            {
                // 플레이어를 눌렀지만, 해당 플레이어의 ActorNumber가 Empty인 버그이므로, Error처리.
                PhotonNetwork.Disconnect(); // 이거 말고도 따로 필요함.
            }
            LoveCardDisplay.SetActive(false);

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
                
            // else
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

    // Mock ActorNumber 생성 함수
    private int GetMockActorNumber()
    {
        // 테스트 시 사용할 임의의 ActorNumber 반환 (예: 9999)
        return 9999;
    }

    public void OnMessageScreen() // SelectPlayerCanvas의 SecretMessageDisplay에 속한 보조 패널.
                                  // AuxiliaryCanvas로 가거나 SelectPlayerCanvas의 직속으로 바꿔야 할까?
    {
        
        GameObject clickedButton    = EventSystem.current.currentSelectedGameObject;
        GameObject messageScreen    = clickedButton.transform.parent.gameObject;
        Text buttonText             = clickedButton.GetComponentInChildren<Text>();
        int targetActorNumber       = int.Parse(messageScreen.transform.Find("TargetPlayerActorNumber").GetComponent<Text>().text);
        Text message              = messageScreen.transform.Find("InputField/Text").GetComponent<Text>();

        if (buttonText.text == "전송")
        {
            selectPlayerCanvas.SendSecretMessage(targetActorNumber, message.text);

            messageScreen.SetActive(false);
            SecretMessageDisplay.SetActive(false);
        }
        else if (buttonText.text == "취소")
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
        // 현재 패널 활성화/비활성화
        if (displayToToggle != null)
        {
            displayToToggle.SetActive(!displayToToggle.activeSelf);
        }

        // 다른 패널이 활성화되어 있으면 비활성화
        if (otherDisplay != null && otherDisplay.activeSelf)
        {
            otherDisplay.SetActive(false);
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

        if (targetPanel == MiniGameSelectDisplay)
        {
            GameObject targetPlayer = clickedButton.transform.parent.parent.gameObject;
            Text actorNumberText = targetPlayer.transform.Find("PlayerActorNumber").GetComponent<Text>();

            // 테스트용 Mock 데이터 설정
            int targetActorNumber = actorNumberText.text == "Empty" ? GetMockActorNumber() : int.Parse(actorNumberText.text);

            if (targetActorNumber == -1)
            {
                // 플레이어를 눌렀지만, 해당 플레이어의 ActorNumber가 Empty인 버그이므로, Error처리.
                PhotonNetwork.Disconnect(); // 이거 말고도 따로 필요함.
            }


            string currCanvas = canvasManager.GetCurrCanvas();
            if (currCanvas == "MiniGame1")
            {
                auxiliaryCanvas.selectDisplay.SetActive(false);

                if (auxiliaryCanvas.GetSelectDisplayConcept() == "가장 많이 뽑힌 사람은 누구일까요?")
                {
                    Debug.Log("auxiliaryCanvas.GetSelectDisplayConcept() == \"가장 많이 뽑힌 사람은 누구일까요?\" check in");
                    uploader.UploadSelectionMG1(targetActorNumber);
                    canvasManager.MiniGame1.GetComponent<MiniGame1>().AfterSelection();
                    ResultDisplay1.SetActive(true);
                }
                else
                {
                    Debug.Log("else check in");
                    uploader.UploadReceivedVotesMG1(targetActorNumber);
                    canvasManager.MiniGame1.GetComponent<MiniGame1>().AfterSelection();
                    auxiliaryCanvas.SetSelectDisplayConcept("가장 많이 뽑힌 사람은 누구일까요?");
                    BaseDisplay1.SetActive(false);
                    WaitDisplay1.SetActive(true);
                }
            }
            // MiniGame2는 필요없음.
            Debug.Log("OnAuxiliaryCanvas check out");
        }

        else if (targetPanel == Timer)
        {
            Timer.transform.Find("ButtonSkip").gameObject.SetActive(false);
            string type = canvasManager.GetCurrCanvas() + "Timer";
            uploader.UploadReadyCount(type);
        }

        else if(targetPanel == RewardEffect)
        {
            if (buttonText.text == "확인")
            {
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", false);
                auxiliaryCanvas.InitRewardDisplay();
                if (GameManager.Instance.GetSignMG2()) // MiniGame1이 끝났다는 신호가 true이면,
                {
                    canvasManager.TurnOffAndOn(canvasManager.MiniGame2, canvasManager.SelectPlayerCanvas);
                }
                else if (GameManager.Instance.GetSignMG1())
                {
                    canvasManager.TurnOffAndOn(canvasManager.MiniGame1, canvasManager.SelectPlayerCanvas);
                }
            }
        }
    }

    public void OnMiniGame1Canvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == BaseDisplay1)
        {
            if (buttonText.text == "선택하기")
            {
                MiniGameSelectDisplay.SetActive(true);
                BaseDisplay1.SetActive(false);
            }
        }
        else if (targetPanel == ResultDisplay1)
        {
            if (buttonText.text == "확인")
            {
                //이 이전에 보상 추가하기.
                //canvasManager.TurnOffAndOn(canvasManager.MiniGame1, canvasManager.MiniGame2);
                GameManager.Instance.SetSignMG1(true);
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", true);
                ResultDisplay1.SetActive(false);
            }
        }
    }

    public void OnMiniGame2Canvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;
        Text buttonText = clickedButton.GetComponentInChildren<Text>();

        if (targetPanel == BaseDisplay2)
        {
            MiniGame2 miniGame2 = canvasManager.MiniGame2.GetComponent<MiniGame2>();
            if (clickedButton.name == "ButtonLeft")
            {
                miniGame2.AfterSelection(0);
            }
            else if(clickedButton.name == "ButtonRight")
            {
                miniGame2.AfterSelection(1);
            }
            BaseDisplay2.SetActive(false);
            WaitDisplay2.SetActive(true);
        }
        else if(targetPanel == ResultDisplay2)
        {
            if (buttonText.text == "확인")
            {
                //이 이전에 보상 추가하기.
                GameManager.Instance.SetSignMG2(true);
                auxiliaryCanvas.SetActiveDisplay("rewardEffect", true);
                ResultDisplay2.SetActive(false);
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

        return null; // 조건에 맞는 부모가 없을 경우
    }


    public void OnDebugCanvas()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        GameObject targetPanel = clickedButton.transform.parent.gameObject;

        Text targetText = targetPanel.transform.Find("DebugBtn/Text").gameObject.GetComponent<Text>();
        GameObject debugBox = targetPanel.transform.Find("DebugBox").gameObject;

        if(targetText != null && targetText.text == "D")
        {
            ToggleDisplay(debugBox, null);
        }
    }
}
