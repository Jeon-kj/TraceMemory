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
        // Firebase에서 URL 가져오기
        Uploader.GetImageUrl(fileName, (imageUrl) =>
        {
            // Firebase로부터 가져온 URL을 사용하여 이미지를 로드
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                // URL을 받아온 뒤 이미지를 로드
                LoadImageFromUrl(targetImage, imageUrl);
            });
        });
    }

    // URL에서 이미지를 로드하는 메서드
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

    // 코루틴을 통해 URL에서 이미지를 다운로드하고 UI에 적용
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

            // UI 작업을 메인 스레드로 전달
            targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    // 서버에 저장한 QuestionCanvas에 대한 답 불러오기.
    public void LoadAnswers(int actorNumber, Action<int[]> onAnswersLoaded)
    {
        string roomCode = GameManager.Instance.GetRoomCode();
        // Firebase 경로에서 ActorNumber로 데이터를 가져옴
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

                // 불러온 answers 배열을 콜백으로 전달
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
        // Firebase 경로에서 ActorNumber로 데이터를 가져옴
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
                    onFirstImpressionScoreLoaded(0);  // 값이 없을 경우 기본값 0을 전달
                }
            }
            else
            {
                Debug.LogError($"Error loading {scoreType}: " + task.Exception);
            }
        });
    }



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
                // 메시지를 모두 불러왔으므로 콜백 실행
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
            alwaysOnCanvas.AddMessageToContent(newMessage);
        }
        else
        {
            Debug.LogError("Snapshot does not exist or is null.");
        }
    }

    public void EnsureSecretMessagePath(int targetActorNumber, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase에서 경로 참조
        DatabaseReference messageRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("SecretMessage");

        // 경로가 존재하는지 확인
        messageRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // 경로가 존재하지 않으면 기본값을 설정해서 경로를 생성
                    messageRef.SetValueAsync("Initial Message").ContinueWith(setTask =>
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log("SecretMessage path created with initial message.");
                            onPathCreated?.Invoke(); // 경로가 생성되었음을 알림
                        }
                        else
                        {
                            Debug.LogError($"Failed to create SecretMessage path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // 경로가 이미 존재하면 콜백을 바로 호출
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check SecretMessage path: {task.Exception}");
            }
        });
    }



    // About LoveCard (In AlwaysOnCanvas)
    public void HandleScoreChanged(object sender, ValueChangedEventArgs e, int targetActorNumber)
    {
        if (e.Snapshot.Exists && e.Snapshot.Value != null)
        {
            int updatedScore = int.Parse(e.Snapshot.Value.ToString());
            Debug.Log($"{e.Snapshot.Key} score changed: {updatedScore}");

            // 점수 업데이트에 따른 추가 로직
            alwaysOnCanvas.OnScoreChanged(updatedScore);
        }
        else
        {
            Debug.LogWarning("Score data does not exist or is null.");
        }
    }
}

