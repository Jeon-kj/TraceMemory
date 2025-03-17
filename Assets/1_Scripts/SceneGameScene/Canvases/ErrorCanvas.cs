using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorCanvas : MonoBehaviour
{
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessageText;
    private System.Action onErrorConfirmed; // ���� Ȯ�� �� ������ �Լ� ����

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
