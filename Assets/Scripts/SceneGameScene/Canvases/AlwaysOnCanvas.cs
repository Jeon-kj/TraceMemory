using Firebase.Database;
using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnCanvas : MonoBehaviour
{
    public GameObject messagePrefab; // 메시지 프리팹
    public Transform contentTransform; // 메시지를 추가할 Content 영역
    public Transform loveCardDisplay;
    public Transform roomDisplay;

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
        UpdatePlayerDisplay();
        MonitorScoreOrMessage(targetActorNumber, "SecretMessage");
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

        if(Type == "LoveCardScore") reference.ValueChanged += (sender, e) => loader.HandleScoreChanged(sender, e, targetActorNumber);
        else if(Type == "SecretMessage")
        {
            loader.EnsureSecretMessagePath(targetActorNumber, () =>
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

    public void OnScoreChanged(int updatedScore)
    {
        // 점수 변화에 따른 원하는 동작 처리
        Debug.Log($"Score has been updated to: {updatedScore}");

        // 추가적으로 점수 변화에 따른 로직을 여기에 구현하세요
    }


    private void UpdatePlayerDisplay()
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 모든 자식 Transform을 한 번에 가져옴 (비활성화 포함)
        Transform[] roomDisplayChildren = roomDisplay.GetComponentsInChildren<Transform>(true);
        Transform[] loveCardDisplayChildren = loveCardDisplay.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < 6; i++)
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
}
