using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AuxiliaryCanvas : MonoBehaviour
{
    public GameObject roomDisplay;
    public GameObject selectDisplay;
    public GameObject timer;

    private bool timerSign = false;
    private System.Random random = new System.Random();  // Random 객체 생성

    Uploader uploader;
    Loader loader;
    CanvasManager canvasManager;

    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
        loader = FindObjectOfType<Loader>();
        canvasManager = FindObjectOfType<CanvasManager>();
    }

    private void Start()
    {
        UpdatePlayerDisplay();
    }

    private void UpdatePlayerDisplay()
    {
        int maxPlayers = 6;

        // 모든 자식 Transform을 한 번에 가져옴 (비활성화 포함)
        Transform[] roomDisplayChildren = roomDisplay.GetComponentsInChildren<Transform>(true);
        Transform[] selectDisplayChildren = selectDisplay.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < maxPlayers / 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                string gender = j == 0 ? "Male" : "Female";
                string playerName = $"{gender}{i + 1}";

                Transform sourceTransform = roomDisplayChildren.FirstOrDefault(t => t.name == playerName);
                Transform targetTransform = selectDisplayChildren.FirstOrDefault(t => t.name == playerName);

                if (sourceTransform == null || targetTransform == null)
                {
                    Debug.LogWarning($"Could not find source or target transform for {playerName}");
                    continue;
                }

                // PlayerName 복사
                Text targetNameText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                Text sourceNameText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                if (targetNameText != null && sourceNameText != null)
                    targetNameText.text = sourceNameText.text;


                // PlayerActorNumber 복사
                Text targetActorNumberText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                Text sourceActorNumberText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                if (targetActorNumberText != null && sourceActorNumberText != null)
                    targetActorNumberText.text = sourceActorNumberText.text;

                // ImageSource 복사
                Transform maskTransform; // 이러는 이유 : FirstOrDefault(t => t.name == "Mask/ImageSource") 로는 찾을 수가 없었음. 자식과 부모로 인식하지 못하고 이름 그 자체로 인식함.
                maskTransform = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Mask");
                Image targetImage = maskTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ImageSource").GetComponent<Image>();
                maskTransform = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Mask");
                Image sourceImage = maskTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ImageSource").GetComponent<Image>();

                if (targetImage != null && sourceImage != null)
                    targetImage.sprite = sourceImage.sprite;

                if (targetNameText != null && targetNameText.text == "Empty")
                    targetTransform.gameObject.SetActive(false);
            }
        }
    }

    public void SetSelectDisplayConcept(string str)
    {
        Transform concept = selectDisplay.transform.Find("Concept");
        concept.GetComponent<Text>().text = str;
    }

    public string GetSelectDisplayConcept() { return selectDisplay.transform.Find("Concept").GetComponent<Text>().text; }

    public string GetPlayerName(int targetActorNumber)
    {
        Transform[] selectDisplayChildren = selectDisplay.GetComponentsInChildren<Transform>(true);

        string targetName = "";
        foreach(Transform t in selectDisplayChildren)
        {
            if (t.name == "Concept") continue;

            Transform playerActorNumberTransform = t.Find("PlayerActorNumber");
            if(playerActorNumberTransform != null)
            {
                Debug.Log($"t : {t}");
                if (playerActorNumberTransform.GetComponent<Text>().text == targetActorNumber.ToString())
                {
                    targetName = playerActorNumberTransform.transform.parent.Find("PlayerName").GetComponent<Text>().text;
                    break;
                }
            }
        }

        Debug.Log($"targetName : {targetName}");
        return targetName;
    }

    // About Timer
    private IEnumerator TimerCoroutine(DateTime startTime, int duration)
    {
        Debug.Log("TimerCoroutine");
        TimeSpan timeLimit = TimeSpan.FromSeconds(duration);
        while (true)
        {
            TimeSpan elapsedTime = DateTime.UtcNow - startTime;
            
            if (elapsedTime >= timeLimit)
            {
                // 주어진 시간이 다 되었을 때,
                Debug.Log("Timer ended.");
                timerSign = false;
            }

            if (timerSign == false)
            {
                string currCanvas = canvasManager.GetCurrCanvas();
                uploader.InitReadyCount(currCanvas + "Timer");

                if (currCanvas == "MiniGame1")
                    canvasManager.MiniGame1.GetComponent<MiniGame1>().TimeOver();
                else if (currCanvas == "MiniGame2")
                    canvasManager.MiniGame2.GetComponent<MiniGame2>().TimeOver();
                break;
            }

            int remainingTime = (int)(timeLimit-elapsedTime).TotalSeconds;
            Debug.Log("Time remaining: " + remainingTime + " seconds");
            timer.transform.Find("Text").GetComponent<Text>().text = remainingTime.ToString();
            yield return new WaitForSeconds(1); // 1초마다 업데이트
        }
    }

    public void InitializeLocalTimer(DateTime startTime, int duration)
    {
        SetTimerSign(true);
        StartCoroutine(TimerCoroutine(startTime, duration));
    }

    public void StartTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            uploader.SetStartTime();
        }
        // 나머지는 Uploader.SetStartTime에서 호출하는 PunRPC를 통해 동기화.
    }

    public void SetTimerSign(bool sign) { timerSign = sign; }

    // About Reward
    public void DistributeMiniGameReward(int actorNumber)
    {
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if(player.ActorNumber == actorNumber)
            {
                ReceiveMiniGameReward();
                break;
            }
        }
    }

    [PunRPC]
    async void ReceiveMiniGameReward()
    {
        int AN = PhotonNetwork.LocalPlayer.ActorNumber;
        // 1. 무작위의 번호를 추첨하여 초기에 조사했던 질문을 뽑아옴.
        List<int> infoIndexList = new List<int>();
        string SignReceivedPartnerInfo;
        try
        {
            SignReceivedPartnerInfo = await loader.SignReceivedPartnerInfo(AN);
            for (int i = 0; i < SignReceivedPartnerInfo.Length; i++)
            {
                if (SignReceivedPartnerInfo[i] == '0')  // 1은 이미 얻은 정보라는 뜻. 0은 아직 얻지 않은 정보이기 때문에
                {
                    infoIndexList.Add(i);   // 랜덤으로 추출할 정보로써 리스트에 추가됨.
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        int index = -1; // 초기값을 -1로 설정
        if (infoIndexList.Count > 0)  // 리스트가 비어있지 않다면
        {
            index = random.Next(infoIndexList.Count); // 랜덤 인덱스 선택
        }

        int questionIndex = infoIndexList[index];

        // 2. 해당 질문에 대한 파트너의 대답을 받아옴.
        int responseIndex;
        try
        {
            responseIndex = await loader.ResponseIndexToQuestion(AN, index);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        // 여기까지 하면 몇번 질문에 몇번 선택지를 찍었다! 까지는 알았음. 그럼 몇번 질문이 뭐고, 몇번 선택지는 뭐다! 를 알아야 함.
        //loader.QuestionInDictonary();
        //loader.ResponseInDictonary(); 이거 구현.
        // 3. 이를 텍스트 형식으로 RewardEffect에 표시.
        // 4. 받은 질문의 index를 기록해두어 중복 보상 방지.
        // 5. 그러기 위해서는 초기 역할 분배 때, 자신의 짝의 actorNumber를 알아둬야 함.
    }
}
