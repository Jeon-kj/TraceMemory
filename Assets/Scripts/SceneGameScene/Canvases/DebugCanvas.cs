using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance; // �̱��� �ν��Ͻ�
    public Text debugText;

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

    public void DebugLog(string msg)
    {
        debugText.text += "\n";
        debugText.text += msg;
    }
}
