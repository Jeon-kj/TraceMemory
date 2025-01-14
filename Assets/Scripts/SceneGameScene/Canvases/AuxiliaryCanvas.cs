using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AuxiliaryCanvas : MonoBehaviour
{
    public GameObject roomDisplay;
    public GameObject selectDisplay;
    public GameObject timer;

    private bool timerSign = false;

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
}
