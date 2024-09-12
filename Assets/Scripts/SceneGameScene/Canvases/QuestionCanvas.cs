using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCanvas : MonoBehaviour
{
    private Uploader uploader;

    public GameObject[] questions; // �� �гο� ���� ���� (Inspector���� �г��� ����)
    private int[] answers; // �� ������ ���� ������� ���� �迭�� ���������� ����.


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
        // �� �г��� ��ȸ�ϸ� ��� ���¸� Ȯ��
        for (int i = 0; i < questions.Length; i++)
        {
            // �г� ���� ��� ����� ������
            Toggle[] toggles = questions[i].GetComponentsInChildren<Toggle>();

            Debug.Log("Panel " + (i + 1) + " Responses:");

            // �� ����� ���� ���¸� Ȯ��
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
