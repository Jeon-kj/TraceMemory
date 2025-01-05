using Firebase.Storage;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Firebase.Database;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;

public class Uploader : MonoBehaviourPunCallbacks
{
    private FirebaseStorage storage;
    private DatabaseReference databaseReference;
    private CanvasManager canvasManager;

    private void Awake()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
    }

    async void Start()
    {
        // Firebase �ʱ�ȭ�� �Ϸ�� ������ ���
        await FirebaseManager.Instance.WaitForFirebaseInitialized();

        // �ʱ�ȭ�� �Ϸ�� �Ŀ� storage�� ����
        storage = FirebaseManager.Instance.storage;
        databaseReference = FirebaseManager.Instance.database.RootReference;
    }

    public async Task<bool> UploadImage(byte[] imageBytes, string fileName)
    {
        StorageReference storageRef = storage.RootReference;
        StorageReference imageRef = storageRef.Child("images/" + fileName);

        try
        {
            // �̹����� ���ε��ϰ�, �۾��� �Ϸ�� �� ����� ��ȯ
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

    // QuestionCanvas���� �÷��̾��� �亯 ������ ����.
    public void UploadAnswers(int[] answers, int mockActorNumber = -1, string mockRoomCode = null)
    {
        // ���� �����͸� ����� ���, ���� ��Ʈ��ũ ������ ��� ���
        int actorNumber = mockActorNumber == -1 ? PhotonNetwork.LocalPlayer.ActorNumber : mockActorNumber;
        string roomCode = string.IsNullOrEmpty(mockRoomCode) ? GameManager.Instance.GetRoomCode() : mockRoomCode;

        // Dictionary�� ��ȯ�Ͽ� ������ ���ε�
        Dictionary<string, object> answerData = new Dictionary<string, object>();

        for (int i = 0; i < answers.Length; i++)
        {
            answerData[$"question_{i}"] = answers[i];
        }

        Debug.Log("UploadAnswers : " + roomCode + " " + actorNumber);
        // Firebase Realtime Database�� ActorNumber�� Ű�� ����Ͽ� ������ ����
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

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(scoreType)
            .RunTransaction(mutableData =>
        {
            int currentCount = 0;
            if (mutableData.Value != null)
            {
                // ���� ���� ������ �� ���� ���
                currentCount = int.Parse(mutableData.Value.ToString());
            }
            // 1�� ����
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

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
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

    public void UploadVoting(string scoreType)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child(scoreType)
            .RunTransaction(mutableData =>
            {
                int currentCount = 0;
                if (mutableData.Value != null)
                {
                    // ���� ���� ������ �� ���� ���
                    currentCount = int.Parse(mutableData.Value.ToString());
                }
                // 1�� ����
                currentCount++;
                mutableData.Value = currentCount;

                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                Debug.Log("UploadVoting continueWith check in");
                if (task.IsCompleted)
                {
                    Debug.Log("UploadVoting continueWith check in task Is Completed");
                    CheckAndNotifyEndOfVoting(scoreType);
                    Debug.Log($"{scoreType} updated successfully in Firebase.");
                }
                else
                {
                    Debug.LogError($"Failed to update {scoreType} in Firebase: " + task.Exception);
                }
            });
    }

    private void CheckAndNotifyEndOfVoting(string scoreType)
    {
        Debug.Log("CheckAndNotifyEndOfVoting check in task Is Completed");
        var roomRef = databaseReference.Child(GameManager.Instance.GetRoomCode()).Child(scoreType);

        roomRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int currentCount = int.Parse(snapshot.Value.ToString());
                Debug.Log($"currentCount : {currentCount}");
                if (currentCount == GameManager.Instance.GetPlayerMaxNumber())
                {

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        if (photonView == null)
                        {
                            Debug.LogError("PhotonView is not assigned!");
                            return;
                        }

                        foreach (Player player in PhotonNetwork.PlayerList)
                        {
                            try
                            {
                                photonView.RPC("ReceiveVotingEndSign", player);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error: {e.Message}");
                            }
                        }
                    });
                }
            }
        });
    }

    [PunRPC]
    void ReceiveVotingEndSign()
    {
        Debug.Log("ReceiveVotingEndSign() check in task Is Completed");
        if (!canvasManager || !canvasManager.MiniGame1)
        {
            Debug.LogError("CanvasManager or MiniGame1 component is not initialized!");
            return;
        }
        canvasManager.MiniGame1.GetComponent<MiniGame1>().VotingEnd();
    }
}
