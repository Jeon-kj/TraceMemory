using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCanvas : MonoBehaviour
{
    private Uploader uploader;

    public GameObject[] questions; // �� �гο� ���� ���� (Inspector���� �г��� ����)
    private int[] answers; // �� ������ ���� ������� ���� �迭�� ���������� ����.

    private bool nextSign = false;

    private void Awake()
    {
        uploader = FindObjectOfType<Uploader>();
    }

    private void Start()
    {
        answers = new int[questions.Length];
        Init();
    }

    void Init()
    {
        questions[0].gameObject.SetActive(true);
    }

    public void SubmitAnswer()
    {
        // �� �г��� ��ȸ�ϸ� ��� ���¸� Ȯ��
        for (int i = 0; i < questions.Length; i++)
        {
            answers[i] = -1;

            ToggleGroup toggleGroup = questions[i].GetComponent<ToggleGroup>();
            Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // ���� Ȱ��ȭ�� ��� �������� (���õ� �亯)

            Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
            if (activeToggle != null)
            {
                answers[i] = Array.IndexOf(toggles, activeToggle);
                Debug.Log("Panel " + (i + 1) + " Selected Toggle Index: " + answers[i]);
            }

            // questionDictionary ����. -> 0�� ������ 2���̶�� ��� �̷������� ����ϴµ�, 0�� ������ ���� 2�� ������ �������� �˱� ����.
            uploader.QuestionInDictonary(i, questions[i].transform.Find("Question").GetComponent<Text>().text);

            for(int j=0;j < toggles.Length; j++)
            {
                uploader.ResponseInDictonary(i, j, toggles[j].transform.Find("Label").GetComponent<Text>().text);
            }
        }

        uploader.UploadAnswers(answers);
    }

    public void CheckAnswer(GameObject question)
    {
        ToggleGroup toggleGroup = question.GetComponent<ToggleGroup>();
        Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // ���� Ȱ��ȭ�� ��� ��������

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
}
