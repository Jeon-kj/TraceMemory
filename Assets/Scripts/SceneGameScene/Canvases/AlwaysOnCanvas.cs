using Firebase.Database;
using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnCanvas : MonoBehaviour
{
    public GameObject messagePrefab; // �޽��� ������
    public Transform contentTransform; // �޽����� �߰��� Content ����
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

        // ���� ��� ����
        DatabaseReference reference = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(Type);

        if(Type == "LoveCardScore") reference.ValueChanged += (sender, e) => loader.HandleScoreChanged(sender, e, targetActorNumber);
        else if(Type == "SecretMessage")
        {
            loader.EnsureSecretMessagePath(targetActorNumber, () =>
            {
                // ��ΰ� �����ϰų� ������ �� �޽��� ���� ����
                loader.LoadAllMessages(reference, () =>
                {
                    // �ǽð� �޽��� �߰�
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
        // �������� �����ϰ� Content ������ �߰�
        GameObject newMessageObject = Instantiate(messagePrefab, contentTransform);

        // ������ ���� Text ������Ʈ�� ã�� �޽����� ����
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
        // ���� ��ȭ�� ���� ���ϴ� ���� ó��
        Debug.Log($"Score has been updated to: {updatedScore}");

        // �߰������� ���� ��ȭ�� ���� ������ ���⿡ �����ϼ���
    }


    private void UpdatePlayerDisplay()
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // ��� �ڽ� Transform�� �� ���� ������ (��Ȱ��ȭ ����)
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

                // PlayerName ����
                Text targetNameText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                Text sourceNameText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerName").GetComponent<Text>();
                if (targetNameText != null && sourceNameText != null)
                    targetNameText.text = sourceNameText.text;


                // PlayerActorNumber ����
                Text targetActorNumberText = targetTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                Text sourceActorNumberText = sourceTransform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "PlayerActorNumber").GetComponent<Text>();
                if (targetActorNumberText != null && sourceActorNumberText != null)
                    targetActorNumberText.text = sourceActorNumberText.text;

                // ImageSource ����
                Transform maskTransform; // �̷��� ���� : FirstOrDefault(t => t.name == "Mask/ImageSource") �δ� ã�� ���� ������. �ڽİ� �θ�� �ν����� ���ϰ� �̸� �� ��ü�� �ν���.
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
