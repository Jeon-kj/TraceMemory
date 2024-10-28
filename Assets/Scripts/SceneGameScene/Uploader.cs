using Firebase.Storage;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Firebase.Database;
using System.Collections.Generic;
using Photon.Pun;

public class Uploader : MonoBehaviour
{
    private FirebaseStorage storage;
    private DatabaseReference databaseReference;

    async void Start()
    {
        // Firebase 초기화가 완료될 때까지 대기
        await FirebaseManager.Instance.WaitForFirebaseInitialized();

        // 초기화가 완료된 후에 storage를 설정
        storage = FirebaseManager.Instance.storage;
        databaseReference = FirebaseManager.Instance.database.RootReference;
    }

    public async Task<bool> UploadImage(byte[] imageBytes, string fileName)
    {
        StorageReference storageRef = storage.RootReference;
        StorageReference imageRef = storageRef.Child("images/" + fileName);

        try
        {
            // 이미지를 업로드하고, 작업이 완료된 후 결과를 반환
            await imageRef.PutBytesAsync(imageBytes);
            Debug.Log("Upload successful");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }
    }


    public void GetImageUrl(string fileName, System.Action<string> onUrlReceived)
    {
        StorageReference storageRef = storage.RootReference;
        StorageReference imageRef = storageRef.Child("images/" + fileName);

        imageRef.GetDownloadUrlAsync().ContinueWith((Task<System.Uri> task) => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                System.Uri downloadUrl = task.Result;
                onUrlReceived(downloadUrl.ToString());
            }
            else
            {
                Debug.LogError(task.Exception.ToString());
            }
        });
    }

    // QuestionCanvas에서 플레이어의 답변 서버에 저장.
    public void UploadAnswers(int[] answers, int mockActorNumber = -1, string mockRoomCode = null)
    {
        // 모의 데이터를 사용할 경우, 실제 네트워크 데이터 대신 사용
        int actorNumber = mockActorNumber == -1 ? PhotonNetwork.LocalPlayer.ActorNumber : mockActorNumber;
        string roomCode = string.IsNullOrEmpty(mockRoomCode) ? GameManager.Instance.GetRoomCode() : mockRoomCode;

        // Dictionary로 변환하여 데이터 업로드
        Dictionary<string, object> answerData = new Dictionary<string, object>();

        for (int i = 0; i < answers.Length; i++)
        {
            answerData[$"question_{i}"] = answers[i];
        }

        Debug.Log("UploadAnswers : " + roomCode + " " + actorNumber);
        // Firebase Realtime Database에 ActorNumber를 키로 사용하여 데이터 저장
        databaseReference.Child(roomCode).Child("user_answers").Child(actorNumber.ToString()).SetValueAsync(answerData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Answers uploaded successfully.");
            }
            else
            {
                Debug.LogError("Error uploading answers: " + task.Exception);
            }
        });
    }

    public void UploadScore(string scoreType, int targetActorNumber)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase 경로에서 scoreType에 따라 점수 업데이트
        databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(scoreType)
            .RunTransaction(mutableData =>
        {
            int currentCount = 0;
            if (mutableData.Value != null)
            {
                // 기존 값이 있으면 그 값을 사용
                currentCount = int.Parse(mutableData.Value.ToString());
            }
            // 1을 더함
            currentCount++;
            mutableData.Value = currentCount;

            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"{scoreType} updated successfully in Firebase.");
            }
            else
            {
                Debug.LogError($"Failed to update {scoreType} in Firebase: " + task.Exception);
            }
        });
    }

    public void UploadSecretMessage(int targetActorNumber, string message)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase 경로에서 scoreType에 따라 점수 업데이트
        DatabaseReference newMessageRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("SecretMessage")
            .Push();

        newMessageRef.SetValueAsync(message).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("SecretMessage uploaded successfully in Firebase.");
            }
            else
            {
                Debug.LogError($"Failed to upload SecretMessage in Firebase: {task.Exception}");
            }
        });
    }
}
