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
                        "���� ���� �� ���� �� ���� �����?",
                        "�뷡�� ���� �� �θ� �� ���� �����?",
                        "���� ���� �� �� �� ���� �����?"
                        };

    private string PickedQuestion;
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
            // �ش� �ε����� ����Ͽ� ���� ���¸� ������Ʈ
            PickedQuestion = questionList[questionIndex];

            Debug.Log("MiniGame1�� ������ : " + PickedQuestion);

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
        // �� �÷��̾�� Ŭ���� �Ͽ���. O
        // ���Ŀ� �� �ؾ��� ��, ButtonManager���� ȣ��뵵.  O
        WaitAnoterPlayer();
        CompleteSignSendToServer();
    }

    public void CompleteSignSendToServer()
    {
        // ��ǥ�� �Ϸ��� �������� ������ �˷��� ��. O
        // ������ �� ���� ���� �� �ο��� ������ ��ǥ�� ���� O
        // �ð��� �� �Ǹ� ��ǥ�� ����. 
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
        baseDisplay.transform.Find("Question").GetComponent<Text>().text = "���� �Ϸ��߽��ϴ�!";
    }
}
