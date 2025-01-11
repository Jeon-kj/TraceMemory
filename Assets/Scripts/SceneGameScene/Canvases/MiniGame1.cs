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
                        "옷을 가장 잘 입을 거 같은 사람은?",
                        "노래를 가장 잘 부를 거 같은 사람은?",
                        "춤을 가장 잘 출 거 같은 사람은?"
                        };

    private string PickedQuestion;
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

    /* 왜 얘 있으면 OnRoomPropertiesUpdate 호출 안되지?
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
            // 해당 인덱스를 사용하여 게임 상태를 업데이트
            PickedQuestion = questionList[questionIndex];

            Debug.Log("MiniGame1의 질문은 : " + PickedQuestion);

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
        // 투표를 완료한 상태임을 서버에 알려야 함. O
        // 서버는 그 수를 세서 총 인원이 맞으면 투표를 종료 O
        // 시간이 다 되면 투표를 종료. 
        WaitAnoterPlayer();
        uploader.UploadVotedCount("MiniGame1");
    }

    public void WaitAnoterPlayer()
    {
        baseDisplay.SetActive(true);
        baseDisplay.transform.Find("Button").gameObject.SetActive(false);
        baseDisplay.transform.Find("Question").GetComponent<Text>().text = "선택 완료했습니다!";
    }

    public void OnAllPlayersVoted()
    {
        if (sign == "ProcessTopScorers") ProcessTopScorers();
        else if (sign == "ProcessTopPredictors") ProcessTopPredictors();

    }

    public async void ProcessTopScorers()
    {
        var (topScorers, highestScore) = await loader.FindTopScorers("MiniGame1");
        var topScorerPredictors = await loader.FindTopScorerPredictors("MiniGame1", topScorers);

        Debug.Log($"Highest Score: {highestScore}");
        Transform topScorer = resultDisplay.transform.Find("Result");
        Debug.Log($"topScorer Transform: {topScorer}");

        topScorer.GetComponent<Text>().text = $"최다 득표수는 {highestScore}표 입니다.\n최다 득표자는\n";
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

        waitDisplay.SetActive(false);
        canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().selectDisplay.SetActive(true);
    }

    public async void ProcessTopPredictors()
    {
        var (topScorers, highestScore) = await loader.FindTopScorers("MiniGame1");
        var topScorerPredictors = await loader.FindTopScorerPredictors("MiniGame1", topScorers);

        Transform topScorer = resultDisplay.transform.Find("Result");
        Debug.Log($"topScorerPredictors Transform: {topScorerPredictors}");

        topScorer.GetComponent<Text>().text += $"예측에 성공한 플레이어는\n";
        if (topScorerPredictors.Count == 0) topScorer.GetComponent<Text>().text += "아무도 없습니다.";

        foreach (int predictor in topScorerPredictors)
        {
            Debug.Log($"Top Predictor: ActorNumber {predictor}");
            string targetName = "";
            try
            {
                targetName = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().GetPlayerName(predictor);
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
        waitDisplay.SetActive(false);
        baseDisplay.SetActive(true);
        // timer 시작 함수.
        AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();        
        auxiliaryCanvas.StartTimer();
    }

    public void TimeOver()
    {
        AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
        baseDisplay.SetActive(false);
        auxiliaryCanvas.timer.SetActive(false);
        auxiliaryCanvas.selectDisplay.SetActive(true);
    }
}
