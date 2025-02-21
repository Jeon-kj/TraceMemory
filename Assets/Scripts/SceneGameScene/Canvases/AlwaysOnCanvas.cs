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
    public GameObject messagePrefab; // �޽��� ������
    public GameObject roomDisplay;
    public Transform contentTransform; // �޽����� �߰��� Content ����
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

        // ������ ��ܿ� ��ġ�� ȣ��ī�� ������ ������ �÷��̾� �̹��� ������Ʈ. (�÷��̾� ������ ��������)
        UpdatePlayerDisplay(); 
        
        // �÷��̾ ���� �޽������� �������̽��� �߰��ϰ�, ������ ���� �޽��� �̺�Ʈ�� ���ؼ��� ���� �۾��� ��.
        MonitorScoreOrMessage(targetActorNumber, "SecretMessage");

        // ���� �� ���� �ִ� ��� �÷��̾���� ȣ��ī�������� 
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

        // SecretMessage�� ��쿡�� ������ �����ϴ� ������ �ϰ� ������Ʈ�Ѵ�. ���� ������ ���ο� �����鿡 ���ؼ��� �ڵ鸵�Ѵ�.
        // LoveCard�� ���� ���� �ԷµǴ� ������ ���ؼ��� �ڵ鸵�� �Ѵ�.
        // �� �̷� ���̸� ����? ����� �ȳ���. AlwaysOnCanvas�� ���� ��� ���ĺ��� ���� �ֱ� ������, �޽��� ���۰� ȣ��ī�� ���ۺ��� ���� ����ȴ�.
        // �׷��� ������ �̷л� �ϰ� ������Ʈ�� �ʿ� �����ٵ�.. ������ ����� �ٽ� �����ϴ� ��츦 ����ϴ� �ǰ�?
        // -> ���� �ǵ��� Uploader���� Loader�� ���� ����Ǿ� reference ��ΰ� ��ȿ���� ���� ��츦 ����ϱ� ���� ���� ������,
        // -> ChildAdded �̺�Ʈ �����ʴ� �ʿ� ������ ������. �׷��� ValueChanged�� ��� ���� �Էµ� ������ �ν����� ���ϱ⿡ �߰��� �ʿ並 ����.
        if (Type == "LoveCardScore")
        {
            // LoveCardScore ��θ� Ȯ���ϰ� �ʱ�ȭ
            loader.EnsurePath(targetActorNumber, "LoveCardScore", () =>
            {
                // ��ΰ� Ȯ�εǾ��ų� ������ �� ���� ���� ������ ���
                reference.ValueChanged += (sender, e) => loader.HandleScoreChanged(sender, e, targetActorNumber);
            });
        }
        else if(Type == "SecretMessage")
        {
            loader.EnsurePath(targetActorNumber, "SecretMessage", () =>
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

    public void OnScoreChanged(int targetActorNumber, int updatedScore)
    {
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        Transform[] loveCardDisplayChildren = loveCardDisplay.GetComponentsInChildren<Transform>(true);
        Transform targetTransform = loveCardDisplayChildren.FirstOrDefault(t => 
            t.name == "PlayerActorNumber" && t.GetComponent<Text>().text == targetActorNumber.ToString()); // GetComponent ���ɹ���.
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

        // ��� �ڽ� Transform�� �� ���� ������ (��Ȱ��ȭ ����)
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
        // ���� �г� Ȱ��ȭ/��Ȱ��ȭ
        if (displayToToggle != null)
        {
            displayToToggle.SetActive(!displayToToggle.activeSelf);
        }

        // �ٸ� �г��� Ȱ��ȭ�Ǿ� ������ ��Ȱ��ȭ
        if (otherDisplay != null && otherDisplay.activeSelf)
        {
            otherDisplay.SetActive(false);
        }
    }
}
