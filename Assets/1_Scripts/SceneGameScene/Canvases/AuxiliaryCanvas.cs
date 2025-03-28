using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AuxiliaryCanvas : MonoBehaviourPunCallbacks
{
    [Header("Family Section")]    
    public GameObject miniGameSelectDisplay;
    public GameObject timer;
    public GameObject rewardEffect;
    public GameObject winner;
    [Space(10)]

    [Header("Other Section")]
    public GameObject roomDisplay;
    [Space(10)]

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

    public void FirstTurnOnCanvas()
    {
        UpdatePlayerDisplay();
    }

    private void UpdatePlayerDisplay()
    {
        int maxPlayers = 6;

        // 모든 자식 Transform을 한 번에 가져옴 (비활성화 포함)
        Transform[] roomDisplayChildren = roomDisplay.GetComponentsInChildren<Transform>(true);
        Transform[] selectDisplayChildren = miniGameSelectDisplay.GetComponentsInChildren<Transform>(true);
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

                // 공통적인 UI 업데이트 함수 호출
                UpdateProfileUI(sourceTransform, targetTransform);

                // 만약 플레이어가 비어 있다면 비활성화
                Text targetNameText = targetTransform.Find("PlayerName").GetComponent<Text>();
                if (targetNameText != null && targetNameText.text == "Empty")
                    targetTransform.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateProfileUI(Transform sourceProfile, Transform targetProfile)
    {
        // PlayerName 복사
        Text targetNameText = targetProfile.Find("PlayerName").GetComponent<Text>();
        Text sourceNameText = sourceProfile.Find("PlayerName").GetComponent<Text>();

        if (targetNameText != null && sourceNameText != null)
            targetNameText.text = sourceNameText.text;

        // PlayerActorNumber 복사
        Text targetActorNumberText = targetProfile.Find("PlayerActorNumber").GetComponent<Text>();
        Text sourceActorNumberText = sourceProfile.Find("PlayerActorNumber").GetComponent<Text>();

        if (targetActorNumberText != null && sourceActorNumberText != null)
            targetActorNumberText.text = sourceActorNumberText.text;

        // ImageSource 복사
        Image sourceImage = sourceProfile.Find("Mask/ImageSource").GetComponent<Image>();
        Image targetImage = targetProfile.Find("Mask/ImageSource").GetComponent<Image>();

        if (targetImage != null && sourceImage != null)
            targetImage.sprite = sourceImage.sprite;
    }

    public void SetSelectDisplayConcept(string str)
    {
        Transform concept = miniGameSelectDisplay.transform.Find("Concept");
        concept.GetComponent<Text>().text = str;
    }

    public string GetSelectDisplayConcept() { return miniGameSelectDisplay.transform.Find("Concept").GetComponent<Text>().text; }

    public string GetPlayerName(int targetActorNumber)
    {
        Transform[] selectDisplayChildren = miniGameSelectDisplay.GetComponentsInChildren<Transform>(true);

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
                photonView.RPC("ReceiveMiniGameReward", player);
                break;
            }
        }
    }

    [PunRPC]
    async void ReceiveMiniGameReward()
    {
        int AN = PhotonNetwork.LocalPlayer.ActorNumber;
        int PAN = await loader.PartnerActorNumber(AN);  // PartnerAN

        // 1. 무작위의 번호를 추첨하여 초기에 조사했던 질문을 뽑아옴.
        List<int> infoIndexList = new List<int>();
        string SignReceivedPartnerInfo = "";
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
            // 이제 해당 질문을 얻은적 있음 처리를 해야함.
        }

        int questionIndex = infoIndexList[index];

        // 2. 해당 질문에 대한 파트너의 대답을 받아옴.
        int responseIndex;
        responseIndex = await loader.ResponseIndexToQuestion(PAN, index);   // index번째 question에 대한 Partner의 응답 index를 가져옴.

        string question = await loader.QuestionInDictonary(questionIndex);
        string response = await loader.ResponseInDictonary(questionIndex, responseIndex);

        // 3. 이를 텍스트 형식으로 RewardEffect에 표시.
        Text rewardText = rewardEffect.transform.Find("Text").GetComponent<Text>();
        rewardText.text = $"\"{question}\"라는 질문에 대해 당신의 짝은 \"{response}\"라고 응답했습니다.";

        // 4. 받은 질문의 index를 기록해두어 중복 보상 방지.
        if(SignReceivedPartnerInfo != "")
        {
            string newSignReceivedPartnerInfo = new string('0', SignReceivedPartnerInfo.Length);
            uploader.SignReceivedPartnerInfo(AN, newSignReceivedPartnerInfo);
        }
    }

    public void InitRewardDisplay()
    {
        Text rewardText = rewardEffect.transform.Find("Text").GetComponent<Text>();
        rewardText.text = "보상을 얻지 못했습니다.";
    }

    // About Winner
    public async void GameEnd()
    {        
        // 승리한 짝 ActorNumber 계산.
        var (winningPairsAN, maxScore)  = await loader.FindWinningPairAN();
        // 승리한 짝 프로필 복사하기.
        UpdateWinnerProfiles(winningPairsAN);
        SetActiveDisplay("winner", true);
        winner.transform.Find($"WinnerPanel{winningPairsAN.Count}").gameObject.SetActive(true);
        winner.transform.Find("Text").GetComponent<Text>().text = $"호감카드의 합이 {maxScore}로 가장 높은 팀.";
    }

    public void UpdateWinnerProfiles(List<Tuple<string, string>> winningPairsAN)
    {
        int count = winningPairsAN.Count;

        // roomDisplay에 있는 모든 Transform 중 "Mask"오브젝트가 자식으로 있는 것만 가져옴 (즉, 프로필 이미지 Transform들)
        Transform[] sourceProfiles = roomDisplay.GetComponentsInChildren<Transform>(true)
            .Where(child => child.name == "Mask")  // 이름이 "Mask"인 오브젝트만 필터링
            .Select(mask => mask.parent)  // "Mask" 오브젝트의 부모를 가져옴
            .ToArray();
        // 자식 오브젝트로 프로필 이미지를 가진 패널의 Transform
        // 우승 커플의 수에 따라 할당되는 Transform이 달라짐.
        Transform winnerPanel = winner.transform.Find($"WinnerPanel{count}");
        // winnerPanel의 하위 모든 Transform (즉, 프로필 이미지들을 묶은 WinningPair{i})
        Transform[] winningPairs = winnerPanel.GetComponentsInChildren<Transform>(true);

        int i = 1;
        int j = 1;
        foreach (Tuple<string, string> winningPair in winningPairsAN)
        {
            if (i > count) break;
            foreach (Transform sourceProfile in sourceProfiles)
            {
                if (i > count) break;

                string sourceAN = sourceProfile.Find("PlayerActorNumber").GetComponent<Text>().text;

                if (winningPair.Item1 == sourceAN || winningPair.Item2 == sourceAN)
                {
                    Transform targetProfile = winningPairs[i].transform.Find($"Player{j}");
                    UpdateProfileUI(sourceProfile, targetProfile);
                    j++;

                    if (j == 3)
                    {
                        i++;
                        j = 1;
                    }
                }
            }
        }
    }


    // About Display
    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "roomDisplay":
                roomDisplay.SetActive(sign);
                break;
            case "miniGameSelectDisplay":
                miniGameSelectDisplay.SetActive(sign);
                break;
            case "timer":
                timer.SetActive(sign);
                break;
            case "rewardEffect":
                rewardEffect.SetActive(sign);
                break;
            case "winner":
                winner.SetActive(sign);
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
            case "roomDisplay":
                gameObject = roomDisplay;
                break;
            case "miniGameSelectDisplay":
                gameObject = miniGameSelectDisplay;
                break;
            case "timer":
                gameObject = timer;
                break;
            case "rewardEffect":
                gameObject = rewardEffect;
                break;
            case "winner":
                gameObject = winner;
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }

    public void SetInit()
    {
        GetPanel("timer").transform.Find("ButtonSkip").gameObject.SetActive(true);
        miniGameSelectDisplay.transform.Find("Concept").GetComponent<Text>().text = "";
    }
}
