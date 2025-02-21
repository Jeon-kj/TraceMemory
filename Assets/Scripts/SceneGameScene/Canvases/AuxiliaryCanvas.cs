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
    public GameObject roomDisplay;
    public GameObject miniGameSelectDisplay;
    public GameObject timer;
    public GameObject rewardEffect;

    private bool timerSign = false;
    private System.Random random = new System.Random();  // Random ��ü ����

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

        // ��� �ڽ� Transform�� �� ���� ������ (��Ȱ��ȭ ����)
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

                // PlayerName ����
                Text targetNameText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                Text sourceNameText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                                
                if (targetNameText != null && sourceNameText != null)
                    targetNameText.text = sourceNameText.text;


                // PlayerActorNumber ����
                Text targetActorNumberText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                Text sourceActorNumberText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                if (targetActorNumberText != null && sourceActorNumberText != null)
                    targetActorNumberText.text = sourceActorNumberText.text;

                // ImageSource ����
                Transform maskTransform; // �̷��� ���� : FirstOrDefault(t => t.name == "Mask/ImageSource") �δ� ã�� ���� ������. �ڽİ� �θ�� �ν����� ���ϰ� �̸� �� ��ü�� �ν���.
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
                // �־��� �ð��� �� �Ǿ��� ��,
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
            yield return new WaitForSeconds(1); // 1�ʸ��� ������Ʈ
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
        // �������� Uploader.SetStartTime���� ȣ���ϴ� PunRPC�� ���� ����ȭ.
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
        DebugCanvas.Instance.DebugLog($"Received RPC Call Sign, Execute ReceiveMiniGameReward!");
        int AN = PhotonNetwork.LocalPlayer.ActorNumber;
        int PAN = await loader.PartnerActorNumber(AN);  // PartnerAN

        // 1. �������� ��ȣ�� ��÷�Ͽ� �ʱ⿡ �����ߴ� ������ �̾ƿ�.
        List<int> infoIndexList = new List<int>();
        string SignReceivedPartnerInfo = "";
        try
        {
            SignReceivedPartnerInfo = await loader.SignReceivedPartnerInfo(AN);
            for (int i = 0; i < SignReceivedPartnerInfo.Length; i++)
            {
                if (SignReceivedPartnerInfo[i] == '0')  // 1�� �̹� ���� ������� ��. 0�� ���� ���� ���� �����̱� ������
                {
                    infoIndexList.Add(i);   // �������� ������ �����ν� ����Ʈ�� �߰���.
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        int index = -1; // �ʱⰪ�� -1�� ����
        if (infoIndexList.Count > 0)  // ����Ʈ�� ������� �ʴٸ�
        {
            index = random.Next(infoIndexList.Count); // ���� �ε��� ����
            // ���� �ش� ������ ������ ���� ó���� �ؾ���.
        }

        int questionIndex = infoIndexList[index];

        // 2. �ش� ������ ���� ��Ʈ���� ����� �޾ƿ�.
        int responseIndex;
        responseIndex = await loader.ResponseIndexToQuestion(PAN, index);   // index��° question�� ���� Partner�� ���� index�� ������.

        string question = await loader.QuestionInDictonary(questionIndex);
        string response = await loader.ResponseInDictonary(questionIndex, responseIndex);

        // 3. �̸� �ؽ�Ʈ �������� RewardEffect�� ǥ��.
        Text rewardText = rewardEffect.transform.Find("Text").GetComponent<Text>();
        rewardText.text = $"\"{question}\"��� ������ ���� ����� ¦�� \"{response}\"��� �����߽��ϴ�.";

        // 4. ���� ������ index�� ����صξ� �ߺ� ���� ����.
        if(SignReceivedPartnerInfo != "")
        {
            string newSignReceivedPartnerInfo = new string('0', SignReceivedPartnerInfo.Length);
            uploader.SignReceivedPartnerInfo(AN, newSignReceivedPartnerInfo);
        }
    }

    public void InitRewardDisplay()
    {
        Text rewardText = rewardEffect.transform.Find("Text").GetComponent<Text>();
        rewardText.text = "������ ���� ���߽��ϴ�.";
    }

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
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }
}
