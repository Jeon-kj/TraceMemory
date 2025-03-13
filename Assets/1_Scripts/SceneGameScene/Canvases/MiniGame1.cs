using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.UI;
using Photon.Realtime;

public class MiniGame1 : MonoBehaviourPunCallbacks
{
    public GameObject baseDisplay;
    public GameObject waitDisplay;
    public GameObject resultDisplay;

    private string sign = "";

    private string[] questionList = new string[]{
                        "���� ���� �� ���� �� ���� �����?",
                        "�뷡�� ���� �� �θ� �� ���� �����?",
                        "���� ���� �� �� �� ���� �����?"
                        };

    private string PickedQuestion;
    private System.Random random = new System.Random();  // Random ��ü ����

    CanvasManager canvasManager;
    Uploader uploader;
    Loader loader;

    public object rewardEffect { get; private set; }

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

    /* �� �� ������ OnRoomPropertiesUpdate ȣ�� �ȵ���?
    public override void OnEnable()
    {
        waitDisplay.SetActive(true);
        baseDisplay.SetActive(false);
        resultDisplay.SetActive(false);
        uploader.UploadReadyCount("MiniGame1");
    }*/

    private void InitializeGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndexMG1"))
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
            ExitGames.Client.Photon.Hashtable propsToSet = new ExitGames.Client.Photon.Hashtable() { { "questionIndexMG1", index } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(propsToSet);
        }
        catch (Exception e)
        {
            Debug.Log("Error Detect : " + e);
        }
    }

    void SyncGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndexMG1"))
        {
            int questionIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["questionIndexMG1"];
            // �ش� �ε����� ����Ͽ� ���� ���¸� ������Ʈ
            PickedQuestion = questionList[questionIndex];

            Debug.Log("MiniGame1�� ������ : " + PickedQuestion);

            Transform question = baseDisplay.transform.Find("Question");
            question.GetComponent<Text>().text = PickedQuestion;

            canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().SetSelectDisplayConcept(PickedQuestion);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("questionIndexMG1"))
        {
            SyncGameState();
        }
    }

    public void AfterSelection()
    {
        // ��ǥ�� �Ϸ��� �������� ������ �˷��� ��. O
        // ������ �� ���� ���� �� �ο��� ������ ��ǥ�� ���� O
        // �ð��� �� �Ǹ� ��ǥ�� ����.
        uploader.UploadVotedCount("MiniGame1");
    }

    public void OnAllPlayersVoted()
    {
        if (sign == "ProcessTopScorers") ProcessTopScorers();
        else if (sign == "ProcessTopPredictors") ProcessTopPredictors();

    }

    public async void ProcessTopScorers()
    {
        var (topScorers, highestScore) = await loader.FindTopScorers("MiniGame1");

        Debug.Log($"Highest Score: {highestScore}");
        Transform topScorer = resultDisplay.transform.Find("Result");
        Debug.Log($"topScorer Transform: {topScorer}");

        topScorer.GetComponent<Text>().text = $"�ִ� ��ǥ���� {highestScore}ǥ �Դϴ�.\n�ִ� ��ǥ�ڴ�\n";
        foreach (int scorer in topScorers)
        {
            Debug.Log($"Top Scorer: ActorNumber {scorer}");
            string targetName = "";
            try
            {
                targetName = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().GetPlayerName(scorer);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            topScorer.GetComponent<Text>().text += $"{targetName}\n";
        }

        SetActiveDisplay("waitDisplay", false);
        canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().SetActiveDisplay("miniGameSelectDisplay", true);
    }

    public async void ProcessTopPredictors()
    {
        var (topScorers, highestScore) = await loader.FindTopScorers("MiniGame1");
        var topScorerPredictors = await loader.FindTopScorerPredictors("MiniGame1", topScorers);

        Transform topScorer = resultDisplay.transform.Find("Result");

        topScorer.GetComponent<Text>().text += $"������ ������ �÷��̾��\n";
        if (topScorerPredictors.Count == 0) topScorer.GetComponent<Text>().text += "�ƹ��� �����ϴ�.";

        foreach (int predictor in topScorerPredictors)
        {
            string targetName = "";
            try
            {
                targetName = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().GetPlayerName(predictor);
                if (PhotonNetwork.IsMasterClient)
                {
                    canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().DistributeMiniGameReward(predictor);
                }                    
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            topScorer.GetComponent<Text>().text += $"{targetName}\n";
        }
    }

    public void SetSign(string s) { sign = s; }

    public string GetSign() { return sign; }

    public void StartMiniGame()
    {
        SetActiveDisplay("waitDisplay", false);
        SetActiveDisplay("baseDisplay", true);
        // timer ���� �Լ�.
        AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();        
        auxiliaryCanvas.StartTimer();
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
