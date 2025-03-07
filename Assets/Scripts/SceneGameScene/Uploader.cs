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
    private Loader loader;
    private CanvasManager canvasManager;

    private void Awake()
    {
        loader = FindObjectOfType<Loader>();
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

    // �� �ο��� ����
    public void UploadPlayerMaxNumber(string roomCode, int n)
    {
        DatabaseReference selectionRef = databaseReference
            .Child(roomCode)
            .Child("PlayerMaxNumber");

        selectionRef.SetValueAsync(n).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"{roomCode} room maximum number of players is {n}");
            }
            else
            {
                Debug.LogError($"Failed to update PlayerMaxNumber in Firebase: " + task.Exception);
            }
        });
    }

    // QuestionCanvas���� �÷��̾��� �亯 ������ ����.
    public void UploadAnswers(int[] answers)
    {
        // ���� �����͸� ����� ���, ���� ��Ʈ��ũ ������ ��� ���
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string roomCode = GameManager.Instance.GetRoomCode();

        // Dictionary�� ��ȯ�Ͽ� ������ ���ε�
        Dictionary<string, object> answerData = new Dictionary<string, object>();

        for (int i = 0; i < answers.Length; i++)
        {
            answerData[$"question_{i}"] = answers[i];
        }

        Debug.Log("UploadAnswers : " + roomCode + " " + actorNumber);
        // Firebase Realtime Database�� ActorNumber�� Ű�� ����Ͽ� ������ ����
        databaseReference
            .Child(roomCode)
            .Child("user_answers")
            .Child(actorNumber.ToString())
            .SetValueAsync(answerData).ContinueWith(task =>
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

    public void QuestionInDictonary(int questionIndex, string question)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child("QuestionDictionary")
            .Child(questionIndex.ToString())
            .Child("Question")
            .SetValueAsync(question).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Question updated successfully in Firebase QuestionDictonary.");
                }
                else
                {
                    Debug.LogError($"Failed to update Question in Firebase QuestionDictonary: " + task.Exception);
                }
            });
    }

    public void ResponseInDictonary(int questionIndex, int responseIndex, string response)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child("QuestionDictionary")
            .Child(questionIndex.ToString())
            .Child(responseIndex.ToString())
            .SetValueAsync(response).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"[Response] updated successfully in Firebase QuestionDictonary.");
                }
                else
                {
                    Debug.LogError($"Failed to update [Response] in Firebase QuestionDictonary: " + task.Exception);
                }
            });
    }

    //All Player Ready?
    public void UploadReadyCount(string type)    // 
    {
        // roomCode.type.ReadyCount �ش� ��ο� �غ�Ϸ�� ������� ���� ���
        // type <= {"MiniGame1", "MiniGame2", "GameEnd"}
        string roomCode = GameManager.Instance.GetRoomCode();

        databaseReference
            .Child(roomCode)
            .Child(type)
            .Child("ReadyCount")
            .RunTransaction(mutableData =>
            {
                
                int currentCount = 0;
                if (mutableData.Value != null)
                {
                    // ���� ���� ������ �� ���� ���
                    currentCount = int.Parse(mutableData.Value.ToString());
                }
                currentCount++;
                mutableData.Value = currentCount;

                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    loader.CheckAndNotifyEndOfReady(type);
                    Debug.Log($"{type} updated successfully in Firebase.");
                }
                else
                {
                    Debug.LogError($"Failed to update {type} in Firebase: " + task.Exception);
                }
            });
    }

    public void InitReadyCount(string type)
    {
        var roomRef = databaseReference.Child(GameManager.Instance.GetRoomCode()).Child(type).Child("ReadyCount");
        try
        {
            roomRef.SetValueAsync(0).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to set value in Firebase: {task.Exception}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error Find {e}");
        }
    }

    // MiniGame
    public void UploadReceivedVotesMG1(int targetActorNumber)  // Loader.FindTopScorers(gameType)
    {
        // roomCode.gameType2.ActorNumber.ReceivedVotes �ش� ��ο� targetActorNumber�� ���� ��ǥ ���� ����.
        // gameType <= {"MiniGame1", "MiniGame2"}
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� gameType�� ���� ���� ������Ʈ
        DatabaseReference scoreRef = databaseReference
            .Child(roomCode)
            .Child("MiniGame1")
            .Child(targetActorNumber.ToString())            
            .Child("ReceivedVotes");

        scoreRef.RunTransaction(mutableData =>
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
                    Debug.Log($"MiniGame1 updated successfully in Firebase.");
                }
                else
                {
                    Debug.LogError($"Failed to update MiniGame1 in Firebase: " + task.Exception);
                }
            });
    }   

    public void UploadSelectionMG1(int targetActorNumber) // Loader.FindTopScorerPredictors(gameType)
    {
        // roomCode.gameType.ActorNumber.Selection �ش� ��ο� ActorNumber�� ��ǥ�� �÷��̾�(targetActorNumber) ����.
        // gameType <= {"MiniGame1", "MiniGame2"}

        string roomCode = GameManager.Instance.GetRoomCode();

        DatabaseReference selectionRef = databaseReference
            .Child(roomCode)
            .Child("MiniGame1")
            .Child(PhotonNetwork.LocalPlayer.ActorNumber.ToString())
            .Child("Selection");

        Debug.Log($"selectionRef :: {selectionRef}");
        Debug.Log($"targetActorNumber :: {targetActorNumber}");
        Debug.Log($"gameType :: MiniGame1");

        selectionRef.SetValueAsync(targetActorNumber).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"MiniGame1 Selection updated successfully in Firebase for actor {targetActorNumber}.");
            }
            else
            {
                Debug.LogError($"Failed to update selection in Firebase: " + task.Exception);
            }
        });
    }

    public void UploadVotedCount(string gameType)    // CheckAndNotifyEndOfVoting(gameType)
    {
        // roomCode.MiniGame1.VotedCount �ش� ��ο� ��ǥ�� ������ �ο��� ��ŭ ������Ŵ.
        // gameType <= {"MiniGame1", "MiniGame2"}

        string roomCode = GameManager.Instance.GetRoomCode();

        databaseReference
            .Child(roomCode)
            .Child(gameType)
            .Child("VotedCount")
            .RunTransaction(mutableData =>
            {
                int currentCount = 0;
                if (mutableData.Value != null)
                {
                    // ���� ���� ������ �� ���� ���
                    currentCount = int.Parse(mutableData.Value.ToString());
                }
                currentCount++;
                mutableData.Value = currentCount;
                DebugCanvas.Instance.DebugLog($"---------------UploadVotedCount currentCount: {currentCount}");
                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    loader.CheckAndNotifyEndOfVoting(gameType);
                    Debug.Log($"{gameType} updated successfully in Firebase.");
                }
                else
                {
                    Debug.LogError($"Failed to update {gameType} in Firebase: " + task.Exception);
                }
            });
    }

    public void UploadPlayerChoiceMG2(int index)  // FindChoiceAndPlayer()
    {
        // roomCode.MiniGame2.index �ش� ��ο� �ش� �ε����� �������� ������ �÷��̾��� ActorNumber�� ����.
        // index <= {0,1}
        string roomCode = GameManager.Instance.GetRoomCode();
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // Firebase ��ο��� gameType�� ���� ���� ������Ʈ
        DatabaseReference scoreRef = databaseReference
            .Child(roomCode)
            .Child("MiniGame2")
            .Child(index.ToString());

        scoreRef.RunTransaction(mutableData =>
        {
            List<object> players = mutableData.Value as List<object>;
            if (players == null)
            {
                players = new List<object>();
            }

            // ���� �÷��̾��� ActorNumber�� ����Ʈ�� ���ٸ� �߰�
            if (!players.Contains(actorNumber))
            {
                players.Add(actorNumber);
            }

            mutableData.Value = players;
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Player {actorNumber} choice for MiniGame2 updated successfully in Firebase.");
            }
            else
            {
                Debug.LogError($"Failed to update player choice for MiniGame2 in Firebase: " + task.Exception);
            }
        });
    }

    // Secret Message
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


    // About Timer
    public void SetStartTime()
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        DatabaseReference timerRef = databaseReference
            .Child(roomCode)
            .Child("StartTime");

        // ���� UTC �ð��� ���� �ð����� �����մϴ�.
        string startTime = DateTime.UtcNow.ToString();

        // Ÿ�̸� ��忡 ���� �ð��� �����մϴ�.
        timerRef.SetValueAsync(startTime).ContinueWith(task => {
            if (task.IsFaulted)
            {
                // ���� ó��
                Debug.LogError("Error setting timer start time: " + task.Exception);
            }
            else
            {
                // ���������� �ð� ����
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
                    foreach (Player player in PhotonNetwork.PlayerList)
                    {
                        photonView.RPC("AsyncTimer", player);
                    }
                });
            }
        });
    }

    [PunRPC]
    void AsyncTimer()
    {
        /*
        if (!PhotonNetwork.IsMasterClient)�̰� �� �ȵɱ�?
        {
            loader.GetStartTime(); 
        }
        */
        loader.GetStartTime();
    }

    // About Reward.
    public void PartnerActorNumber(int actorNumber, int partnerActorNumber) // �÷��̾���� ��Ʈ�� ���� ����.
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child(actorNumber.ToString())
            .Child("Partner")
            .SetValueAsync(partnerActorNumber).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to set value in Firebase: {task.Exception}");
                }
            });
    }

    public void SignReceivedPartnerInfo(int actorNumber, string newSignReceivedPartnerInfo)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase ��ο��� scoreType�� ���� ���� ������Ʈ
        databaseReference
            .Child(roomCode)
            .Child(actorNumber.ToString())
            .Child("SignReceivedPartnerInfo")
            .SetValueAsync(newSignReceivedPartnerInfo).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to set value in Firebase: {task.Exception}");
                }
            });
    }
}
