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
        "���� ��� ���ָ� �غ� ���� �ִ�. (2�� �̻�)",
        "�� �� �ϳ��� ��� �� �Դ´ٸ�?"
    };

    private string[,] answerList = new string[,] {
        { "O", "X" },
        { "���", "ġŲ" }
    };

    private string PickedQuestion;
    private string[] PickedAnswer;
    private System.Random random = new System.Random();  // Random ��ü ����

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
            // �ش� �ε����� ����Ͽ� ���� ���¸� ������Ʈ
            PickedQuestion = questionList[questionIndex];
            PickedAnswer = new string[] { answerList[questionIndex, 0], answerList[questionIndex, 1]};

            Debug.Log("MiniGame2�� ������ : " + PickedQuestion);

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
        // ��ǥ ������ �˸���, ��ü ��ǥ���� ��ü �ο��� ������ Ȯ���ϴ� �ڵ�.
        // ������ �ݿ�.
        WaitAnoterPlayer();
        uploader.UploadVotedCount(canvasManager.GetCurrCanvas());
        uploader.UploadPlayerChoiceMG2(index);
    }

    public void WaitAnoterPlayer()
    {
        SetActiveDisplay("baseDisplay", true);
        baseDisplay.transform.Find("ButtonLeft").gameObject.SetActive(false);
        baseDisplay.transform.Find("ButtonRight").gameObject.SetActive(false);
        baseDisplay.transform.Find("Question").GetComponent<Text>().text = "���� �Ϸ��߽��ϴ�!";
    }

    public void OnAllPlayersSelected()
    {
        ProcessLeastCountChoice();
    }

    private async void ProcessLeastCountChoice()
    {
        var (leastCountChoiceIdx,  players) = await loader.FindChoiceAndPlayer();

        Transform topScorer = resultDisplay.transform.Find("Result");

        if (players.Count == GameManager.Instance.GetPlayerMaxNumber() / 2) // ���� ó��.
        {
            topScorer.GetComponent<Text>().text += "�� �������� ������ ����� ���� �����մϴ�.";

            SetActiveDisplay("waitDisplay", false);
            SetActiveDisplay("resultDisplay", true);
            return;
        }

        topScorer.GetComponent<Text>().text = $"���� ���� ���õ� �������� {PickedAnswer[leastCountChoiceIdx]} �Դϴ�.\n�ش� ������ ������ �����\n";
        if (players.Count == 0) topScorer.GetComponent<Text>().text += "�ƹ��� �����ϴ�. \n";
        
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
        // timer ���� �Լ�.
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
