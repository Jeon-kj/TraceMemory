using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MiniGame2 : MonoBehaviourPunCallbacks
{
    public GameObject baseDisplay;
    public GameObject waitDisplay;
    public GameObject resultDisplay;

    //private string sign = "";

    private string[] questionList = new string[]{
        "나는 장기 연애를 해본 적이 있다. (2년 이상)",
        "둘 중 하나를 평생 못 먹는다면?"
    };

    private string[,] answerList = new string[,] {
        { "O", "X" },
        { "라면", "치킨" }
    };

    private string PickedQuestion;
    private string[] PickedAnswer;
    private System.Random random = new System.Random();  // Random 객체 생성

    CanvasManager canvasManager;
    Uploader uploader;
    Loader loader;

    private void Awake()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        uploader = FindObjectOfType<Uploader>();
        loader = FindObjectOfType<Loader>();
    }

    private void Start()
    {
        InitializeGameState();

    }

    private void InitializeGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndexMG2"))
        {
            SyncGameState();
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            PickQuestion();
        }
    }

    private void PickQuestion()
    {
        int index = random.Next(questionList.Length);

        try
        {
            ExitGames.Client.Photon.Hashtable propsToSet = new ExitGames.Client.Photon.Hashtable() { { "questionIndexMG2", index } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(propsToSet);
        }
        catch (Exception e)
        {
            Debug.Log("Error Detect : " + e);
        }
    }

    void SyncGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndexMG2"))
        {
            int questionIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["questionIndexMG2"];
            // 해당 인덱스를 사용하여 게임 상태를 업데이트
            PickedQuestion = questionList[questionIndex];
            PickedAnswer = new string[] { answerList[questionIndex, 0], answerList[questionIndex, 1]};

            Debug.Log("MiniGame2의 문제는 : " + PickedQuestion);

            Transform question = baseDisplay.transform.Find("Question");
            Transform buttonLeft = baseDisplay.transform.Find("ButtonLeft");
            Transform buttonRight = baseDisplay.transform.Find("ButtonRight");

            question.GetComponent<Text>().text = PickedQuestion;
            buttonLeft.GetComponentInChildren<Text>().text = PickedAnswer[0];
            buttonRight.GetComponentInChildren<Text>().text = PickedAnswer[1];

            //canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().SetSelectDisplayConcept(PickedQuestion);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("questionIndexMG2"))
        {
            SyncGameState();
        }
    }

    public void AfterSelection(int index)
    {
        // 투표 했음을 알리고, 전체 투표수가 전체 인원과 같은지 확인하는 코드.
        // 선택지 반영.
        WaitAnoterPlayer();
        uploader.UploadVotedCount(canvasManager.GetCurrCanvas());
        uploader.UploadPlayerChoiceMG2(index);
    }

    public void WaitAnoterPlayer()
    {
        SetActiveDisplay("baseDisplay", true);
        baseDisplay.transform.Find("ButtonLeft").gameObject.SetActive(false);
        baseDisplay.transform.Find("ButtonRight").gameObject.SetActive(false);
        baseDisplay.transform.Find("Question").GetComponent<Text>().text = "선택 완료했습니다!";
    }

    public void OnAllPlayersSelected()
    {
        ProcessLeastCountChoice();
    }

    private async void ProcessLeastCountChoice()
    {
        var (leastCountChoiceIdx,  players) = await loader.FindChoiceAndPlayer();

        Transform topScorer = resultDisplay.transform.Find("Result");

        if (players.Count == GameManager.Instance.GetPlayerMaxNumber() / 2) // 동점 처리.
        {
            topScorer.GetComponent<Text>().text += "두 선택지를 선택한 사람의 수가 동일합니다.";

            SetActiveDisplay("waitDisplay", false);
            SetActiveDisplay("resultDisplay", true);
            return;
        }

        topScorer.GetComponent<Text>().text = $"가장 적게 선택된 선택지는 {PickedAnswer[leastCountChoiceIdx]} 입니다.\n해당 선택지 선택한 사람은\n";
        if (players.Count == 0) topScorer.GetComponent<Text>().text += "아무도 없습니다. \n";
        
        foreach (int player in players)
        {
            Debug.Log($"Selector: ActorNumber {player}");
            string targetName = "";
            try
            {
                targetName = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().GetPlayerName(player);
                if (PhotonNetwork.IsMasterClient)
                {
                    canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().DistributeMiniGameReward(player);
                }                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            topScorer.GetComponent<Text>().text += $"{targetName}\n";
        }

        SetActiveDisplay("waitDisplay", false);
        SetActiveDisplay("resultDisplay", true);
    }

    public void StartMiniGame()
    {
        SetActiveDisplay("waitDisplay", false);
        SetActiveDisplay("baseDisplay", true);
        // timer 시작 함수.
        /*
        AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
        auxiliaryCanvas.StartTimer();
        */
    }

    public void TimeOver()
    {
        AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
        SetActiveDisplay("baseDisplay", false);
        auxiliaryCanvas.SetActiveDisplay("timer", false);
        auxiliaryCanvas.SetActiveDisplay("miniGameSelectDisplay", true);
    }

    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "baseDisplay":
                baseDisplay.SetActive(sign);
                break;
            case "waitDisplay":
                waitDisplay.SetActive(sign);
                break;
            case "resultDisplay":
                resultDisplay.SetActive(sign);
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
            case "baseDisplay":
                gameObject = baseDisplay;
                break;
            case "waitDisplay":
                gameObject = waitDisplay;
                break;
            case "resultDisplay":
                gameObject = resultDisplay;
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }
}
