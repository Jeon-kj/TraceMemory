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
    public GameObject resultDisplay;

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

    public void InitializeGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndex"))
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
            ExitGames.Client.Photon.Hashtable propsToSet = new ExitGames.Client.Photon.Hashtable() { { "questionIndex", index } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(propsToSet);
        }
        catch (Exception e)
        {
            Debug.Log("Error Detect : " + e);
        }
    }

    void SyncGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("questionIndex"))
        {
            int questionIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["questionIndex"];
            // 해당 인덱스를 사용하여 게임 상태를 업데이트
            PickedQuestion = questionList[questionIndex];

            Debug.Log("MiniGame1의 질문은 : " + PickedQuestion);

            Transform question = baseDisplay.transform.Find("Question");
            question.GetComponent<Text>().text = PickedQuestion;

            canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().UpdateSelectDisplayConcept(PickedQuestion);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("questionIndex"))
        {
            SyncGameState();
        }
    }


    public void CompleteSelecting()
    {
        // 본 플레이어는 클릭을 하였음. O
        // 이후에 뭘 해야할 지, ButtonManager에서 호출용도.  O
        WaitAnoterPlayer();
        CompleteSignSendToServer();
    }

    public void CompleteSignSendToServer()
    {
        // 투표를 완료한 상태임을 서버에 알려야 함. O
        // 서버는 그 수를 세서 총 인원이 맞으면 투표를 종료 O
        // 시간이 다 되면 투표를 종료. 
        uploader.UploadVoting("MiniGame1");
    }

    public void VotingEnd()
    {
        ProcessTopScorers();
    }

    public async void ProcessTopScorers()
    {
        var (topScorers, highestScore) = await loader.FindTopScorersAsync("MiniGame1");
        Debug.Log($"Highest Score: {highestScore}");
        Transform topScorer = resultDisplay.transform.Find("Result");
        Debug.Log($"topScorer Transform: {topScorer}");
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
        baseDisplay.SetActive(false);
        resultDisplay.SetActive(true);
    }


    public void WaitAnoterPlayer()
    {
        baseDisplay.gameObject.SetActive(true);
        baseDisplay.transform.Find("Button").gameObject.SetActive(false);
        baseDisplay.transform.Find("Question").GetComponent<Text>().text = "선택 완료했습니다!";
    }
}
