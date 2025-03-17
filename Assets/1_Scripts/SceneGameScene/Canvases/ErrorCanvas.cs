using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorCanvas : MonoBehaviour
{
    public static ErrorCanvas Instance; // 싱글턴 인스턴스

    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessageText;
    private System.Action onErrorConfirmed; // 에러 확인 후 실행할 함수 저장

    void Awake()
    {
        // 싱글턴 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 옵션: 씬 전환시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스가 생성되면 파괴
        }
    }

    public void ShowErrorMessage(string message, System.Action onConfirm)
    {
        errorMessageText.text = message;
        onErrorConfirmed = onConfirm; // 확인 버튼 클릭 시 실행할 동작 저장
        errorPanel.SetActive(true);
    }

    // "확인" 버튼을 눌렀을 때 실행될 함수
    public void OnErrorConfirmed()
    {
        errorPanel.SetActive(false);
        onErrorConfirmed?.Invoke(); // null이 아닐때 onErrorConfirmed 실행.
    }

    /*
     사용예시
    ShowErrorMessage("해당 코드의 방은 존재하지 않습니다.", () =>
    {
        inputFieldRoomCode.text = ""; // 방 코드 입력 필드 초기화
        inputFieldRoomCode.ActivateInputField(); // 입력 창 다시 활성화
    });
     */
}
