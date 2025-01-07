using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AuxiliaryCanvas : MonoBehaviour
{
    public GameObject roomDisplay;
    public GameObject selectDisplay;

    private void Start()
    {
        UpdatePlayerDisplay();
    }

    private void UpdatePlayerDisplay()
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 모든 자식 Transform을 한 번에 가져옴 (비활성화 포함)
        Transform[] roomDisplayChildren = roomDisplay.GetComponentsInChildren<Transform>(true);
        Transform[] selectDisplayChildren = selectDisplay.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < maxPlayers / 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                string gender = j == 0 ? "Male" : "Female";
                string playerName = $"{gender}{i + 1}";

                Transform sourceTransform = roomDisplayChildren.FirstOrDefault(t => t.name == playerName);
                Transform targetTransform = selectDisplayChildren.FirstOrDefault(t => t.name == playerName);

                if (sourceTransform == null || targetTransform == null)
                {
                    Debug.LogWarning($"Could not find source or target transform for {playerName}");
                    continue;
                }

                // PlayerName 복사
                Text targetNameText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                Text sourceNameText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                if (targetNameText != null && sourceNameText != null)
                    targetNameText.text = sourceNameText.text;


                // PlayerActorNumber 복사
                Text targetActorNumberText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                Text sourceActorNumberText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                if (targetActorNumberText != null && sourceActorNumberText != null)
                    targetActorNumberText.text = sourceActorNumberText.text;

                // ImageSource 복사
                Transform maskTransform; // 이러는 이유 : FirstOrDefault(t => t.name == "Mask/ImageSource") 로는 찾을 수가 없었음. 자식과 부모로 인식하지 못하고 이름 그 자체로 인식함.
                maskTransform = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Mask");
                Image targetImage = maskTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ImageSource").GetComponent<Image>();
                maskTransform = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Mask");
                Image sourceImage = maskTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ImageSource").GetComponent<Image>();

                if (targetImage != null && sourceImage != null)
                    targetImage.sprite = sourceImage.sprite;

                if (targetNameText != null && targetNameText.text == "Empty")
                    targetTransform.gameObject.SetActive(false);
            }
        }
    }

    public void SetSelectDisplayConcept(string str)
    {
        Transform concept = selectDisplay.transform.Find("Concept");
        concept.GetComponent<Text>().text = str;
    }

    public string GetSelectDisplayConcept() { return selectDisplay.transform.Find("Concept").GetComponent<Text>().text; }

    public string GetPlayerName(int targetActorNumber)
    {
        Transform[] selectDisplayChildren = selectDisplay.GetComponentsInChildren<Transform>(true);

        string targetName = "";
        foreach(Transform t in selectDisplayChildren)
        {
            if (t.name == "Concept") continue;

            Transform playerActorNumberTransform = t.Find("PlayerActorNumber");
            if(playerActorNumberTransform != null)
            {
                Debug.Log($"t : {t}");
                if (playerActorNumberTransform.GetComponent<Text>().text == targetActorNumber.ToString())
                {
                    targetName = playerActorNumberTransform.transform.parent.Find("PlayerName").GetComponent<Text>().text;
                    break;
                }
            }
        }

        Debug.Log($"targetName : {targetName}");
        return targetName;
    }
}
