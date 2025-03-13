using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance; // 싱글턴 인스턴스
    public Text debugText;

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

    public void DebugLog(string msg)
    {
        debugText.text += "\n";
        debugText.text += msg;
        Debug.Log(msg);
    }

    public void ToggleDisplay(GameObject displayToToggle, GameObject otherDisplay)
    {
        // 현재 패널 활성화/비활성화
        if (displayToToggle != null)
        {
            displayToToggle.SetActive(!displayToToggle.activeSelf);
        }

        // 다른 패널이 활성화되어 있으면 비활성화
        if (otherDisplay != null && otherDisplay.activeSelf)
        {
            otherDisplay.SetActive(false);
        }
    }
}
