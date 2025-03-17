using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorCanvas : MonoBehaviour
{
    public static ErrorCanvas Instance; // �̱��� �ν��Ͻ�

    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessageText;
    private System.Action onErrorConfirmed; // ���� Ȯ�� �� ������ �Լ� ����

    void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �ɼ�: �� ��ȯ�� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� �ν��Ͻ��� �����Ǹ� �ı�
        }
    }

    public void ShowErrorMessage(string message, System.Action onConfirm)
    {
        errorMessageText.text = message;
        onErrorConfirmed = onConfirm; // Ȯ�� ��ư Ŭ�� �� ������ ���� ����
        errorPanel.SetActive(true);
    }

    // "Ȯ��" ��ư�� ������ �� ����� �Լ�
    public void OnErrorConfirmed()
    {
        errorPanel.SetActive(false);
        onErrorConfirmed?.Invoke(); // null�� �ƴҶ� onErrorConfirmed ����.
    }

    /*
     ��뿹��
    ShowErrorMessage("�ش� �ڵ��� ���� �������� �ʽ��ϴ�.", () =>
    {
        inputFieldRoomCode.text = ""; // �� �ڵ� �Է� �ʵ� �ʱ�ȭ
        inputFieldRoomCode.ActivateInputField(); // �Է� â �ٽ� Ȱ��ȭ
    });
     */
}
