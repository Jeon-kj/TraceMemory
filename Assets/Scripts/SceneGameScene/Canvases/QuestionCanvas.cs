using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCanvas : MonoBehaviour
{
    private Uploader uploader;

    public GameObject[] questions; // 각 패널에 대한 참조 (Inspector에서 패널을 연결)
    private int[] answers; // 각 질문에 대한 사용자의 답을 배열에 정수형으로 저장.


    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
    }

    private void Start()
    {
        answers = new int[questions.Length];
    }

    public void SubmitAnwer()
    {
        // 각 패널을 순회하며 토글 상태를 확인
        for (int i = 0; i < questions.Length; i++)
        {
            // 패널 내의 모든 토글을 가져옴
            Toggle[] toggles = questions[i].GetComponentsInChildren<Toggle>();

            Debug.Log("Panel " + (i + 1) + " Responses:");

            // 각 토글의 선택 상태를 확인
            for (int j = 0; j < toggles.Length; j++)
            {
                bool isToggleOn = toggles[j].isOn;
                if (isToggleOn)
                {
                    answers[i] = j;
                    break;
                }
            }
        }

        uploader.UploadAnswers(answers);
        //uploader.UploadAnswers(answers, mockActorNumber: 1234, mockRoomCode: "TestRoom123");
    }
}
