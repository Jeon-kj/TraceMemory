using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCanvas : MonoBehaviour
{
    private Uploader uploader;

    public GameObject[] questions; // 각 패널에 대한 참조 (Inspector에서 패널을 연결)
    private int[] answers; // 각 질문에 대한 사용자의 답을 배열에 정수형으로 저장.

    private bool nextSign = false;

    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
    }

    private void Start()
    {
        answers = new int[questions.Length];
    }

    public void SubmitAnswer()
    {
        // 각 패널을 순회하며 토글 상태를 확인
        for (int i = 0; i < questions.Length; i++)
        {
            ToggleGroup toggleGroup = questions[i].GetComponent<ToggleGroup>();
            Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // 현재 활성화된 토글 가져오기 (선택된 답변)

            if (activeToggle != null)
            {
                answers[i] = activeToggle.transform.GetSiblingIndex(); // 선택된 토글의 인덱스 저장
                Debug.Log("Panel " + (i + 1) + " Selected Toggle Index: " + answers[i]);
            }

            
        }

        uploader.UploadAnswers(answers);
    }

    public void CheckAnswer(GameObject question)
    {
        ToggleGroup toggleGroup = question.GetComponent<ToggleGroup>();
        Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // 현재 활성화된 토글 가져오기

        if(activeToggle == null)
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
}
