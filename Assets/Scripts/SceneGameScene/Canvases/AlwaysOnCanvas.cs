using Firebase.Database;
using Photon.Pun;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnCanvas : MonoBehaviour
{
    [Header("Family Section")]
    public GameObject messageBtn;
    public GameObject loveCardBtn;
    public GameObject messageDisplay;
    public GameObject loveCardDisplay;
    [Space(10)]

    [Header("Other Section")]
    public GameObject messagePrefab; // 메시지 프리팹
    public GameObject roomDisplay;
    public Transform contentTransform; // 메시지를 추가할 Content 영역
    [Space(10)]

    private DatabaseReference databaseReference;

    private Loader loader;
    private CanvasManager canvasManager;

    private void Awake()
    {
        loader = FindObjectOfType<Loader>();
    }

    private void Start()
    {
        databaseReference = FirebaseManager.Instance.database.RootReference;

        string roomCode = GameManager.Instance.GetRoomCode();
        int targetActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // 오른쪽 상단에 위치한 호감카드 누르면 나오는 플레이어 이미지 업데이트. (플레이어 대기방을 기준으로)
        UpdatePlayerDisplay(); 
        
        // 플레이어가 받은 메시지들을 인터페이스에 추가하고, 앞으로 들어올 메시지 이벤트에 대해서도 같은 작업을 함.
        MonitorScoreOrMessage(targetActorNumber, "SecretMessage");

        // 같은 방 내에 있는 모든 플레이어들의 호감카드점수를 
        MonitorAllPlayersScoreChanges("LoveCardScore");
    }


    public void MonitorScoreOrMessage(int targetActorNumber, string Type)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // 점수 경로 설정
        DatabaseReference reference = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(Type);

        // SecretMessage의 경우에는 기존에 존재하던 정보를 일괄 업데이트한다. 이후 들어오는 새로운 정보들에 대해서도 핸들링한다.
        // LoveCard의 경우는 새로 입력되는 정보에 대해서만 핸들링을 한다.
        // 왜 이런 차이를 뒀지? 기억이 안나네. AlwaysOnCanvas는 역할 배분 이후부터 켜져 있기 때문에, 메시지 전송과 호감카드 전송보다 일찍 실행된다.
        // 그렇기 때문에 이론상 일괄 업데이트가 필요 없을텐데.. 연결이 끊겼다 다시 접속하는 경우를 대비하는 건가?
        // -> 원래 의도는 Uploader보다 Loader가 먼저 실행되어 reference 경로가 유효하지 않은 경우를 대비하기 위해 만든 거지만,
        // -> ChildAdded 이벤트 리스너는 필요 없음을 깨달음. 그러나 ValueChanged의 경우 새로 입력된 정보는 인식하지 못하기에 추가할 필요를 느낌.
        if (Type == "LoveCardScore")
        {
            // LoveCardScore 경로를 확인하고 초기화
            loader.EnsurePath(targetActorNumber, "LoveCardScore", () =>
            {
                // 경로가 확인되었거나 생성된 후 점수 변경 리스너 등록
                reference.ValueChanged += (sender, e) => loader.HandleScoreChanged(sender, e, targetActorNumber);
            });
        }
        else if(Type == "SecretMessage")
        {
            loader.EnsurePath(targetActorNumber, "SecretMessage", () =>
            {
                // 경로가 존재하거나 생성된 후 메시지 로직 실행
                loader.LoadAllMessages(reference, () =>
                {
                    // 실시간 메시지 추가
                    reference.ChildAdded += loader.HandleNewMessageAdded;
                });
            });
        }
    }

    public void MonitorAllPlayersScoreChanges(string scoreType)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int targetActorNumber = player.ActorNumber;
            MonitorScoreOrMessage(targetActorNumber, scoreType);
        }
    }


    public void AddMessageToContent(string message)
    {
        // 프리팹을 생성하고 Content 영역에 추가
        GameObject newMessageObject = Instantiate(messagePrefab, contentTransform);

        // 프리팹 내의 Text 컴포넌트를 찾아 메시지를 설정
        Text messageText = newMessageObject.GetComponentInChildren<Text>();
        if (messageText != null)
        {
            messageText.text = message;
            Debug.Log(messageText.text);
        }
        else
        {
            Debug.LogError("Message prefab does not contain a Text component.");
        }
    }

    public void OnScoreChanged(int targetActorNumber, int updatedScore)
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        Transform[] loveCardDisplayChildren = loveCardDisplay.GetComponentsInChildren<Transform>(true);
        Transform targetTransform = loveCardDisplayChildren.FirstOrDefault(t => 
            t.name == "PlayerActorNumber" && t.GetComponent<Text>().text == targetActorNumber.ToString()); // GetComponent 성능문제.
        if (targetTransform != null)
        {
            targetTransform.parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "NumberOfCard").GetComponent<Text>().text = updatedScore.ToString();
        }
        else
        {
            Debug.Log($"targetTranform is {targetTransform}.");
        }
    }


    private void UpdatePlayerDisplay()
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 모든 자식 Transform을 한 번에 가져옴 (비활성화 포함)
        Transform[] roomDisplayChildren = roomDisplay.GetComponentsInChildren<Transform>(true);
        Transform[] loveCardDisplayChildren = loveCardDisplay.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < maxPlayers/2; i++)
        {
            for(int j=0; j < 2; j++)
            {
                string gender = j == 0 ? "Male" : "Female";
                string playerName = $"{gender}{i + 1}";

                Transform sourceTransform = roomDisplayChildren.FirstOrDefault(t => t.name == playerName);
                Transform targetTransform = loveCardDisplayChildren.FirstOrDefault(t => t.name == playerName);

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

    public void SetActiveDisplay(string target, bool sign)
    {
        switch (target)
        {
            case "messageBtn":
                messageBtn.SetActive(sign);
                break;
            case "loveCardBtn":
                loveCardBtn.SetActive(sign);
                break;
            case "messageDisplay":
                messageDisplay.SetActive(sign);
                break;
            case "loveCardDisplay":
                loveCardDisplay.SetActive(sign);
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }
    }

    public GameObject GetPanel(string target)
    {
        GameObject gameObject = null;
        switch (target)
        {
            case "messageBtn":
                gameObject = messageBtn;
                break;
            case "loveCardBtn":
                gameObject = loveCardBtn;
                break;
            case "messageDisplay":
                gameObject = messageDisplay;
                break;
            case "loveCardDisplay":
                gameObject = loveCardDisplay;
                break;
            default:
                throw new ArgumentException("Invalid target specified: " + target);
        }

        return gameObject;
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
