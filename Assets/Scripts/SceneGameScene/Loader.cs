using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using System;
using Firebase.Database;
using System.Collections.Generic;
using Photon.Pun;

public class Loader : MonoBehaviour
{
    private DatabaseReference databaseReference;

    Uploader Uploader;
    CanvasManager canvasManager;
    AlwaysOnCanvas alwaysOnCanvas;

    private void Awake()
    {
        Uploader = GetComponent<Uploader>();
        canvasManager = FindObjectOfType<CanvasManager>();
        alwaysOnCanvas = canvasManager.AlwaysOnCanvas.GetComponent<AlwaysOnCanvas>();
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

    /*public void LoadFirstImpressionScore(string scoreType, int targetActorNumber, Action<int> onFirstImpressionScoreLoaded)
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
    }*/



    // About Message (In AlwaysOnCanvas)
    public void LoadAllMessages(DatabaseReference messageRef, Action onMessagesLoaded)
    {
        messageRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists && snapshot.HasChildren)
                {
                    foreach (DataSnapshot childSnapshot in snapshot.Children)
                    {
                        string message = childSnapshot.Value.ToString();
                        alwaysOnCanvas.AddMessageToContent(message);
                    }
                }
                // �޽����� ��� �ҷ������Ƿ� �ݹ� ����
                onMessagesLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load messages from Firebase: {task.Exception}");
            }
        });
    }

    public void HandleNewMessageAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot.Exists && e.Snapshot.Value != null)
        {
            string newMessage = e.Snapshot.Value.ToString();
            Debug.Log($"{e.Snapshot.Key} Message updated: {newMessage}");
            alwaysOnCanvas.AddMessageToContent(newMessage);
        }
        else
        {
            Debug.LogError("Snapshot does not exist or is null.");
        }
    }

    /*public void EnsureSecretMessagePath(int targetActorNumber, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ��� ����
        DatabaseReference messageRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("SecretMessage");

        // ��ΰ� �����ϴ��� Ȯ��
        messageRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    messageRef.SetValueAsync("Initial Message").ContinueWith(setTask =>
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log("SecretMessage path created with initial message.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create SecretMessage path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check SecretMessage path: {task.Exception}");
            }
        });
    }*/



    // About LoveCard (In AlwaysOnCanvas)
    public void HandleScoreChanged(object sender, ValueChangedEventArgs e, int targetActorNumber)
    {
        if (e.Snapshot.Exists && e.Snapshot.Value != null)
        {
            int updatedScore = int.Parse(e.Snapshot.Value.ToString());
            Debug.Log($"{e.Snapshot.Key} score changed: {updatedScore}");

            // ���� ������Ʈ�� ���� �߰� ����
            alwaysOnCanvas.OnScoreChanged(targetActorNumber, updatedScore);
        }
        else
        {
            Debug.LogWarning("Score data does not exist or is null.");
        }
    }

    /*public void EnsureLoveCardScorePath(int targetActorNumber, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ���� ��� ����
        DatabaseReference scoreRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("LoveCardScore");

        // ��ΰ� �����ϴ��� Ȯ��
        scoreRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    scoreRef.SetValueAsync(0).ContinueWith(setTask => // ������ 0���� �ʱ�ȭ
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log("LoveCardScore path created with initial score.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create LoveCardScore path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check LoveCardScore path: {task.Exception}");
            }
        });
    }*/

    public void EnsurePath(int targetActorNumber, string type, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ���� ��� ����
        DatabaseReference newRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(type);

        // ��ΰ� �����ϴ��� Ȯ��
        newRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    newRef.SetValueAsync(type=="LoveCardScore" ? 0 : "Initial Message").ContinueWith(setTask => // ������ 0���� �ʱ�ȭ
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log($"{type} path created.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create {type} path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    //Debug.Log($"{type} path already exist.");
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check {type} path: {task.Exception}");
            }
        });
    }


    // About MiniGame1 (roodCode.MiniGame1.ActorNumber)
    public async Task<(List<int> topScorers, int highestScore)> FindTopScorersAsync(string scoreType)
    {
        Debug.Log($"Fetching top scorers for score type: {scoreType}");
        string roomCode = GameManager.Instance.GetRoomCode();

        int highestScore = 0;
        List<int> topScorers = new List<int>();

        foreach (var one in PhotonNetwork.PlayerList)
        {
            var scoreRef = databaseReference.Child(roomCode).Child(one.ActorNumber.ToString()).Child(scoreType);
            DataSnapshot snapshot = await scoreRef.GetValueAsync();
            Debug.Log($"scoreRef : {scoreRef}");

            if (snapshot.Exists)
            {
                Debug.Log($"Key: {snapshot.Key}, Value: {snapshot.Value.ToString()}");
                int score = int.Parse(snapshot.Value.ToString());
                if (score > highestScore)
                {
                    highestScore = score;
                    topScorers.Clear();
                    topScorers.Add(one.ActorNumber);
                }
                else if (score == highestScore)
                {
                    topScorers.Add(one.ActorNumber);
                }
            }
        }
        Debug.Log($"topScorers : {topScorers}");
        Debug.Log($"highestScore : {highestScore}");
        return (topScorers, highestScore);
        /*
        try
        {
            DataSnapshot snapshot = await roomRef.GetValueAsync();

            if (snapshot.Exists)
            {
                int highestScore = 0;
                List<int> topScorers = new List<int>();

                foreach (DataSnapshot playerSnapshot in snapshot.Children)
                {
                    if (playerSnapshot.HasChild(scoreType))
                    {
                        int score = int.Parse(playerSnapshot.Child(scoreType).Value.ToString());
                        int actorNumber = int.Parse(playerSnapshot.Key);

                        if (score > highestScore)
                        {
                            highestScore = score;
                            topScorers.Clear();
                            topScorers.Add(actorNumber);
                        }
                        else if (score == highestScore)
                        {
                            topScorers.Add(actorNumber);
                        }
                    }
                }
                Debug.Log($"topScorers : {topScorers}");
                Debug.Log($"highestScore : {highestScore}");
                return (topScorers, highestScore);
            }
            else
            {
                throw new Exception("Failed to retrieve scores");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error retrieving scores for room {roomCode} with type {scoreType}: {ex.Message}");
            throw;
        }*/
    }
}

