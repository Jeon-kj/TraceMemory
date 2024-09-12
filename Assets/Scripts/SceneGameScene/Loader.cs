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
}

