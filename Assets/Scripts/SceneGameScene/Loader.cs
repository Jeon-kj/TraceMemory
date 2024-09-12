using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using System;
using Firebase.Database;
using System.Collections.Generic;

public class Loader : MonoBehaviour
{
    Uploader Uploader;
    private DatabaseReference databaseReference;

    private void Awake()
    {
        Uploader = GetComponent<Uploader>();        
    }

    async void Start()
    {
        await FirebaseManager.Instance.WaitForFirebaseInitialized();
        databaseReference = FirebaseManager.Instance.database.RootReference;
    }

    public void LoadPlayerImage(Image targetImage, string fileName)
    {
        // Firebase���� URL ��������
        Uploader.GetImageUrl(fileName, (imageUrl) =>
        {
            // Firebase�κ��� ������ URL�� ����Ͽ� �̹����� �ε�
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                // URL�� �޾ƿ� �� �̹����� �ε�
                LoadImageFromUrl(targetImage, imageUrl);
            });
        });
    }

    // URL���� �̹����� �ε��ϴ� �޼���
    public void LoadImageFromUrl(Image targetImage, string imageUrl)
    {
        try
        {
            StartCoroutine(DownloadImage(targetImage, imageUrl));
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred: " + ex.Message);
        }
    }

    // �ڷ�ƾ�� ���� URL���� �̹����� �ٿ�ε��ϰ� UI�� ����
    private IEnumerator DownloadImage(Image targetImage, string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            // UI �۾��� ���� ������� ����
            targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    // ������ ������ QuestionCanvas�� ���� �� �ҷ�����.
    public void LoadAnswers(int actorNumber, Action<int[]> onAnswersLoaded)
    {
        string roomCode = GameManager.Instance.GetRoomCode();
        // Firebase ��ο��� ActorNumber�� �����͸� ������
        databaseReference.Child(roomCode).Child("user_answers").Child(actorNumber.ToString()).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                List<int> answersList = new List<int>();

                foreach (var child in snapshot.Children)
                {
                    int answer = int.Parse(child.Value.ToString());
                    answersList.Add(answer);
                }

                // �ҷ��� answers �迭�� �ݹ����� ����
                onAnswersLoaded(answersList.ToArray());
            }
            else
            {
                Debug.LogError("Error loading answers: " + task.Exception);
            }
        });
    }

    public void LoadFirstImpressionScore(string scoreType, int targetActorNumber, Action<int> onFirstImpressionScoreLoaded)
    {
        string roomCode = GameManager.Instance.GetRoomCode();
        // Firebase ��ο��� ActorNumber�� �����͸� ������
        databaseReference.Child(roomCode).Child(targetActorNumber.ToString()).Child(scoreType).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    int firstImpressionScore = int.Parse(task.Result.Value.ToString());

                    onFirstImpressionScoreLoaded(firstImpressionScore);
                }
                else
                {
                    Debug.LogError("FirstImpressionScore does not exist for ActorNumber: " + targetActorNumber);
                    onFirstImpressionScoreLoaded(0);  // ���� ���� ��� �⺻�� 0�� ����
                }
            }
            else
            {
                Debug.LogError($"Error loading {scoreType}: " + task.Exception);
            }
        });
    }
}

