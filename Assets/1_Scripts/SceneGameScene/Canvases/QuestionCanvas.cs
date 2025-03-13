using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCanvas : MonoBehaviour
{
    [Header("Family Section")]
    public GameObject[] askQuestions;
    [Space(10)]

    private Uploader uploader;

    private int[] answers; // 각 질문에 대한 사용자의 답을 배열에 정수형으로 저장.

    private bool nextSign = false;

    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
    }

    private void Start()
    {
        answers = new int[askQuestions.Length];
        Init();
    }

    void Init()
    {
        askQuestions[0].gameObject.SetActive(true);
    }

    public void SubmitAnswer()
    {
        // 각 패널을 순회하며 토글 상태를 확인
        for (int i = 0; i < askQuestions.Length; i++)
        {
            answers[i] = -1;

            ToggleGroup toggleGroup = askQuestions[i].GetComponent<ToggleGroup>();
            Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // 현재 활성화된 토글 가져오기 (선택된 답변)

            Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
            if (activeToggle != null)
            {
                answers[i] = Array.IndexOf(toggles, activeToggle);
                Debug.Log("Panel " + (i + 1) + " Selected Toggle Index: " + answers[i]);
            }

            // questionDictionary 구성. -> 0번 질문에 2번이라고 대답 이런식으로 기록하는데, 0번 질문이 뭐고 2번 질문이 무엇인지 알기 위해.
            if(PhotonNetwork.IsMasterClient)
            {
                uploader.QuestionInDictonary(i, askQuestions[i].transform.Find("Question").GetComponent<Text>().text);

                for (int j = 0; j < toggles.Length; j++)
                {
                    uploader.ResponseInDictonary(i, j, toggles[j].transform.Find("Label").GetComponent<Text>().text);
                }
            }            
        }

        uploader.UploadAnswers(answers);
    }

    public void CheckAnswer(GameObject question)
    {
        ToggleGroup toggleGroup = question.GetComponent<ToggleGroup>();
        Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // 현재 활성화된 토글 가져오기

        if (activeToggle == null)
        {
            SetNextSign(false);
        }
        else if (activeToggle != null)
        {
            SetNextSign(true);
        }
    }

    public void SetNextSign(bool sign) { nextSign = sign; }

    public bool GetNextSign() { return nextSign; }

    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "askQuestion1":
                askQuestions[0].SetActive(sign);
                break;
            case "askQuestion2":
                askQuestions[1].SetActive(sign);
                break;
            case "askQuestion3":
                askQuestions[2].SetActive(sign);
                break;
            case "askQuestion4":
                askQuestions[3].SetActive(sign);
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
            case "askQuestion1":
                gameObject = askQuestions[0];
                break;
            case "askQuestion2":
                gameObject = askQuestions[1];
                break;
            case "askQuestion3":
                gameObject = askQuestions[2];
                break;
            case "askQuestion4":
                gameObject = askQuestions[3];
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
    }
}
