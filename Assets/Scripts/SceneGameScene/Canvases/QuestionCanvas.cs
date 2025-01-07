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
    }

    public void SubmitAnswer()
    {
        // �� �г��� ��ȸ�ϸ� ��� ���¸� Ȯ��
        for (int i = 0; i < questions.Length; i++)
        {
            ToggleGroup toggleGroup = questions[i].GetComponent<ToggleGroup>();
            Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // ���� Ȱ��ȭ�� ��� �������� (���õ� �亯)

            if (activeToggle != null)
            {
                answers[i] = activeToggle.transform.GetSiblingIndex(); // ���õ� ����� �ε��� ����
                Debug.Log("Panel " + (i + 1) + " Selected Toggle Index: " + answers[i]);
            }

            
        }

        uploader.UploadAnswers(answers);
    }

    public void CheckAnswer(GameObject question)
    {
        ToggleGroup toggleGroup = question.GetComponent<ToggleGroup>();
        Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault(); // ���� Ȱ��ȭ�� ��� ��������

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
